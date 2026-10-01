using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using PassKee.Business.Clients.Smtp;
using PassKee.Business.Common.Constants.Http;
using PassKee.Business.Services.Http;
using PassKee.Business.Services.Queue;
using PassKee.Business.Testing.Factories;
using PassKee.Business.Testing.Seeders.Entity;
using PassKee.Business.Testing.Services;
using PassKee.Orm.Constants;
using PassKee.Orm.Dao;
using PassKee.Orm.Entities;
using Persistence.Transactions.Behaviors;
using Xunit;

namespace PassKee.Tests.Integration.Api.Core;

public class BaseTest : IClassFixture<ApiCustomWebApplicationFactory>, IDisposable
{
    protected readonly ApiCustomWebApplicationFactory _factory;
    protected IServiceScope ServiceScope;
    
    protected readonly IServiceProvider ServiceProvider;
    protected readonly HttpClient HttpClient;
    protected readonly IDbSessionProvider DbSessionProvider;
    protected readonly IUserSeeder UserSeeder;
    protected readonly IDataFactory<UserEntity> UserFactory;
    private readonly IDbCleanUpService _dbCleanUpService;
    protected readonly IQueueDao _queueDao;
    protected readonly IQueueService _queueService;
    protected readonly IHttpCookiesService CookiesService;
    protected readonly SmtpClientServiceMock SmtpClientServiceMock;

    public BaseTest(ApiCustomWebApplicationFactory factory)
    {
        _factory = factory;
        HttpClient = _factory.CreateClient();
        
        ServiceScope = _factory.Services.CreateScope();
        ServiceProvider = ServiceScope.ServiceProvider;
        
        DbSessionProvider = ServiceProvider.GetRequiredService<IDbSessionProvider>();
        _dbCleanUpService = ServiceProvider.GetRequiredService<IDbCleanUpService>();
        UserSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        UserFactory = ServiceProvider.GetRequiredService<IDataFactory<UserEntity>>();
        _queueDao = ServiceProvider.GetRequiredService<IQueueDao>();
        _queueService = ServiceProvider.GetRequiredService<IQueueService>();
        CookiesService = ServiceProvider.GetRequiredService<IHttpCookiesService>();
        SmtpClientServiceMock = (ServiceProvider.GetRequiredService<ISmtpClientService>() as SmtpClientServiceMock)!;

        _dbCleanUpService.CleanUp().Wait();
    }

    public void Dispose()
    {
        ServiceScope.Dispose();
        GC.SuppressFinalize(this);
    }

    protected async Task FlushDbChanges(bool isClearSession = false)
    { 
        await DbSessionProvider.CurrentSession.FlushAsync();
        if (isClearSession)
        {
            DbSessionProvider.CurrentSession.Clear();
        }
    }
    
    protected async Task RefreshEntity(object obj)
    {
        await DbSessionProvider.CurrentSession.RefreshAsync(obj);
    }
    
    protected async Task FlushAndRefreshEntity(object obj, bool isClearSession = false)
    {
        await FlushDbChanges(isClearSession);
        await DbSessionProvider.CurrentSession.RefreshAsync(obj);
    }
    
    protected async Task<int> QueueProcess(QueueChannel channel)
    {
        await FlushDbChanges();
        await _queueDao.Flush();
        await _queueDao.UpdateProcessAtForPending();
        return await _queueService.ProcessAsync(channel, isClearSessionForEachIteration: false);
    }
    
    #region Http
    public async Task<HttpResponseMessage> PostRequestAsAnonymousAsync(string url, object? data = null)
    {
        await FlushDbChanges();

        var requestData = JsonContent.Create(data ?? new { });
        return await HttpClient.PostAsync(url, requestData);
    }
        
    public async Task<HttpResponseMessage> PostRequestAsync(string url, string jwtToken, object? data = null)
    {
        await FlushDbChanges();

        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        var requestData = JsonContent.Create(data ?? new {});
        return await HttpClient.PostAsync(url, requestData);
    }
        
    public async Task<HttpResponseMessage> GetRequestAsAnonymousAsync(
        string url,
        Dictionary<string, string?>? urlParams = null
    )
    {
        urlParams ??= new Dictionary<string, string?>();
        var uri = new Uri(QueryHelpers.AddQueryString(url, urlParams), UriKind.Relative);
        await FlushDbChanges();

        return await HttpClient.GetAsync(uri);
    }
        
    public async Task<HttpResponseMessage> GetRequestAsync(string url, string jwtToken, Dictionary<string, string?>? urlParams = null)
    {
        await FlushDbChanges();

        urlParams ??= new Dictionary<string, string?>();
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        HttpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
        HttpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "text/json");
        
        var uri = new Uri(QueryHelpers.AddQueryString(url, urlParams), UriKind.Relative);
        return await HttpClient.GetAsync(uri);
    }

    public async Task<HttpResponseMessage> PutRequestAsync(string url, string jwtToken, object? data = null)
    {
        await FlushDbChanges();

        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        var requestData = JsonContent.Create(data ?? new { });
        return await HttpClient.PutAsync(url, requestData);
    }

    public async Task<HttpResponseMessage> DeleteRequestAsync(string url, string jwtToken)
    {
        await FlushDbChanges();

        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        return await HttpClient.DeleteAsync(url);
    }

    public async Task<HttpResponseMessage> GetRequestWithCookieAsync(
        string url,
        string cookieName,
        string cookieValue,
        Dictionary<string, string?>? urlParams = null
    )
    {
        await FlushDbChanges();

        urlParams ??= new Dictionary<string, string?>();
        var uri = new Uri(QueryHelpers.AddQueryString(url, urlParams), UriKind.Relative);
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Add("Cookie", $"{cookieName}={Uri.EscapeDataString(cookieValue)}");
        request.Headers.Add(HeaderNames.Accept, "application/json");

        return await HttpClient.SendAsync(request);
    }

    public async Task<HttpResponseMessage> PostRequestWithCookieAsync(
        string url,
        string cookieName,
        string cookieValue,
        object? data = null
    )
    {
        await FlushDbChanges();

        var requestData = JsonContent.Create(data ?? new { });
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = requestData
        };
        request.Headers.Add("Cookie", $"{cookieName}={Uri.EscapeDataString(cookieValue)}");
        request.Headers.Add(HeaderNames.Accept, "application/json");

        return await HttpClient.SendAsync(request);
    }

    protected string PrepareCookieName(string baseName)
    {
        return CookiesService.PrepareName(baseName);
    }

    protected string PrepareCookieName(HttpCookieKeyEnum key)
    {
        return CookiesService.PrepareName(key);
    }
    #endregion
}

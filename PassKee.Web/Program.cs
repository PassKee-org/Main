using System;
using System.Net.Http;
using Blazored.LocalStorage;
using Fluxor;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PassKee.Web;
using PassKee.Web.Services.Http;
using PassKee.Web.Services.Http.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
var currentAssembly = typeof(Program).Assembly;

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Init Environment config file 
var webHttp = new HttpClient()
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
};
var configurationFile = "Debug";
#if IS_RELEASE_BUILD
    configurationFile = "Release";
#elif IS_DEVELOPMENT_BUILD
    configurationFile = "Development";
#elif IS_LOCAL_BUILD
    configurationFile = "Local";
#endif
Console.WriteLine($"Application loaded with {configurationFile} configuration");
using var response = await webHttp.GetAsync($"appsettings.{configurationFile}.json");
using var stream = await response.Content.ReadAsStreamAsync();
builder.Configuration.AddJsonStream(stream);

// Base HttpClient
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Local Storage
builder.Services.AddBlazoredLocalStorage();

// Fluxor Store
builder.Services.AddFluxor(options =>
{
    options.ScanAssemblies(currentAssembly);
});

// Custom HTTP Client & Api Service
builder.Services.AddScoped<CustomHttpClient>();
builder.Services.AddScoped<ApiService>();

await builder.Build().RunAsync();

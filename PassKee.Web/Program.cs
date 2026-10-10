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
// Storage, Crypto & Business Services
builder.Services.AddScoped<PassKee.Web.Services.Storage.ISessionLockStorageService, PassKee.Web.Services.Storage.SessionLockStorageService>();
builder.Services.AddScoped<PassKee.Web.Services.Auth.IAuthClientService, PassKee.Web.Services.Auth.AuthClientService>();
builder.Services.AddScoped<PassKee.Web.Services.Auth.IInactivityLockService, PassKee.Web.Services.Auth.InactivityLockService>();
builder.Services.AddScoped<PassKee.Web.Core.Services.Vaults.IVaultCryptoService, PassKee.Web.Core.Services.Vaults.VaultCryptoService>();
builder.Services.AddScoped<PassKee.Web.Core.Services.Vaults.IVaultSearchService, PassKee.Web.Core.Services.Vaults.VaultSearchService>();
builder.Services.AddScoped<PassKee.Web.Services.Vaults.IVaultClientService, PassKee.Web.Services.Vaults.VaultClientService>();
builder.Services.AddScoped<PassKee.Web.Services.Import.IKdbxImportReader, PassKee.Web.Services.Import.KeePassImportReader>();
builder.Services.AddScoped<PassKee.Web.Services.Import.IVaultImportService, PassKee.Web.Services.Import.VaultImportService>();
builder.Services.AddScoped<PassKee.Web.Services.Import.VaultImportSession>();

// UI Services
builder.Services.AddScoped<PassKee.Web.Core.Services.UI.Toast.IToastService, PassKee.Web.Core.Services.UI.Toast.ToastService>();
builder.Services.AddScoped<PassKee.Web.Core.Services.UI.Modal.IAppModalDialogService, PassKee.Web.Core.Services.UI.Modal.AppModalDialogService>();

await builder.Build().RunAsync();

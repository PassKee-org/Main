using Blazored.LocalStorage;
using Fluxor;
using PassKee.Web.Server.Components;
using PassKee.Web.Services.Http;
using PassKee.Web.Services.Http.Client;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseStaticWebAssets();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

// Add HTTP Client
builder.Services.AddHttpClient();

// Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Fluxor state management
builder.Services.AddFluxor(options =>
{
    options.ScanAssemblies(typeof(PassKee.Web.App).Assembly);
});

// Custom HTTP Client & Api Service
builder.Services.AddScoped<CustomHttpClient>();
builder.Services.AddScoped<ApiService>();

// Storage, Crypto & Business Services
builder.Services.AddScoped<PassKee.Web.Services.Storage.ISessionLockStorageService, PassKee.Web.Services.Storage.SessionLockStorageService>();
builder.Services.AddScoped<PassKee.Web.Services.Auth.IAuthClientService, PassKee.Web.Services.Auth.AuthClientService>();
builder.Services.AddScoped<PassKee.Web.Core.Services.Vaults.IVaultCryptoService, PassKee.Web.Core.Services.Vaults.VaultCryptoService>();
builder.Services.AddScoped<PassKee.Web.Services.Vaults.IVaultClientService, PassKee.Web.Services.Vaults.VaultClientService>();

// UI Services
builder.Services.AddScoped<PassKee.Web.Core.Services.UI.Toast.IToastService, PassKee.Web.Core.Services.UI.Toast.ToastService>();
builder.Services.AddScoped<PassKee.Web.Core.Services.UI.Modal.IAppModalDialogService, PassKee.Web.Core.Services.UI.Modal.AppModalDialogService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddAdditionalAssemblies(typeof(PassKee.Web.App).Assembly)
    .AddInteractiveWebAssemblyRenderMode();

app.Run();

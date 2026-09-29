using Blazored.LocalStorage;
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
var apiConnectSources = app.Environment.IsProduction()
    ? "https://api.passkee.org"
    : "https://api.passkee.org https://dev-api.passkee.org http://localhost:5265 https://localhost:5265";

app.Use(async (context, next) =>
{
    context.Response.Headers["Content-Security-Policy"] = $"default-src 'self'; base-uri 'self'; object-src 'none'; frame-ancestors 'none'; form-action 'self'; script-src 'self' 'wasm-unsafe-eval'; style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; font-src 'self' data: https://cdn.jsdelivr.net; img-src 'self' data: blob:; connect-src 'self' {apiConnectSources}";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    await next();
});

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

using AppMultiTenant.Client.Components;
using AppMultiTenant.Client.State;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents();

// Configuración de HttpClient para la API
var apiSettings = builder.Configuration.GetSection("ApiSettings");
var apiBaseUrl = apiSettings["BaseUrl"] ?? "https://localhost:7291";

// Registrar HttpClient con URL base configurada
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

// Registrar servicios para autenticación
builder.Services.AddAuthorization();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => 
    provider.GetRequiredService<CustomAuthenticationStateProvider>());

// Registrar ApiClients (placeholder para futura implementación)
// builder.Services.AddScoped<IAuthApiClient, AuthApiClient>();
// builder.Services.AddScoped<IUserApiClient, UserApiClient>();
// builder.Services.AddScoped<IRoleApiClient, RoleApiClient>();
// builder.Services.AddScoped<ITenantApiClient, TenantApiClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>();

app.Run();

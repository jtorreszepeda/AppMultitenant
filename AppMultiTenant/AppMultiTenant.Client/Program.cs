using AppMultiTenant.Client.Components;
using AppMultiTenant.Client.Services;
using AppMultiTenant.Client.State;
using AppMultiTenant.Client.ViewModels;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents();

// Configuración de HttpClient para la API
var apiSettings = builder.Configuration.GetSection("ApiSettings");
var apiBaseUrl = apiSettings["BaseUrl"] ?? "https://localhost:7291";

// Registrar el AuthTokenHandler para agregar automáticamente JWT a las solicitudes
builder.Services.AddScoped<AuthTokenHandler>();

// Registrar HttpClient con URL base configurada y configurar con AuthTokenHandler
builder.Services.AddHttpClient("API", client => 
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthTokenHandler>();

// Registrar HttpClient como un servicio para inyectar en componentes
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

// Registrar servicios para autenticación
builder.Services.AddAuthorization();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => 
    provider.GetRequiredService<CustomAuthenticationStateProvider>());

// Registrar ApiClients
builder.Services.AddScoped<IAuthApiClient, AuthApiClient>();
builder.Services.AddScoped<IUserApiClient, UserApiClient>();
builder.Services.AddScoped<IRoleApiClient, RoleApiClient>();
builder.Services.AddScoped<ITenantApiClient, TenantApiClient>();
builder.Services.AddScoped<ISectionApiClient, SectionApiClient>();

// Registrar ViewModels
builder.Services.AddScoped<LoginViewModel>();
builder.Services.AddScoped<RegisterViewModel>();
builder.Services.AddScoped<UserListViewModel>();
builder.Services.AddScoped<CreateUserViewModel>();
builder.Services.AddScoped<EditUserViewModel>();

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

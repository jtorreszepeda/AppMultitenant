using AppMultiTenant.Client.Components;
using AppMultiTenant.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents();

// Añadir servicios de MudBlazor
builder.Services.AddMudServices();

// Registrar el AuthTokenHandler
builder.Services.AddTransient<AuthTokenHandler>();

// Configurar HttpClient con el AuthTokenHandler
builder.Services.AddScoped(sp => 
{
    // Obtener el handler para adjuntar el token JWT
    var authTokenHandler = sp.GetRequiredService<AuthTokenHandler>();
    
    // Configurar el InnerHandler para el DelegatingHandler
    authTokenHandler.InnerHandler = new HttpClientHandler();
    
    // Crear el HttpClient con el handler configurado
    var httpClient = new HttpClient(authTokenHandler)
    {
        BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? 
                              builder.Configuration["ApiSettings:BaseUrl"] ?? 
                              "https://localhost:7001")
    };
    
    return httpClient;
});

// Registrar servicios para autenticación
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => 
    provider.GetRequiredService<CustomAuthenticationStateProvider>());
builder.Services.AddAuthorizationCore();

// Registrar servicios de la aplicación
builder.Services.AddScoped<ISectionService, SectionService>();

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

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>();

app.Run();

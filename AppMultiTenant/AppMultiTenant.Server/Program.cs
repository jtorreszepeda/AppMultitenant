using Serilog;
using AppMultiTenant.Application.Configuration;
using AppMultiTenant.Application.Interfaces.Services;
using AppMultiTenant.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;
using AppMultiTenant.Server.Middleware;
using AppMultiTenant.Infrastructure.Persistence.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add User Secrets in Development environment
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Configure Serilog
builder.Host.UseSerilog((context, configuration) => 
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container.

// Register services from different projects/layers
ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
ConfigureMiddleware(app, app.Environment);

// Ensure we log application startup
Log.Information("Application starting up");
// Log some configuration values (without sensitive data)
Log.Information("Application Configuration: TenantResolutionStrategy = {TenantResolutionStrategy}", 
    builder.Configuration["TenantConfiguration:TenantResolutionStrategy"]);

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}

// Method to configure all services
void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // Controllers
    services.AddControllers();
    
    // API Documentation
    services.AddOpenApi();
    
    // CORS Configuration
    services.AddCors(options =>
    {
        options.AddPolicy("AppCorsPolicy", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });
    
    // Configuration Options pattern registration
    services.Configure<TenantConfiguration>(
        configuration.GetSection("TenantConfiguration"));
    services.Configure<JwtSettings>(
        configuration.GetSection("JwtSettings"));
    
    // Domain and Application layer services will be registered here in future tasks
    
    // Infrastructure layer services
    // HttpContextAccessor - necesario para el TenantResolverService
    services.AddHttpContextAccessor();
    
    // TenantResolverService - implementación básica para resolución de inquilinos
    services.AddScoped<ITenantResolverService, TenantResolverService>();
    
    // Persistence services - Entity Framework Core y repositorios
    services.AddPersistenceServices(configuration);
    
    // Authentication services will be registered here in future tasks
}

// Method to configure the HTTP request pipeline
void ConfigureMiddleware(WebApplication app, IWebHostEnvironment env)
{
    // Developer exception page and OpenAPI
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.MapOpenApi();
    }
    
    // Request logging middleware
    app.UseSerilogRequestLogging();
    
    // Global error handling middleware will be added here in future tasks
    
    // Tenant resolution middleware will be added here in future tasks
    app.UseTenantResolution();
    
    app.UseHttpsRedirection();
    
    app.UseCors("AppCorsPolicy");
    
    app.UseRouting();
    
    app.UseAuthentication();
    app.UseAuthorization();
    
    app.MapControllers();
}

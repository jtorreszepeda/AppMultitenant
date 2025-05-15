using Serilog;
using AppMultiTenant.Application.Configuration;
using AppMultiTenant.Application.Interfaces.Services;
using AppMultiTenant.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;
using AppMultiTenant.Server.Middleware;
using AppMultiTenant.Infrastructure.Persistence.Configuration;
using Serilog.Context;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add User Secrets in Development environment
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Configure Serilog
builder.Host.UseSerilog((context, configuration) => 
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext());

// Add Application Insights if configured
var appInsightsConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
if (!string.IsNullOrEmpty(appInsightsConnectionString))
{
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.ConnectionString = appInsightsConnectionString;
        options.EnableAdaptiveSampling = builder.Configuration.GetValue<bool>("ApplicationInsights:EnableAdaptiveSampling", true);
        options.EnableHeartbeat = builder.Configuration.GetValue<bool>("ApplicationInsights:EnableHeartbeat", true);
    });
    
    Log.Information("Application Insights telemetry enabled");
}

// Add services to the container.

// Register services from different projects/layers
ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
ConfigureMiddleware(app, app.Environment);

// Ensure we log application startup
Log.Information("Application starting up in {Environment} environment", app.Environment.EnvironmentName);

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
    // Controllers con validación de modelos
    services.AddControllers();
    
    // Configuración de validación de modelos automática
    services.AddModelValidation();
    
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
    
    // Domain and Application layer services
    services.AddApplicationServices();
    
    // Infrastructure layer services
    // HttpContextAccessor - necesario para el TenantResolverService
    services.AddHttpContextAccessor();
    
    // TenantResolverService - implementación básica para resolución de inquilinos
    services.AddScoped<ITenantResolverService, TenantResolverService>();
    
    // Persistence services - Entity Framework Core y repositorios
    services.AddPersistenceServices(configuration);
    
    // Identity services - ASP.NET Core Identity configurado para multi-inquilino
    services.AddIdentityServices(configuration);
    
    // JWT Authentication services
    services.AddJwtAuthentication(configuration);
    
    // Configuración de autorización
    services.AddAuthorization(options =>
    {
        // Política para requerir acceso a un inquilino específico
        options.AddPolicy("RequireTenantAccess", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.AddRequirements(new TenantRequirement());
        });

        // Políticas para permisos de sistema
        AddPermissionPolicy(options, "CreateUser", AppMultiTenant.Domain.Entities.Permission.SystemPermissions.CreateUser);
        AddPermissionPolicy(options, "EditUser", AppMultiTenant.Domain.Entities.Permission.SystemPermissions.EditUser);
        AddPermissionPolicy(options, "DeleteUser", AppMultiTenant.Domain.Entities.Permission.SystemPermissions.DeleteUser);
        AddPermissionPolicy(options, "ViewUsers", AppMultiTenant.Domain.Entities.Permission.SystemPermissions.ViewUsers);
        
        AddPermissionPolicy(options, "CreateRole", AppMultiTenant.Domain.Entities.Permission.SystemPermissions.CreateRole);
        AddPermissionPolicy(options, "EditRole", AppMultiTenant.Domain.Entities.Permission.SystemPermissions.EditRole);
        AddPermissionPolicy(options, "DeleteRole", AppMultiTenant.Domain.Entities.Permission.SystemPermissions.DeleteRole);
        AddPermissionPolicy(options, "ViewRoles", AppMultiTenant.Domain.Entities.Permission.SystemPermissions.ViewRoles);
        
        AddPermissionPolicy(options, "AssignRoles", AppMultiTenant.Domain.Entities.Permission.SystemPermissions.AssignRoles);
        AddPermissionPolicy(options, "AssignPermissions", AppMultiTenant.Domain.Entities.Permission.SystemPermissions.AssignPermissions);
        
        AddPermissionPolicy(options, "DefineSections", AppMultiTenant.Domain.Entities.Permission.SystemPermissions.DefineSections);
        AddPermissionPolicy(options, "ViewAllSections", AppMultiTenant.Domain.Entities.Permission.SystemPermissions.ViewAllSections);
    });
    
    // Método auxiliar para agregar políticas de permisos
    void AddPermissionPolicy(AuthorizationOptions options, string policyName, string permission)
    {
        options.AddPolicy(policyName, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.AddRequirements(new TenantRequirement());
            policy.AddRequirements(new PermissionRequirement(permission));
        });
    }
    
    // Registro de los authorization handlers
    services.AddScoped<IAuthorizationHandler, TenantAuthorizationHandler>();
    services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
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
    app.UseSerilogRequestLogging(options => 
    {
        // Customize the message template
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        
        // Enrich with custom properties
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) => 
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
            diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress?.ToString());
            
            // Try to get TenantId and add it to the log context if available
            var tenantId = httpContext.Items["TenantId"]?.ToString();
            if (!string.IsNullOrEmpty(tenantId))
            {
                LogContext.PushProperty("TenantId", tenantId);
                diagnosticContext.Set("TenantId", tenantId);
            }
        };
    });
    
    // Global error handling middleware
    app.UseGlobalExceptionHandling();
    
    // Tenant resolution middleware
    app.UseTenantResolution();
    
    app.UseHttpsRedirection();
    
    app.UseCors("AppCorsPolicy");
    
    app.UseRouting();
    
    app.UseAuthentication();
    app.UseAuthorization();
    
    app.MapControllers();
}

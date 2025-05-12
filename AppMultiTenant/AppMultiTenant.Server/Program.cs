using Serilog;

var builder = WebApplication.CreateBuilder(args);

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
    
    // Domain and Application layer services will be registered here in future tasks
    
    // Infrastructure layer services will be registered here in future tasks
    
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
    
    app.UseHttpsRedirection();
    
    app.UseCors("AppCorsPolicy");
    
    app.UseRouting();
    
    app.UseAuthentication();
    app.UseAuthorization();
    
    app.MapControllers();
}

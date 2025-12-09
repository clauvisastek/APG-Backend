using APG.Persistence;
using APG.Persistence.Data;
using APG.Application.Services;
using APG.Persistence.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://astekcanada.ca.auth0.com/";
        options.Audience = "https://api.apg-astek.com";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudiences = new[] { "https://api.apg-astek.com", "https://astekcanada.ca.auth0.com/userinfo" },
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromDays(365), // Allow 1 year clock skew for development
            NameClaimType = "name",
            RoleClaimType = "https://apg-astek.com/roles"
        };
        options.RequireHttpsMetadata = false; // Allow HTTP for local development
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError(context.Exception, "Authentication failed: {Message}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Token validated successfully for user: {User}", context.Principal?.Identity?.Name ?? "Unknown");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("Authentication challenge: {Error} - {ErrorDescription}", context.Error, context.ErrorDescription);
                return Task.CompletedTask;
            }
        };
    });

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:3000")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});

// Add Persistence layer (includes DbContext)
builder.Services.AddPersistence(builder.Configuration);

// Add HttpContextAccessor for role-based access control
builder.Services.AddHttpContextAccessor();

// Add Application services
builder.Services.AddScoped<IBusinessUnitService, BusinessUnitService>();
builder.Services.AddScoped<ISectorService, SectorService>();
builder.Services.AddScoped<IBusinessUnitAccessService, BusinessUnitAccessService>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ICalculatorSettingsService, CalculatorSettingsService>();
builder.Services.AddScoped<IGlobalSalarySettingsService, GlobalSalarySettingsService>();
builder.Services.AddScoped<IMarginSimulationService, MarginSimulationService>();
builder.Services.AddScoped<IMarketTrendsService, MarketTrendsService>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Apply migrations automatically on startup (optional - remove in production)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Checking database connection...");
        logger.LogInformation($"Connection string: {builder.Configuration.GetConnectionString("DefaultConnection")}");
        
        if (context.Database.CanConnect())
        {
            logger.LogInformation("Database connection successful. Applying migrations...");
            context.Database.Migrate();
            logger.LogInformation("Migrations applied successfully.");
        }
        else
        {
            logger.LogWarning("Cannot connect to database. Skipping migrations.");
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while applying migrations.");
        // Don't throw - allow app to start even if migrations fail
    }
}

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map health check endpoint
app.MapHealthChecks("/health");

// Debug endpoint to check authentication
app.MapGet("/api/debug/auth", (HttpContext context) =>
{
    var user = context.User;
    return new
    {
        IsAuthenticated = user?.Identity?.IsAuthenticated ?? false,
        Name = user?.Identity?.Name,
        Claims = user?.Claims.Select(c => new { c.Type, c.Value }).ToList()
    };
}).WithName("DebugAuth").WithOpenApi();

// Add a simple test endpoint
app.MapGet("/", () => new
{
    Status = "Running",
    Environment = app.Environment.EnvironmentName,
    Timestamp = DateTime.UtcNow
}).WithName("Root").WithOpenApi();

app.Run();

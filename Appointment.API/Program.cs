using System.Text;
using System.Text.Json;
using Appointment.Application.Services.Classes;
using Appointment.Application.Services.Interfaces;
using Appointment.Infrastructure.Data;
using Appointment.Infrastructure.Repositories.Classes;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ─── Controllers ──────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy        = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// ─── Swagger with SoftOnCloud JWT bearer ──────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title   = "Appointment API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "SoftOnCloud JWT from POST https://api.softoncloud.com/api/auth/login. Example: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ─── SoftOnCloud HTTP client (product-connection, etc.) ───────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("SoftOnCloud", client =>
{
    var baseUrl = builder.Configuration["SoftOnCloud:ApiBaseUrl"] ?? "https://api.softoncloud.com";
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
});

builder.Services.AddScoped<ITenantConnectionFactory, SoftOnCloudTenantConnectionFactory>();

// ─── Infrastructure helpers ───────────────────────────────────────────────────
// Product DB connection comes from SoftOnCloud product-connection (via factory).
builder.Services.AddScoped<ProductDatabaseHelper>();

// ─── Customer DI registrations ────────────────────────────────────────────────
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

// ─── Professional DI registrations ────────────────────────────────────────────
builder.Services.AddScoped<IProfessionalRepository, ProfessionalRepository>();
builder.Services.AddScoped<IProfessionalService, ProfessionalService>();

// ─── Service (treatment) catalog DI ───────────────────────────────────────────
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IServiceCatalogService, ServiceCatalogService>();

// ─── Appointment booking DI ───────────────────────────────────────────────────
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IAppointmentDocumentRepository, AppointmentDocumentRepository>();
builder.Services.AddScoped<IAppointmentDocumentService, AppointmentDocumentService>();

// ─── Queue / Check-In DI ──────────────────────────────────────────────────────
builder.Services.AddScoped<IQueueRepository, QueueRepository>();
builder.Services.AddScoped<IQueueService, QueueService>();

// ─── Cabin / Room DI ──────────────────────────────────────────────────────────
builder.Services.AddScoped<ICabinRepository, CabinRepository>();
builder.Services.AddScoped<ICabinService, CabinService>();

// ─── Dashboard DI ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// ─── Settings DI ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();
builder.Services.AddScoped<ISettingsService, SettingsService>();

// ─── SoftOnCloud JWT validation (tokens issued by SoftOnCloud login API) ──────
var jwtSecret = builder.Configuration["SoftOnCloud:Jwt:Secret"]
                ?? throw new InvalidOperationException("SoftOnCloud:Jwt:Secret is not configured.");
var jwtIssuer = builder.Configuration["SoftOnCloud:Jwt:Issuer"] ?? "SoftOnCloud";
var jwtAudience = builder.Configuration["SoftOnCloud:Jwt:Audience"] ?? "SoftOnCloud";

if (string.Equals(jwtSecret, "REPLACE_WITH_SOFTONCLOUD_JWT_SECRET", StringComparison.Ordinal))
{
    throw new InvalidOperationException(
        "SoftOnCloud:Jwt:Secret is still the placeholder. Ask SoftOnCloud platform for the JWT signing secret.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtIssuer,
        ValidAudience            = jwtAudience,
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew                = TimeSpan.FromMinutes(1)
    };

    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            context.HandleResponse();
            context.Response.StatusCode  = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(new { message = "Your session has ended. Please sign in again." }));
        }
    };
});

// ─── CORS ─────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AppointmentCors", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ─── Build ────────────────────────────────────────────────────────────────────
var app = builder.Build();

var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
startupLogger.LogInformation(
    "Appointment API starting. Environment={Environment}; SoftOnCloud={SoftOnCloudBase}",
    app.Environment.EnvironmentName,
    builder.Configuration["SoftOnCloud:ApiBaseUrl"]);

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Appointment API v1");
    options.RoutePrefix = "swagger";
});

app.UseRouting();
app.UseCors("AppointmentCors");

// Serve uploaded appointment documents from wwwroot/Uploads/...
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Appointment API is running");
app.MapControllers();

app.Run();

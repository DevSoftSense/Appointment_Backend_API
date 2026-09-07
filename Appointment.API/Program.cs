using System.Text;
using System.Text.Json;
using Appointment.API.Helpers;
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

// ─── Swagger with JWT bearer ──────────────────────────────────────────────────
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
        Description  = "Enter your JWT token. Example: Bearer {token}"
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

// ─── Infrastructure helpers ───────────────────────────────────────────────────
builder.Services.AddScoped<MasterDatabaseHelper>();
builder.Services.AddScoped<ProductDatabaseHelper>();
builder.Services.AddScoped<IJwtTokenHelper, JwtTokenHelper>();

// ─── Auth DI registrations ────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService,    AuthService>();

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

// ─── JWT authentication ───────────────────────────────────────────────────────
var jwtKey      = builder.Configuration["Jwt:Key"]      ?? throw new InvalidOperationException("Jwt:Key is not configured.");
var jwtIssuer   = builder.Configuration["Jwt:Issuer"]   ?? "Appointment.API";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "Appointment.API.Users";

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
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
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
startupLogger.LogInformation("Appointment API starting. Environment={Environment}", app.Environment.EnvironmentName);

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Appointment API v1");
    options.RoutePrefix = "swagger";
});

app.UseRouting();
app.UseCors("AppointmentCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Appointment API is running");
app.MapControllers();

app.Run();

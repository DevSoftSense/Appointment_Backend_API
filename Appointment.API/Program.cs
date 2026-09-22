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
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient("SoftOnCloud", client =>
{
    var baseUrl = builder.Configuration["SoftOnCloud:ApiBaseUrl"] ?? "https://api.softoncloud.com";
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
    // Avoid hanging staff/QR requests until the browser aborts (shows as fake CORS / ERR_FAILED).
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<ITenantConnectionFactory, SoftOnCloudTenantConnectionFactory>();
builder.Services.AddScoped<IPublicBookOrgContext, PublicBookOrgContext>();

// ─── Infrastructure helpers ───────────────────────────────────────────────────
// Product DB: SoftOnCloud product-connection (JWT staff) or /service (QR) or local SecondConnection.
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

// ─── Reports DI ───────────────────────────────────────────────────────────────
builder.Services.AddScoped<IReportsRepository, ReportsRepository>();
builder.Services.AddScoped<IReportsService, ReportsService>();
builder.Services.AddScoped<IReportScheduleRepository, ReportScheduleRepository>();
builder.Services.AddScoped<IReportScheduleService, ReportScheduleService>();

// ─── Settings DI ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IOrgMastersRepository, OrgMastersRepository>();
builder.Services.AddScoped<IOrgMastersService, OrgMastersService>();

// ─── Reminders (email queue on public.tab_notifications) ──────────────────────
builder.Services.AddScoped<IReminderRepository, ReminderRepository>();
builder.Services.AddScoped<IReminderService, ReminderService>();
builder.Services.AddScoped<Appointment.Infrastructure.Email.ISmtpEmailSender, Appointment.Infrastructure.Email.SmtpEmailSender>();
builder.Services.AddHostedService<Appointment.API.Workers.ReminderEmailWorker>();
builder.Services.AddHostedService<Appointment.API.Workers.ReportScheduleEmailWorker>();

builder.Services.AddScoped<IAutoNoShowRepository, AutoNoShowRepository>();
builder.Services.AddScoped<IAutoNoShowService, AutoNoShowService>();
builder.Services.AddHostedService<Appointment.API.Workers.AutoNoShowWorker>();

// ─── Public QR self-booking DI ────────────────────────────────────────────────
builder.Services.AddScoped<IPublicBookingService, PublicBookingService>();
builder.Services.AddSingleton<IPublicBookTokenService, PublicBookTokenService>();

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

// ─── CORS (origins only here — not duplicated in appsettings) ─────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AppointmentCors", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "https://appointment.softoncloud.com",
                "http://appointment.softoncloud.com",
                "https://www.appointment.softoncloud.com",
                "http://www.appointment.softoncloud.com"
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
    "Appointment API starting. Environment={Environment}; SoftOnCloud={SoftOnCloudBase}; Frontend={Frontend}",
    app.Environment.EnvironmentName,
    builder.Configuration["SoftOnCloud:ApiBaseUrl"],
    builder.Configuration["Appointment:FrontendBaseUrl"]);

// Always return JSON errors (with CORS) instead of empty/connection drops that the browser labels as CORS.
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var ex = feature?.Error;
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("UnhandledException");
        logger.LogError(ex, "Unhandled API exception Path={Path}", context.Request.Path);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = ex switch
        {
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            InvalidOperationException => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status500InternalServerError
        };

        var message = ex switch
        {
            UnauthorizedAccessException => "Your session has ended. Please sign in again.",
            InvalidOperationException ioe => ioe.Message,
            _ => "Unexpected server error. Please try again."
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(new { message }));
    });
});

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

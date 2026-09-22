using System.Text;
using System.Text.Json;
using Appointment.Application.Services.Helpers;
using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Reports.Requests;
using Appointment.Domain.DTOs.Reports.Responses;
using Appointment.Infrastructure.Data;
using Appointment.Infrastructure.Email;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Appointment.Application.Services.Classes;

public sealed class ReportScheduleService : IReportScheduleService
{
    private static readonly HashSet<string> AllowedTabs = new(StringComparer.OrdinalIgnoreCase)
    {
        "overview", "appointments", "services", "professionals", "no_show", "customers"
    };

    private static readonly HashSet<string> AllowedFreq = new(StringComparer.OrdinalIgnoreCase)
    {
        "once", "daily", "weekly"
    };

    private readonly IReportScheduleRepository _scheduleRepository;
    private readonly IReportsRepository _reportsRepository;
    private readonly ISmtpEmailSender _emailSender;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ReportScheduleService> _logger;

    public ReportScheduleService(
        IReportScheduleRepository scheduleRepository,
        IReportsRepository reportsRepository,
        ISmtpEmailSender emailSender,
        IConfiguration configuration,
        ILogger<ReportScheduleService> logger)
    {
        _scheduleRepository = scheduleRepository;
        _reportsRepository = reportsRepository;
        _emailSender = emailSender;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ReportScheduleListResponse> ListAsync(
        int orgId, GetReportSchedulesRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        request ??= new GetReportSchedulesRequest();
        if (request.Limit <= 0) request.Limit = 50;
        if (request.Offset < 0) request.Offset = 0;

        var items = await _scheduleRepository.ListAsync(orgId, GetAppId(), request, cancellationToken);
        return new ReportScheduleListResponse
        {
            Items = items,
            TotalCount = items.Count > 0 ? items[0].TotalCount : 0
        };
    }

    public async Task<ReportScheduleItemDto> CreateAsync(
        int orgId, long userId, CreateReportScheduleRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (request is null) throw new ArgumentNullException(nameof(request));

        var email = (request.ToAddress ?? "").Trim();
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException("A valid email address is required.");

        var tab = (request.ReportTab ?? "overview").Trim().ToLowerInvariant();
        if (!AllowedTabs.Contains(tab))
            throw new ArgumentException("Invalid report tab.");

        var freq = (request.Frequency ?? "once").Trim().ToLowerInvariant();
        if (!AllowedFreq.Contains(freq))
            throw new ArgumentException("Frequency must be once, daily, or weekly.");

        var rolling = request.RollingDays <= 0 ? 30 : Math.Min(request.RollingDays, 366);

        if (freq == "once")
        {
            if (request.FromDate is null || request.ToDate is null)
                throw new ArgumentException("FromDate and ToDate are required for a one-time schedule.");
            if (request.ToDate < request.FromDate)
                throw new ArgumentException("ToDate must be on or after FromDate.");
            if (request.ToDate.Value.DayNumber - request.FromDate.Value.DayNumber > 366)
                throw new ArgumentException("Date range cannot exceed 366 days.");
        }

        var def = new ReportScheduleDefinitionDto
        {
            ReportTab = tab,
            Format = "csv",
            Frequency = freq,
            RollingDays = rolling,
            FromDate = freq == "once" ? request.FromDate : null,
            ToDate = freq == "once" ? request.ToDate : null,
            BranchId = request.BranchId,
            ProfessionalId = request.ProfessionalId
        };

        var definitionJson = JsonSerializer.Serialize(def, PostgresJsonOptions.Options);
        var scheduledAt = request.ScheduledAt ?? DateTimeOffset.UtcNow;
        var title = string.IsNullOrWhiteSpace(request.Title)
            ? $"Scheduled report: {tab} ({freq})"
            : request.Title.Trim();

        return await _scheduleRepository.CreateAsync(
            orgId, GetAppId(), userId, title, definitionJson, email, scheduledAt, cancellationToken);
    }

    public async Task CancelAsync(int orgId, long notificationId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (notificationId <= 0)
            throw new ArgumentException("Schedule id is required.");
        await _scheduleRepository.CancelAsync(orgId, GetAppId(), notificationId, cancellationToken);
    }

    public async Task ProcessDueAsync(CancellationToken cancellationToken = default)
    {
        var appId = GetAppId();
        var due = await _scheduleRepository.ClaimDueAsync(appId, orgId: null, batchSize: 10, cancellationToken);

        foreach (var item in due)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(item.ToAddress) || !item.ToAddress.Contains('@'))
                {
                    await _scheduleRepository.MarkFailedAsync(
                        appId, item.NotificationId, item.OrgId, "Missing or invalid to_address", cancellationToken);
                    continue;
                }

                var def = ParseDefinition(item.DefinitionJson);
                var (from, to) = ResolveRange(def);
                var payload = await LoadReportAsync(item.OrgId, appId, def, from, to, cancellationToken);
                var (fileName, csv) = ReportCsvBuilder.Build(def.ReportTab, from, to, payload);

                var subject = string.IsNullOrWhiteSpace(item.Title)
                    ? $"Appointment report ({def.ReportTab}) {from:yyyy-MM-dd} → {to:yyyy-MM-dd}"
                    : item.Title!;

                var bodyHtml =
                    "<div style=\"font-family:Segoe UI,Arial,sans-serif;max-width:560px;margin:0 auto;color:#1f2937\">" +
                    "<div style=\"background:#0f4c81;color:#fff;padding:16px 20px;border-radius:8px 8px 0 0\">" +
                    "<strong style=\"font-size:18px\">SoftOnCloud Appointment</strong></div>" +
                    "<div style=\"border:1px solid #e5e7eb;border-top:0;padding:20px;border-radius:0 0 8px 8px\">" +
                    $"<p>Your scheduled <strong>{Html(def.ReportTab)}</strong> report is attached (CSV).</p>" +
                    $"<p style=\"margin:8px 0\">Period: <strong>{from:dd MMM yyyy}</strong> → <strong>{to:dd MMM yyyy}</strong></p>" +
                    $"<p style=\"color:#6b7280;font-size:13px\">Frequency: {Html(def.Frequency)}. This is an automated message.</p>" +
                    "</div></div>";

                await _emailSender.SendEmailAsync(
                    item.ToAddress,
                    subject,
                    bodyHtml,
                    Encoding.UTF8.GetBytes(csv),
                    fileName,
                    "text/csv",
                    cancellationToken);

                var freq = (def.Frequency ?? "once").Trim().ToLowerInvariant();
                if (freq is "daily" or "weekly")
                    await _scheduleRepository.RescheduleNextAsync(appId, item.NotificationId, item.OrgId, cancellationToken);
                else
                    await _scheduleRepository.MarkSentAsync(appId, item.NotificationId, item.OrgId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed sending scheduled report {Id}", item.NotificationId);
                try
                {
                    await _scheduleRepository.MarkFailedAsync(
                        appId, item.NotificationId, item.OrgId, ex.Message, cancellationToken);
                }
                catch (Exception markEx)
                {
                    _logger.LogError(markEx, "Could not mark schedule {Id} failed", item.NotificationId);
                }
            }
        }
    }

    private async Task<object> LoadReportAsync(
        int orgId, int appId, ReportScheduleDefinitionDto def,
        DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        var tab = (def.ReportTab ?? "overview").Trim().ToLowerInvariant();
        return tab switch
        {
            "appointments" => await _reportsRepository.GetAppointmentsAsync(
                orgId, appId, from, to, def.BranchId, def.ProfessionalId, null, 500, 0, cancellationToken),
            "services" => await _reportsRepository.GetServicesAsync(
                orgId, appId, from, to, def.BranchId, def.ProfessionalId, cancellationToken),
            "professionals" => await _reportsRepository.GetProfessionalsAsync(
                orgId, appId, from, to, def.BranchId, def.ProfessionalId, cancellationToken),
            "no_show" => await _reportsRepository.GetNoShowAsync(
                orgId, appId, from, to, def.BranchId, def.ProfessionalId, cancellationToken),
            "customers" => await _reportsRepository.GetCustomersAsync(
                orgId, appId, from, to, def.BranchId, def.ProfessionalId, 500, 0, cancellationToken),
            _ => await _reportsRepository.GetOverviewAsync(
                orgId, appId, from, to, def.BranchId, def.ProfessionalId, cancellationToken)
        };
    }

    private static ReportScheduleDefinitionDto ParseDefinition(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new InvalidOperationException("Schedule definition JSON is missing.");

        var def = JsonSerializer.Deserialize<ReportScheduleDefinitionDto>(json, PostgresJsonOptions.Options)
                  ?? throw new InvalidOperationException("Could not parse schedule definition.");

        if (string.IsNullOrWhiteSpace(def.ReportTab))
            def.ReportTab = "overview";
        if (string.IsNullOrWhiteSpace(def.Frequency))
            def.Frequency = "once";
        if (def.RollingDays <= 0)
            def.RollingDays = 30;

        return def;
    }

    /// <summary>Uses India calendar date for rolling windows (IST).</summary>
    private static (DateOnly From, DateOnly To) ResolveRange(ReportScheduleDefinitionDto def)
    {
        var freq = (def.Frequency ?? "once").Trim().ToLowerInvariant();
        if (freq == "once")
        {
            if (def.FromDate is null || def.ToDate is null)
                throw new InvalidOperationException("once schedule requires from_date and to_date.");
            return (def.FromDate.Value, def.ToDate.Value);
        }

        var today = GetIndiaToday();
        var to = today;
        var days = Math.Clamp(def.RollingDays, 1, 366);
        var from = to.AddDays(-(days - 1));
        return (from, to);
    }

    private static DateOnly GetIndiaToday()
    {
        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(
                OperatingSystem.IsWindows() ? "India Standard Time" : "Asia/Kolkata");
            return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz));
        }
        catch (TimeZoneNotFoundException)
        {
            return DateOnly.FromDateTime(DateTime.UtcNow.AddHours(5).AddMinutes(30));
        }
    }

    private int GetAppId()
    {
        var appId = _configuration.GetValue<int?>("Appointment:AppId")
                    ?? _configuration.GetValue<int?>("Appointment:ProductId")
                    ?? 25;
        return appId;
    }

    private static void ValidateOrg(int orgId)
    {
        if (orgId <= 0)
            throw new ArgumentException("Organisation id is required.", nameof(orgId));
    }

    private static string Html(string? s) =>
        System.Net.WebUtility.HtmlEncode(s ?? "");
}

using System.Globalization;
using System.Text;
using Appointment.Domain.DTOs.Reports.Responses;

namespace Appointment.Application.Services.Helpers;

public static class ReportCsvBuilder
{
    public static (string FileName, string Csv) Build(
        string reportTab,
        DateOnly from,
        DateOnly to,
        object payload)
    {
        var stamp = $"{from:yyyy-MM-dd}_to_{to:yyyy-MM-dd}";
        var tab = (reportTab ?? "overview").Trim().ToLowerInvariant();

        return tab switch
        {
            "overview" when payload is ReportOverviewDto o =>
                ($"report-overview-{stamp}.csv", ToCsv(OverviewRows(o, from, to))),
            "appointments" when payload is ReportAppointmentsDto a =>
                ($"report-appointments-{stamp}.csv", ToCsv(AppointmentRows(a))),
            "services" when payload is ReportServicesDto s =>
                ($"report-services-{stamp}.csv", ToCsv(ServiceRows(s))),
            "professionals" when payload is ReportProfessionalsDto p =>
                ($"report-professionals-{stamp}.csv", ToCsv(ProfessionalRows(p))),
            "no_show" when payload is ReportNoShowDto n =>
                ($"report-noshow-{stamp}.csv", ToCsv(NoShowRows(n))),
            "customers" when payload is ReportCustomersDto c =>
                ($"report-customers-{stamp}.csv", ToCsv(CustomerRows(c))),
            _ => ($"report-{tab}-{stamp}.csv", ToCsv([new Dictionary<string, object?> { ["note"] = "No data" }]))
        };
    }

    private static IEnumerable<Dictionary<string, object?>> OverviewRows(
        ReportOverviewDto o, DateOnly from, DateOnly to)
    {
        yield return new Dictionary<string, object?>
        {
            ["from"] = from.ToString("yyyy-MM-dd"),
            ["to"] = to.ToString("yyyy-MM-dd"),
            ["total"] = o.Kpis?.Total,
            ["completed"] = o.Kpis?.Completed,
            ["upcoming"] = o.Kpis?.Upcoming,
            ["no_show"] = o.Kpis?.NoShow,
            ["cancelled"] = o.Kpis?.Cancelled,
            ["completed_booking_value"] = o.Kpis?.Revenue,
            ["completion_rate"] = o.Insights?.CompletionRate,
            ["no_show_rate"] = o.Insights?.NoShowRate
        };
    }

    private static IEnumerable<Dictionary<string, object?>> AppointmentRows(ReportAppointmentsDto a) =>
        (a.Items ?? []).Select(r => new Dictionary<string, object?>
        {
            ["appointment_no"] = r.AppointmentNo,
            ["date"] = r.AppointmentDate,
            ["customer"] = r.CustomerName,
            ["service"] = r.ServiceName,
            ["professional"] = r.ProfessionalName,
            ["status"] = r.Status,
            ["source"] = r.Source,
            ["type"] = r.AppointmentType,
            ["amount"] = r.Amount
        });

    private static IEnumerable<Dictionary<string, object?>> ServiceRows(ReportServicesDto s) =>
        (s.Items ?? []).Select(r => new Dictionary<string, object?>
        {
            ["service"] = r.ServiceName,
            ["duration_minutes"] = r.DurationMinutes,
            ["total"] = r.Total,
            ["completed"] = r.Completed,
            ["no_show"] = r.NoShow,
            ["percent"] = r.Percent,
            ["booking_value"] = r.Revenue
        });

    private static IEnumerable<Dictionary<string, object?>> ProfessionalRows(ReportProfessionalsDto p) =>
        (p.Items ?? []).Select(r => new Dictionary<string, object?>
        {
            ["professional"] = r.ProfessionalName,
            ["total"] = r.Total,
            ["completed"] = r.Completed,
            ["no_show"] = r.NoShow,
            ["no_show_rate"] = r.NoShowRate,
            ["booking_value"] = r.Revenue
        });

    private static IEnumerable<Dictionary<string, object?>> NoShowRows(ReportNoShowDto n) =>
        (n.ByProfessional ?? []).Select(r => new Dictionary<string, object?>
        {
            ["professional"] = r.ProfessionalName,
            ["no_show"] = r.NoShow,
            ["total"] = r.Total,
            ["no_show_rate"] = r.NoShowRate
        });

    private static IEnumerable<Dictionary<string, object?>> CustomerRows(ReportCustomersDto c) =>
        (c.Items ?? []).Select(r => new Dictionary<string, object?>
        {
            ["customer"] = r.CustomerName,
            ["code"] = r.PartyCode,
            ["segment"] = r.Segment,
            ["visits"] = r.Visits,
            ["completed"] = r.Completed,
            ["no_show"] = r.NoShow,
            ["cancelled"] = r.Cancelled,
            ["booking_value"] = r.BookingValue,
            ["first_in_period"] = r.FirstInPeriod,
            ["last_in_period"] = r.LastInPeriod,
            ["phone"] = r.Phone,
            ["email"] = r.Email
        });

    private static string ToCsv(IEnumerable<Dictionary<string, object?>> rows)
    {
        var list = rows.ToList();
        if (list.Count == 0)
            return "note\nNo data\n";

        var headers = list[0].Keys.ToList();
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", headers.Select(Escape)));
        foreach (var row in list)
        {
            sb.AppendLine(string.Join(",", headers.Select(h =>
            {
                row.TryGetValue(h, out var v);
                return Escape(Format(v));
            })));
        }
        return sb.ToString();
    }

    private static string Format(object? v) =>
        v switch
        {
            null => "",
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture) ?? "",
            _ => v.ToString() ?? ""
        };

    private static string Escape(string? s)
    {
        s ??= "";
        if (s.Contains('"') || s.Contains(',') || s.Contains('\n') || s.Contains('\r'))
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        return s;
    }
}

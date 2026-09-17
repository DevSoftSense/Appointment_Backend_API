namespace Appointment.Application.Helpers;

/// <summary>
/// Advance booking window: today through last day of (current month + N months).
/// N = booking_window_months (1–12). Default 1 = end of next calendar month.
/// </summary>
public static class BookingWindowHelper
{
    public const int DefaultMonths = 1;
    public const int MinMonths = 1;
    public const int MaxMonths = 12;

    public static int ClampMonths(int? months)
    {
        var n = months ?? DefaultMonths;
        if (n < MinMonths) return MinMonths;
        if (n > MaxMonths) return MaxMonths;
        return n;
    }

    /// <summary>Local calendar today as DateOnly (server local clock).</summary>
    public static DateOnly TodayLocal(DateTime? nowLocal = null)
    {
        var n = nowLocal ?? DateTime.Now;
        return DateOnly.FromDateTime(n);
    }

    /// <summary>Last bookable calendar date for the given window months.</summary>
    public static DateOnly MaxBookableDate(int bookingWindowMonths, DateTime? nowLocal = null)
    {
        var months = ClampMonths(bookingWindowMonths);
        var n = nowLocal ?? DateTime.Now;
        // End of (current month + months): day 0 of month (current+months+1)
        var end = new DateTime(n.Year, n.Month, 1).AddMonths(months + 1).AddDays(-1);
        return DateOnly.FromDateTime(end);
    }

    public static void EnsureDateInWindow(
        DateOnly appointmentDate,
        int bookingWindowMonths,
        DateTime? nowLocal = null)
    {
        var min = TodayLocal(nowLocal);
        var max = MaxBookableDate(bookingWindowMonths, nowLocal);
        if (appointmentDate < min || appointmentDate > max)
        {
            throw new ArgumentException(
                $"Date is outside the booking window. Book from {min:yyyy-MM-dd} to {max:yyyy-MM-dd}.");
        }
    }

    /// <summary>Resolve appointment calendar date from request fields.</summary>
    public static DateOnly ResolveAppointmentDate(DateOnly? appointmentDate, DateTimeOffset startDatetime)
    {
        if (appointmentDate.HasValue)
            return appointmentDate.Value;
        return DateOnly.FromDateTime(startDatetime.LocalDateTime);
    }
}

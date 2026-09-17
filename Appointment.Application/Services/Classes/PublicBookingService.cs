using Appointment.Application.Helpers;
using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Appointments.Requests;
using Appointment.Domain.DTOs.Appointments.Responses;
using Appointment.Domain.DTOs.Customers.Requests;
using Appointment.Domain.DTOs.Customers.Responses;
using Appointment.Domain.DTOs.Professionals.Requests;
using Appointment.Domain.DTOs.Professionals.Responses;
using Appointment.Domain.DTOs.PublicBook.Requests;
using Appointment.Domain.DTOs.PublicBook.Responses;
using Appointment.Domain.DTOs.Services.Requests;
using Appointment.Domain.DTOs.Services.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Appointment.Application.Services.Classes;

/// <summary>
/// QR / public self-booking. Org comes from the booking link (not SoftOnCloud JWT).
/// Uses local product DB when SoftOnCloud:UseProductConnectionDb = false.
/// </summary>
public sealed class PublicBookingService : IPublicBookingService
{
    private readonly ICustomerService _customerService;
    private readonly IServiceCatalogService _serviceCatalogService;
    private readonly IProfessionalService _professionalService;
    private readonly IAppointmentService _appointmentService;
    private readonly ISettingsService _settingsService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PublicBookingService> _logger;

    public PublicBookingService(
        ICustomerService customerService,
        IServiceCatalogService serviceCatalogService,
        IProfessionalService professionalService,
        IAppointmentService appointmentService,
        ISettingsService settingsService,
        IConfiguration configuration,
        ILogger<PublicBookingService> logger)
    {
        _customerService = customerService;
        _serviceCatalogService = serviceCatalogService;
        _professionalService = professionalService;
        _appointmentService = appointmentService;
        _settingsService = settingsService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<PublicBookContextDto> GetContextAsync(int orgId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);

        var services = await _serviceCatalogService.GetServicesAsync(
            orgId,
            new GetServicesRequest { IsActive = true, Limit = 1, Offset = 0 },
            cancellationToken);

        var ready = services.TotalCount > 0 || services.Items.Count > 0;

        var rules = await _settingsService.GetRulesAsync(orgId, cancellationToken);
        var months = BookingWindowHelper.ClampMonths(rules.BookingWindowMonths);
        var min = BookingWindowHelper.TodayLocal();
        var max = BookingWindowHelper.MaxBookableDate(months);

        return new PublicBookContextDto
        {
            Title = "Book an appointment",
            Message = ready
                ? "Register as a new customer or continue if you already visited us."
                : "This organisation has no active services yet. Please contact the clinic.",
            Ready = ready,
            BookingWindowMonths = months,
            BookMinDate = min.ToString("yyyy-MM-dd"),
            BookMaxDate = max.ToString("yyyy-MM-dd"),
        };
    }

    public async Task<CreateCustomerResponse> RegisterCustomerAsync(
        PublicCreateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        ValidateOrg(request.OrgId);

        if (string.IsNullOrWhiteSpace(request.PhoneMobile))
            throw new ArgumentException("Mobile phone is required.");

        var createdBy = GetSystemUserId();
        var create = new CreateCustomerRequest
        {
            DisplayName = request.DisplayName,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneMobile = request.PhoneMobile.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            Gender = request.Gender,
            DateOfBirth = request.DateOfBirth,
            Remarks = request.Remarks,
            Status = "active",
            AcquisitionSource = "qr_self_booking",
            CustomerSince = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        _logger.LogInformation("Public register customer org={OrgId}", request.OrgId);
        return await _customerService.CreateCustomerAsync(request.OrgId, createdBy, create, cancellationToken);
    }

    public Task<CustomerDetailDto?> FindCustomerByPhoneAsync(
        int orgId,
        string phone,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        return _customerService.FindCustomerByPhoneAsync(orgId, phone, cancellationToken);
    }

    public Task<ServiceListResponse> GetServicesAsync(int orgId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        return _serviceCatalogService.GetServicesAsync(
            orgId,
            new GetServicesRequest { IsActive = true, Limit = 100, Offset = 0 },
            cancellationToken);
    }

    public Task<ProfessionalListResponse> GetProfessionalsAsync(
        int orgId,
        int productId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (productId <= 0)
            throw new ArgumentException("Service is required.", nameof(productId));

        return _professionalService.GetProfessionalsAsync(
            orgId,
            new GetProfessionalsRequest
            {
                Status = "active",
                ProductId = productId,
                Limit = 100,
                Offset = 0
            },
            cancellationToken);
    }

    public Task<ProfessionalScheduleGridDto> GetScheduleGridAsync(
        int orgId,
        int employeeId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (employeeId <= 0)
            throw new ArgumentException("Professional is required.", nameof(employeeId));

        return _professionalService.GetScheduleGridAsync(
            orgId, fromDate, toDate, [employeeId], cancellationToken);
    }

    public Task<ProfessionalScheduleDto> GetScheduleAsync(
        int orgId,
        int employeeId,
        DateOnly? asOfDate = null,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (employeeId <= 0)
            throw new ArgumentException("Professional is required.", nameof(employeeId));

        return _professionalService.GetScheduleAsync(orgId, employeeId, asOfDate, cancellationToken);
    }

    public async Task<AppointmentDetailDto> CreateAppointmentAsync(
        PublicCreateAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        ValidateOrg(request.OrgId);

        // Ensure customer belongs to this org
        var customer = await _customerService.GetCustomerByIdAsync(
            request.OrgId, (int)request.CustomerId, cancellationToken);
        if (customer is null)
            throw new ArgumentException("Customer was not found for this organisation.");

        var createdBy = GetSystemUserId();
        var create = new CreateAppointmentRequest
        {
            CustomerId = request.CustomerId,
            ProfessionalId = request.ProfessionalId,
            ProductId = request.ProductId,
            BranchId = request.BranchId ?? (customer.BranchId.HasValue ? customer.BranchId.Value : null),
            StartDatetime = request.StartDatetime,
            EndDatetime = request.EndDatetime,
            AppointmentDate = request.AppointmentDate,
            Notes = request.Notes,
            Amount = request.Amount,
            ReminderMinutesBefore = request.ReminderMinutesBefore,
            Status = "scheduled",
            Source = "qr_self",
            Priority = "normal",
            PaymentStatus = "unpaid",
            IsAllDay = false,
            Visibility = "private"
        };

        _logger.LogInformation(
            "Public create appointment org={OrgId} customer={CustomerId}",
            request.OrgId, request.CustomerId);

        return await _appointmentService.CreateAppointmentAsync(
            request.OrgId, createdBy, create, cancellationToken);
    }

    private static void ValidateOrg(int orgId)
    {
        if (orgId <= 0)
            throw new ArgumentException("Organisation id is required.", nameof(orgId));
    }

    private int GetAppId()
    {
        var appId = _configuration.GetValue<int?>("Appointment:AppId")
                    ?? _configuration.GetValue<int?>("Appointment:ProductId")
                    ?? 25;
        return appId;
    }

    private int GetSystemUserId()
    {
        var id = _configuration.GetValue<int?>("Appointment:PublicBookingSystemUserId") ?? 1;
        return id > 0 ? id : 1;
    }
}

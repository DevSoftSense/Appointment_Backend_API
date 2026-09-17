using Appointment.Domain.DTOs.Appointments.Responses;
using Appointment.Domain.DTOs.Customers.Responses;
using Appointment.Domain.DTOs.Professionals.Responses;
using Appointment.Domain.DTOs.PublicBook.Requests;
using Appointment.Domain.DTOs.PublicBook.Responses;
using Appointment.Domain.DTOs.Services.Responses;

namespace Appointment.Application.Services.Interfaces;

public interface IPublicBookingService
{
    Task<PublicBookContextDto> GetContextAsync(int orgId, CancellationToken cancellationToken = default);

    Task<CreateCustomerResponse> RegisterCustomerAsync(
        PublicCreateCustomerRequest request,
        CancellationToken cancellationToken = default);

    Task<CustomerDetailDto?> FindCustomerByPhoneAsync(
        int orgId,
        string phone,
        CancellationToken cancellationToken = default);

    Task<ServiceListResponse> GetServicesAsync(
        int orgId,
        CancellationToken cancellationToken = default);

    Task<ProfessionalListResponse> GetProfessionalsAsync(
        int orgId,
        int productId,
        CancellationToken cancellationToken = default);

    Task<ProfessionalScheduleGridDto> GetScheduleGridAsync(
        int orgId,
        int employeeId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default);

    Task<ProfessionalScheduleDto> GetScheduleAsync(
        int orgId,
        int employeeId,
        DateOnly? asOfDate = null,
        CancellationToken cancellationToken = default);

    Task<AppointmentDetailDto> CreateAppointmentAsync(
        PublicCreateAppointmentRequest request,
        CancellationToken cancellationToken = default);
}

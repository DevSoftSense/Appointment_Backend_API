using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Appointments.Responses;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Appointment.Application.Services.Classes;

public sealed class AppointmentDocumentService : IAppointmentDocumentService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".png", ".jpg", ".jpeg", ".gif", ".webp",
        ".doc", ".docx", ".xls", ".xlsx", ".txt", ".csv"
    };

    private readonly IAppointmentDocumentRepository _repository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AppointmentDocumentService> _logger;

    public AppointmentDocumentService(
        IAppointmentDocumentRepository repository,
        IAppointmentRepository appointmentRepository,
        IWebHostEnvironment env,
        IConfiguration configuration,
        ILogger<AppointmentDocumentService> logger)
    {
        _repository = repository;
        _appointmentRepository = appointmentRepository;
        _env = env;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AppointmentDocumentDto>> ListAsync(
        int orgId, long appointmentId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (appointmentId <= 0)
            throw new ArgumentException("Appointment id is required.");

        var appId = GetAppId();
        await EnsureAppointmentAsync(orgId, appointmentId, cancellationToken);

        var items = await _repository.ListAsync(orgId, appId, appointmentId, cancellationToken);
        foreach (var item in items)
            item.FileUrl = ToPublicUrl(item.FilePath);
        return items;
    }

    public async Task<IReadOnlyList<AppointmentDocumentDto>> ListByCustomerAsync(
        int orgId, long customerId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (customerId <= 0)
            throw new ArgumentException("Customer id is required.");

        var appId = GetAppId();
        var items = await _repository.ListByCustomerAsync(orgId, appId, customerId, cancellationToken);
        foreach (var item in items)
            item.FileUrl = ToPublicUrl(item.FilePath);
        return items;
    }

    public async Task<AppointmentDocumentDto> UploadAsync(
        int orgId,
        long uploadedBy,
        long appointmentId,
        IFormFile file,
        string? documentType = null,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (uploadedBy <= 0)
            throw new ArgumentException("Uploader user is required.");
        if (appointmentId <= 0)
            throw new ArgumentException("Appointment id is required.");
        if (file is null || file.Length <= 0)
            throw new ArgumentException("A non-empty file is required.");

        var maxMb = _configuration.GetValue("Appointment:Documents:MaxFileSizeMb", 2);
        var maxBytes = Math.Max(1, maxMb) * 1024L * 1024L;
        if (file.Length > maxBytes)
            throw new ArgumentException($"File is too large. Maximum size is {maxMb} MB.");

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
            throw new ArgumentException(
                "File type not allowed. Use PDF, image, Word, Excel, TXT, or CSV.");

        var appId = GetAppId();
        await EnsureAppointmentAsync(orgId, appointmentId, cancellationToken);

        var relativeFolder = Path.Combine(
            "Uploads", "Appointment_Documents", orgId.ToString(), appointmentId.ToString());
        var webRoot = string.IsNullOrWhiteSpace(_env.WebRootPath)
            ? Path.Combine(_env.ContentRootPath, "wwwroot")
            : _env.WebRootPath;
        var physicalFolder = Path.Combine(webRoot, relativeFolder);
        Directory.CreateDirectory(physicalFolder);

        var safeOriginal = SanitizeFileName(Path.GetFileNameWithoutExtension(file.FileName));
        var storedName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}_{safeOriginal}{ext.ToLowerInvariant()}";
        var physicalPath = Path.Combine(physicalFolder, storedName);
        var relativePath = Path.Combine(relativeFolder, storedName).Replace('\\', '/');

        await using (var stream = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        try
        {
            var displayName = string.IsNullOrWhiteSpace(file.FileName)
                ? storedName
                : Path.GetFileName(file.FileName);

            var created = await _repository.CreateAsync(
                orgId,
                appId,
                appointmentId,
                uploadedBy,
                displayName,
                relativePath,
                string.IsNullOrWhiteSpace(documentType) ? GuessDocumentType(ext) : documentType.Trim(),
                cancellationToken: cancellationToken);

            created.FileUrl = ToPublicUrl(created.FilePath);
            return created;
        }
        catch
        {
            TryDeletePhysical(physicalPath);
            throw;
        }
    }

    public async Task DeleteAsync(
        int orgId, long appointmentId, long documentId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (appointmentId <= 0)
            throw new ArgumentException("Appointment id is required.");
        if (documentId <= 0)
            throw new ArgumentException("Document id is required.");

        var appId = GetAppId();
        var (deleted, filePath) = await _repository.DeleteAsync(
            orgId, appId, appointmentId, documentId, cancellationToken);

        if (!deleted)
            throw new ArgumentException("Document not found.");

        if (!string.IsNullOrWhiteSpace(filePath))
        {
            var webRoot = string.IsNullOrWhiteSpace(_env.WebRootPath)
                ? Path.Combine(_env.ContentRootPath, "wwwroot")
                : _env.WebRootPath;
            var physical = Path.Combine(webRoot, filePath.Replace('/', Path.DirectorySeparatorChar));
            TryDeletePhysical(physical);
        }
    }

    private async Task EnsureAppointmentAsync(int orgId, long appointmentId, CancellationToken cancellationToken)
    {
        var apt = await _appointmentRepository.GetAppointmentByIdAsync(
            orgId, GetAppId(), appointmentId, cancellationToken);
        if (apt is null)
            throw new ArgumentException("Appointment not found for this organization.");
    }

    private int GetAppId()
    {
        var appId = _configuration.GetValue<int?>("Appointment:AppId")
                    ?? _configuration.GetValue<int?>("Appointment:ProductId")
                    ?? 0;
        if (appId <= 0)
            throw new InvalidOperationException("Appointment:AppId (or ProductId) is not configured.");
        return appId;
    }

    private static void ValidateOrg(int orgId)
    {
        if (orgId <= 0)
            throw new ArgumentException("Organisation ID is required.");
    }

    private static string ToPublicUrl(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return string.Empty;
        var path = relativePath.Replace('\\', '/').TrimStart('/');
        return "/" + path;
    }

    private static string SanitizeFileName(string name)
    {
        var cleaned = string.Concat(name.Where(ch =>
            char.IsLetterOrDigit(ch) || ch is '-' or '_' or '.' or ' '));
        cleaned = cleaned.Trim().Replace(' ', '_');
        if (cleaned.Length > 80)
            cleaned = cleaned[..80];
        return string.IsNullOrWhiteSpace(cleaned) ? "file" : cleaned;
    }

    private static string GuessDocumentType(string ext) =>
        ext.ToLowerInvariant() switch
        {
            ".pdf" => "pdf",
            ".png" or ".jpg" or ".jpeg" or ".gif" or ".webp" => "image",
            ".doc" or ".docx" => "word",
            ".xls" or ".xlsx" or ".csv" => "spreadsheet",
            ".txt" => "text",
            _ => "other"
        };

    private void TryDeletePhysical(string physicalPath)
    {
        try
        {
            if (File.Exists(physicalPath))
                File.Delete(physicalPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not delete physical document file {Path}", physicalPath);
        }
    }
}

using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Appointment.Infrastructure.Email;

public interface ISmtpEmailSender
{
    Task SendEmailAsync(string toEmail, string subject, string bodyHtml, CancellationToken cancellationToken = default);

    Task SendEmailAsync(
        string toEmail,
        string subject,
        string bodyHtml,
        byte[]? attachmentBytes,
        string? attachmentFileName,
        string? attachmentContentType,
        CancellationToken cancellationToken = default);
}

public sealed class SmtpEmailSender : ISmtpEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task SendEmailAsync(
        string toEmail, string subject, string bodyHtml, CancellationToken cancellationToken = default) =>
        SendEmailAsync(toEmail, subject, bodyHtml, null, null, null, cancellationToken);

    public async Task SendEmailAsync(
        string toEmail,
        string subject,
        string bodyHtml,
        byte[]? attachmentBytes,
        string? attachmentFileName,
        string? attachmentContentType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
            throw new ArgumentException("toEmail is required", nameof(toEmail));

        var smtpHost = _configuration["Smtp:Host"];
        var smtpPort = int.TryParse(_configuration["Smtp:Port"], out var port) ? port : 587;
        var smtpUser = _configuration["Smtp:UserName"];
        var smtpPass = _configuration["Smtp:Password"];
        var enableSsl = !bool.TryParse(_configuration["Smtp:EnableSsl"], out var ssl) || ssl;
        var fromEmail = string.IsNullOrWhiteSpace(_configuration["Smtp:From"])
            ? "noreply@softoncloud.com"
            : _configuration["Smtp:From"]!;

        if (string.IsNullOrWhiteSpace(smtpHost)
            || string.IsNullOrWhiteSpace(smtpUser)
            || string.IsNullOrWhiteSpace(smtpPass))
        {
            throw new InvalidOperationException("SMTP is not configured (Smtp:Host/UserName/Password).");
        }

        using var message = new MailMessage(fromEmail, toEmail.Trim())
        {
            Subject = string.IsNullOrWhiteSpace(subject) ? "SoftOnCloud Appointment" : subject.Trim(),
            Body = bodyHtml ?? "",
            IsBodyHtml = true
        };

        Attachment? attachment = null;
        if (attachmentBytes is { Length: > 0 } && !string.IsNullOrWhiteSpace(attachmentFileName))
        {
            var stream = new MemoryStream(attachmentBytes);
            attachment = new Attachment(
                stream,
                attachmentFileName,
                string.IsNullOrWhiteSpace(attachmentContentType) ? "application/octet-stream" : attachmentContentType);
            message.Attachments.Add(attachment);
        }

        try
        {
            using var smtp = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(smtpUser, smtpPass)
            };

            await Task.Run(() => smtp.Send(message), cancellationToken);
            _logger.LogInformation("Email sent to {To} (attachment={HasAttachment})", toEmail, attachment != null);
        }
        finally
        {
            attachment?.Dispose();
        }
    }
}

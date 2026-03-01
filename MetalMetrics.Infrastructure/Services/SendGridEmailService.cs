using MetalMetrics.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace MetalMetrics.Infrastructure.Services;

public class SendGridEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<SendGridEmailService> _logger;

    public SendGridEmailService(IConfiguration config, ILogger<SendGridEmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        var subject = "Reset Your MetalMetrics Password";
        var htmlBody = BuildEmailTemplate(
            "Password Reset",
            "<p>We received a request to reset your password. Click the button below to set a new password.</p>",
            "Reset Password",
            resetLink,
            "<p style=\"color:#999;font-size:13px;\">If you didn't request this, you can safely ignore this email. This link expires in 24 hours.</p>"
        );
        await SendGenericEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task SendEmailConfirmationAsync(string toEmail, string confirmLink)
    {
        var subject = "Confirm Your MetalMetrics Email";
        var htmlBody = BuildEmailTemplate(
            "Email Confirmation",
            "<p>Welcome to MetalMetrics! Please confirm your email address by clicking the button below.</p>",
            "Confirm Email",
            confirmLink,
            "<p style=\"color:#999;font-size:13px;\">If you didn't create an account, you can safely ignore this email.</p>"
        );
        await SendGenericEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task SendGenericEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var apiKey = _config["SendGrid:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("SendGrid API key not configured. Email to {Email} not sent.", toEmail);
            return;
        }

        var client = new SendGridClient(apiKey);
        var senderEmail = _config["SendGrid:SenderEmail"] ?? "noreply@metalmetrics.app";
        var senderName = _config["SendGrid:SenderName"] ?? "MetalMetrics";
        var from = new EmailAddress(senderEmail, senderName);
        var to = new EmailAddress(toEmail);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlBody);

        var response = await client.SendEmailAsync(msg);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Body.ReadAsStringAsync();
            _logger.LogError("SendGrid email failed. Status: {Status}, Body: {Body}", response.StatusCode, body);
        }
    }

    private static string BuildEmailTemplate(string heading, string bodyHtml, string ctaText, string ctaUrl, string footerHtml)
    {
        return $@"
<!DOCTYPE html>
<html>
<head><meta charset=""utf-8""></head>
<body style=""margin:0;padding:0;background:#f4f4f4;font-family:Arial,sans-serif;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f4f4f4;padding:40px 0;"">
    <tr><td align=""center"">
      <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background:#fff;border-radius:8px;overflow:hidden;"">
        <tr>
          <td style=""background:#1a1a2e;padding:24px;text-align:center;"">
            <span style=""color:#f39c12;font-size:24px;font-weight:bold;"">Metal</span><span style=""color:#fff;font-size:24px;font-weight:bold;"">Metrics</span>
          </td>
        </tr>
        <tr>
          <td style=""padding:32px;"">
            <h2 style=""margin:0 0 16px;color:#333;"">{heading}</h2>
            {bodyHtml}
            <div style=""text-align:center;margin:32px 0;"">
              <a href=""{ctaUrl}"" style=""background:#f39c12;color:#1a1a2e;padding:14px 32px;text-decoration:none;border-radius:6px;font-weight:bold;display:inline-block;"">{ctaText}</a>
            </div>
            {footerHtml}
          </td>
        </tr>
        <tr>
          <td style=""background:#1a1a2e;padding:16px;text-align:center;"">
            <span style=""color:#999;font-size:12px;"">MetalMetrics &copy; 2026</span>
          </td>
        </tr>
      </table>
    </td></tr>
  </table>
</body>
</html>";
    }
}

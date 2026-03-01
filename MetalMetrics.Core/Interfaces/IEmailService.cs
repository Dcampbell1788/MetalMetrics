namespace MetalMetrics.Core.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
    Task SendEmailConfirmationAsync(string toEmail, string confirmLink);
    Task SendGenericEmailAsync(string toEmail, string subject, string htmlBody);
}

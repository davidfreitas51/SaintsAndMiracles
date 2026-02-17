using System.Net;
using System.Net.Mail;
using Core.Models;
using Microsoft.AspNetCore.Identity;

namespace API.Helpers;

public class EmailSender(IConfiguration _configuration, ILogger<EmailSender> logger) : IEmailSender<AppUser>
{
    public async Task SendConfirmationLinkAsync(AppUser user, string email, string confirmationLink)
    {
        string template = LoadTemplate("ConfirmationEmail.html");

        string title, message, buttonText;

        if (user.EmailConfirmed)
        {
            title = "Confirm Your New Email";
            message = "You requested to change your email. Click the button below to confirm your new email.";
            buttonText = "Confirm New Email";
        }
        else
        {
            title = "Confirm Your Email";
            message = "Please confirm your email by clicking the button below.";
            buttonText = "Confirm Email";
        }

        string body = template
            .Replace("{{FirstName}}", user.FirstName)
            .Replace("{{ConfirmationLink}}", confirmationLink)
            .Replace("{{EmailActionTitle}}", title)
            .Replace("{{EmailActionMessage}}", message)
            .Replace("{{EmailActionButtonText}}", buttonText);

        string subject = title;

        await SendEmailAsync(email, subject, body);
    }


    public async Task SendPasswordResetCodeAsync(AppUser user, string email, string resetCode)
    {
        string subject = "Password Reset Code";
        string template = LoadTemplate("ResetPasswordCode.html");

        string body = template
            .Replace("{{FirstName}}", user.FirstName)
            .Replace("{{ResetCode}}", resetCode);

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendPasswordResetLinkAsync(AppUser user, string email, string resetLink)
    {
        string subject = "Reset your password";
        string template = LoadTemplate("ResetPasswordLink.html");

        string body = template
            .Replace("{{FirstName}}", user.FirstName)
            .Replace("{{ResetPasswordLink}}", resetLink);

        await SendEmailAsync(email, subject, body);
    }

    private string LoadTemplate(string templateName)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Helpers", "EmailTemplates", templateName);
        return File.ReadAllText(path);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            var smtpHost = _configuration["Smtp:Host"]!;
            var smtpPort = int.Parse(_configuration["Smtp:Port"]!);
            var smtpUser = _configuration["Smtp:User"]!;
            var smtpPass = _configuration["Smtp:Pass"]!;
            var fromEmail = _configuration["Smtp:From"]!;
            var fromName = _configuration["Smtp:FromName"] ?? "Saints And Miracles";

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);

            logger.LogInformation("Email sent successfully. To={MaskedEmail}, Subject={Subject}", MaskEmail(toEmail), subject);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending email. To={MaskedEmail}, Subject={Subject}", MaskEmail(toEmail), subject);
            throw;
        }
    }

    private static string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            return "***";

        var parts = email.Split('@');
        var localPart = parts[0];
        var domain = parts[1];

        var maskedLocal = localPart.Length <= 2
            ? new string('*', localPart.Length)
            : localPart[0] + new string('*', localPart.Length - 2) + localPart[^1];

        return $"{maskedLocal}@{domain}";
    }
}
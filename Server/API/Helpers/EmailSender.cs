using System.Net;
using System.Net.Mail;
using Core.Models;
using Microsoft.AspNetCore.Identity;

namespace API.Helpers;

public class EmailSender(IConfiguration _configuration) : IEmailSender<AppUser>
{
    public async Task SendConfirmationLinkAsync(AppUser user, string email, string confirmationLink)
    {
        string subject = "Confirm your email";
        string template = LoadTemplate("ConfirmationEmail.html");

        string body = template
            .Replace("{{FirstName}}", user.FirstName)
            .Replace("{{ConfirmationLink}}", confirmationLink);

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
            .Replace("{{ResetLink}}", resetLink);

        await SendEmailAsync(email, subject, body);
    }

    private string LoadTemplate(string templateName)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Helpers", "EmailTemplates", templateName);
        return File.ReadAllText(path);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var smtpHost = _configuration["Smtp:Host"];
        var smtpPort = int.Parse(_configuration["Smtp:Port"]);
        var smtpUser = _configuration["Smtp:User"];
        var smtpPass = _configuration["Smtp:Pass"];
        var fromEmail = _configuration["Smtp:From"];

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(fromEmail),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mailMessage.To.Add(toEmail);

        await client.SendMailAsync(mailMessage);
    }
}

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
        string body = $"Hello {user.FirstName},<br><br>" +
                      $"Please confirm your email by clicking the link below:<br>" +
                      $"<a href='{confirmationLink}'>Confirm Email</a><br><br>" +
                      "Thank you!";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendPasswordResetCodeAsync(AppUser user, string email, string resetCode)
    {
        string subject = "Password Reset Code";
        string body = $"Hello {user.FirstName},<br><br>" +
                      $"Your password reset code is: <b>{resetCode}</b><br><br>" +
                      "If you didn't request this, ignore this email.";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendPasswordResetLinkAsync(AppUser user, string email, string resetLink)
    {
        string subject = "Reset your password";
        string body = $"Hello {user.FirstName},<br><br>" +
                      $"You can reset your password by clicking the link below:<br>" +
                      $"<a href='{resetLink}'>Reset Password</a><br><br>" +
                      "If you didn't request this, ignore this email.";

        await SendEmailAsync(email, subject, body);
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

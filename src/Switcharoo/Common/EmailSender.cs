
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Switcharoo.Common;

public sealed class EmailSender(IOptions<SmtpSettings> options) : IEmailSender
{
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            var smtpSettings = options.Value;

            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(smtpSettings.SenderName, smtpSettings.Sender));
            mimeMessage.To.Add(new MailboxAddress(email, email));
            mimeMessage.Subject = subject;
            mimeMessage.Body = new TextPart("html")
            {
                Text = htmlMessage,
            };

            using var smtpClient = new SmtpClient();
            await smtpClient.ConnectAsync(smtpSettings.Host, smtpSettings.Port, smtpSettings.EnableSsl);
            await smtpClient.AuthenticateAsync(smtpSettings.Username, smtpSettings.Password);
            await smtpClient.SendAsync(mimeMessage);
            await smtpClient.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            // ignored for now
        }
    }
}

using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Ticketing.Application.Interfaces.Email;
using MailKit.Net.Smtp;

namespace Ticketing.Infrastructure.Email;

public class EmailService : IEmailService
{
    readonly EmailSettings _emailSettings;
    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }
    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(
            _emailSettings.FromName,
            _emailSettings.FromEmail));

        message.To.Add(MailboxAddress.Parse(to));

        message.Subject = subject;

        message.Body = new BodyBuilder
        {
            HtmlBody = body
        }.ToMessageBody();

        using var smtpClient = new SmtpClient();

        await smtpClient.ConnectAsync(
            _emailSettings.Host,
            _emailSettings.Port,
            SecureSocketOptions.StartTls,
            cancellationToken);

        await smtpClient.AuthenticateAsync(
            _emailSettings.Username,
            _emailSettings.Password,
            cancellationToken);

        await smtpClient.SendAsync(message, cancellationToken);

        await smtpClient.DisconnectAsync(true, cancellationToken);
    }
}

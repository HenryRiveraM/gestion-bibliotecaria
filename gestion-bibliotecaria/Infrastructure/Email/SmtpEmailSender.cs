using System.Net;
using System.Net.Mail;
using gestion_bibliotecaria.Domain.Common;
using gestion_bibliotecaria.Domain.Ports;
using gestion_bibliotecaria.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace gestion_bibliotecaria.Infrastructure.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;

    public SmtpEmailSender(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(_settings.FromAddress))
        {
            throw new InvalidOperationException("Email.FromAddress no esta configurado.");
        }

        if (string.IsNullOrWhiteSpace(_settings.Smtp.Host))
        {
            throw new InvalidOperationException("Email:Smtp:Host no esta configurado.");
        }

        if (string.IsNullOrWhiteSpace(_settings.Smtp.Username) || string.IsNullOrWhiteSpace(_settings.Smtp.Password))
        {
            throw new InvalidOperationException("Credenciales SMTP no configuradas.");
        }

        using var client = new SmtpClient(_settings.Smtp.Host, _settings.Smtp.Port)
        {
            EnableSsl = _settings.Smtp.EnableSsl,
            Credentials = new NetworkCredential(_settings.Smtp.Username, _settings.Smtp.Password)
        };

        using var mail = new MailMessage
        {
            From = new MailAddress(_settings.FromAddress, _settings.FromName),
            Subject = message.Subject,
            Body = message.PlainTextContent,
            IsBodyHtml = false
        };

        mail.To.Add(message.To);

        await client.SendMailAsync(mail, cancellationToken);
    }
}

using gestion_bibliotecaria.Domain.Common;
using gestion_bibliotecaria.Domain.Ports;
using Microsoft.Extensions.Logging;

namespace gestion_bibliotecaria.Infrastructure.Email;

public class DevelopmentEmailSender : IEmailSender
{
    private readonly ILogger<DevelopmentEmailSender> _logger;

    public DevelopmentEmailSender(ILogger<DevelopmentEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogWarning(
            "[DEV EMAIL MODE] Correo no enviado a proveedor real. To: {To}, Subject: {Subject}, Body: {Body}",
            message.To,
            message.Subject,
            message.PlainTextContent);

        return Task.CompletedTask;
    }
}

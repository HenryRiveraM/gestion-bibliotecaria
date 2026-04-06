using gestion_bibliotecaria.Domain.Common;

namespace gestion_bibliotecaria.Domain.Ports;

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}

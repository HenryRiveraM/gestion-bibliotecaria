using gestion_bibliotecaria.Domain.Ports;
using gestion_bibliotecaria.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace gestion_bibliotecaria.Infrastructure.Email;

public static class EmailSenderFactory
{
    public static IEmailSender Create(IServiceProvider serviceProvider)
    {
        var settings = serviceProvider.GetRequiredService<IOptions<EmailSettings>>().Value;

        if (settings.UseDevelopmentMode)
        {
            return ActivatorUtilities.CreateInstance<DevelopmentEmailSender>(serviceProvider);
        }

        if (string.Equals(settings.Provider, "Smtp", StringComparison.OrdinalIgnoreCase))
        {
            return ActivatorUtilities.CreateInstance<SmtpEmailSender>(serviceProvider);
        }

        if (string.Equals(settings.Provider, "Api", StringComparison.OrdinalIgnoreCase))
        {
            return ActivatorUtilities.CreateInstance<HttpApiEmailSender>(serviceProvider);
        }

        throw new InvalidOperationException($"Proveedor de email no soportado: {settings.Provider}");
    }
}

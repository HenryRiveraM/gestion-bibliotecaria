using System.Net.Http.Json;
using gestion_bibliotecaria.Domain.Common;
using gestion_bibliotecaria.Domain.Ports;
using gestion_bibliotecaria.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace gestion_bibliotecaria.Infrastructure.Email;

public class HttpApiEmailSender : IEmailSender
{
    private readonly HttpClient _httpClient;
    private readonly EmailSettings _settings;

    public HttpApiEmailSender(HttpClient httpClient, IOptions<EmailSettings> options)
    {
        _httpClient = httpClient;
        _settings = options.Value;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiUrl))
        {
            throw new InvalidOperationException("Email:ApiUrl no esta configurado.");
        }

        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new InvalidOperationException("Email:ApiKey no esta configurado para proveedor API.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, _settings.ApiUrl);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        request.Content = JsonContent.Create(new
        {
            from = new { email = _settings.FromAddress, name = _settings.FromName },
            to = new[] { new { email = message.To } },
            subject = message.Subject,
            text = message.PlainTextContent
        });

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Error enviando email por API. Status: {(int)response.StatusCode}. Detalle: {body}");
        }
    }
}

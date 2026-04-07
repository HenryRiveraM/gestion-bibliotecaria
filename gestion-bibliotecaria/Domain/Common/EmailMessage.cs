namespace gestion_bibliotecaria.Domain.Common;

public class EmailMessage
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string PlainTextContent { get; set; } = string.Empty;
}

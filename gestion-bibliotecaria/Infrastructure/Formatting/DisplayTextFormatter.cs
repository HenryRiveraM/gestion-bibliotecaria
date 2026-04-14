using System.Globalization;

namespace gestion_bibliotecaria.Infrastructure.Formatting;

public static class DisplayTextFormatter
{
    private static readonly TextInfo TextInfoEs = CultureInfo.GetCultureInfo("es-ES").TextInfo;

    public static string ToDisplayName(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var compactado = string.Join(' ', value
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        return TextInfoEs.ToTitleCase(TextInfoEs.ToLower(compactado));
    }
}
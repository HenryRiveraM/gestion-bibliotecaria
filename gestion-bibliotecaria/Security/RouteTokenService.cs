using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;

namespace gestion_bibliotecaria.Security;

public class RouteTokenService
{
    private const string ProtectorPurpose = "gestion-bibliotecaria.route-token.v1";
    private readonly IDataProtector _protector;

    public RouteTokenService(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);
    }

    public string CrearToken(int id)
    {
        var idTexto = id.ToString(CultureInfo.InvariantCulture);
        var protegido = _protector.Protect(idTexto);
        return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(protegido));
    }

    public bool TryObtenerId(string? token, out int id)
    {
        id = 0;

        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        try
        {
            var protegido = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var idTexto = _protector.Unprotect(protegido);
            return int.TryParse(idTexto, NumberStyles.None, CultureInfo.InvariantCulture, out id);
        }
        catch
        {
            return false;
        }
    }
}

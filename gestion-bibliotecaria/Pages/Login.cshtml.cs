using gestion_bibliotecaria.Aplicacion.Interfaces;
using gestion_bibliotecaria.Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace gestion_bibliotecaria.Pages;

public class LoginModel : PageModel
{
    private readonly IUsuarioServicio _usuarioServicio;

    [BindProperty]
    public string NombreUsuario { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string? MensajeError { get; set; }

    public LoginModel(IUsuarioServicio usuarioServicio)
    {
        _usuarioServicio = usuarioServicio;
    }

    public IActionResult OnGet()
    {
        return Redirect("/");
    }

    public IActionResult OnPost()
    {
        var resultado = _usuarioServicio.Login(NombreUsuario, Password);

        if (resultado.IsFailure)
        {
            TempData["LoginError"] = resultado.Error.Message;
            return Redirect("/");
        }

        var usuario = resultado.Value;

        HttpContext.Session.SetString(SessionKeys.UsuarioId, usuario.UsuarioId.ToString());
        HttpContext.Session.SetString(SessionKeys.NombreUsuario, usuario.NombreUsuario ?? string.Empty);
        HttpContext.Session.SetString(SessionKeys.Rol, usuario.Rol);

        Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["Expires"] = "0";

        return Redirect("/");
    }

    public IActionResult OnPostLogout()
    {
        HttpContext.Session.Clear();
        Response.Cookies.Delete(".AspNetCore.Session");

        Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["Expires"] = "0";

        return Redirect("/");
    }
}
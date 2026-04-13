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

        return Redirect("/");
    }

    public IActionResult OnPostLogout()
    {
        HttpContext.Session.Clear();
        return Redirect("/");
    }
}
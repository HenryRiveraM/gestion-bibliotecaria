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
        if (!string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionKeys.UsuarioId)))
        {
            return RedirectToPage("/Pages/Index");
        }

        return Page();
    }

    public IActionResult OnPost()
    {
        var resultado = _usuarioServicio.Login(NombreUsuario, Password);

        if (resultado.IsFailure)
        {
            MensajeError = resultado.Error.Message;
            return Page();
        }

        var usuario = resultado.Value;

        HttpContext.Session.SetString(SessionKeys.UsuarioId, usuario.UsuarioId.ToString());
        HttpContext.Session.SetString(SessionKeys.NombreUsuario, usuario.NombreUsuario);
        HttpContext.Session.SetString(SessionKeys.Rol, usuario.Rol);

        return RedirectToPage("/Pages/Index");
    }

    public IActionResult OnPostLogout()
    {
        HttpContext.Session.Clear();
        return RedirectToPage("/Pages/Login");
    }
}

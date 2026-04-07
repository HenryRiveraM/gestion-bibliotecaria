using System.Data;
using gestion_bibliotecaria.Aplicacion.Interfaces;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace gestion_bibliotecaria.Pages.Usuarios;

public class IndexModel : PageModel
{
    private readonly IUsuarioServicio _usuarioServicio;
    private readonly RouteTokenService _routeTokenService;

    public List<UsuarioListadoItem> Usuarios { get; set; } = new();

    [BindProperty]
    public Usuario NuevoUsuario { get; set; } = new();

    [BindProperty]
    public string RolNuevoUsuario { get; set; } = Usuario.RolBibliotecario;

    public string? MensajeError { get; set; }
    public string? MensajeOk { get; set; }

    public IndexModel(IUsuarioServicio usuarioServicio, RouteTokenService routeTokenService)
    {
        _usuarioServicio = usuarioServicio;
        _routeTokenService = routeTokenService;
    }

    public IActionResult OnGet()
    {
        if (!EsAdmin())
        {
            return RedirectToPage("/Pages/Index");
        }

        CargarUsuarios();
        return Page();
    }

    public async Task<IActionResult> OnPostCrearAsync(CancellationToken cancellationToken)
    {
        if (!EsAdmin())
        {
            return RedirectToPage("/Pages/Index");
        }

        var usuarioSesionId = ObtenerUsuarioSesionId();
        if (!usuarioSesionId.HasValue)
        {
            return RedirectToPage("/Pages/Login");
        }

        NuevoUsuario.Rol = RolNuevoUsuario;

        var resultado = await _usuarioServicio.CrearUsuarioAsync(NuevoUsuario, usuarioSesionId.Value, cancellationToken);

        if (resultado.IsFailure)
        {
            MensajeError = resultado.Error.Message;
            CargarUsuarios();
            return Page();
        }

        TempData["MensajeOk"] = "Usuario creado correctamente. Se enviaron credenciales por correo.";
        return RedirectToPage();
    }

    public IActionResult OnPostBaja(string token)
    {
        if (!EsAdmin())
        {
            return RedirectToPage("/Pages/Index");
        }

        var usuarioSesionId = ObtenerUsuarioSesionId();
        if (!usuarioSesionId.HasValue)
        {
            return RedirectToPage("/Pages/Login");
        }

        if (!_routeTokenService.TryObtenerId(token, out var usuarioId))
        {
            return NotFound();
        }

        var resultado = _usuarioServicio.DarDeBaja(usuarioId, usuarioSesionId.Value);

        if (resultado.IsFailure)
        {
            MensajeError = resultado.Error.Message;
            CargarUsuarios();
            return Page();
        }

        TempData["MensajeOk"] = "Usuario dado de baja correctamente.";
        return RedirectToPage();
    }

    private void CargarUsuarios()
    {
        var tabla = _usuarioServicio.Select();
        Usuarios = new List<UsuarioListadoItem>();

        foreach (DataRow row in tabla.Rows)
        {
            var usuarioId = Convert.ToInt32(row["UsuarioId"]);

            Usuarios.Add(new UsuarioListadoItem
            {
                UsuarioId = usuarioId,
                UsuarioIdToken = _routeTokenService.CrearToken(usuarioId),
                Nombres = row["Nombres"].ToString() ?? string.Empty,
                PrimerApellido = row["PrimerApellido"].ToString() ?? string.Empty,
                SegundoApellido = row["SegundoApellido"].ToString() ?? string.Empty,
                Email = row["Email"].ToString() ?? string.Empty,
                NombreUsuario = row["NombreUsuario"].ToString() ?? string.Empty,
                Rol = row["Rol"].ToString() ?? string.Empty,
                Estado = Convert.ToBoolean(row["Estado"])
            });
        }

        MensajeOk = TempData["MensajeOk"]?.ToString();
    }

    private bool EsAdmin()
    {
        var rol = HttpContext.Session.GetString(SessionKeys.Rol);
        return string.Equals(rol, Usuario.RolAdmin, StringComparison.Ordinal);
    }

    private int? ObtenerUsuarioSesionId()
    {
        var usuarioSesion = HttpContext.Session.GetString(SessionKeys.UsuarioId);

        if (int.TryParse(usuarioSesion, out var usuarioId))
        {
            return usuarioId;
        }

        return null;
    }

    public class UsuarioListadoItem
    {
        public int UsuarioId { get; set; }
        public string UsuarioIdToken { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string PrimerApellido { get; set; } = string.Empty;
        public string SegundoApellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public bool Estado { get; set; }
    }
}

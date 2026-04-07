using gestion_bibliotecaria.Aplicacion.Interfaces;
using gestion_bibliotecaria.Domain.Common;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Data;

namespace gestion_bibliotecaria.Pages;

public class AutorModel : PageModel
{
    private readonly IAutorServicio _autorServicio;
    private readonly RouteTokenService _routeTokenService;

    public List<Autor> Autores { get; set; } = new();

    [BindProperty]
    public Autor Autor { get; set; } = new Autor();

    public string ModalActivo { get; set; } = string.Empty;

    public AutorModel(
        IAutorServicio autorServicio,
        RouteTokenService routeTokenService)
    {
        _autorServicio = autorServicio;
        _routeTokenService = routeTokenService;
    }


    public void OnGet()
    {
        CargarAutores();
    }

    private void CargarAutores()
    {
        var tabla = _autorServicio.Select();

        Autores = new List<Autor>();

        foreach (DataRow row in tabla.Rows)
        {
            var autor = new Autor
            {
                AutorId = Convert.ToInt32(row["AutorId"]),
                Nombres = row["Nombres"].ToString()!,
                Apellidos = row["Apellidos"] == DBNull.Value ? null : row["Apellidos"].ToString(),
                Nacionalidad = row["Nacionalidad"] == DBNull.Value ? null : row["Nacionalidad"].ToString(),
                FechaNacimiento = row["FechaNacimiento"] == DBNull.Value ? null : Convert.ToDateTime(row["FechaNacimiento"]),
                Estado = Convert.ToBoolean(row["Estado"])
            };

            autor.RouteToken = _routeTokenService.CrearToken(autor.AutorId);

            Autores.Add(autor);
        }
    }

    public IActionResult OnPostEliminar(string token)
    {
        if (!_routeTokenService.TryObtenerId(token, out var id))
            return NotFound();

        var autor = _autorServicio.GetById(id);
        if (autor == null)
            return NotFound();

        autor.UsuarioSesionId = ObtenerUsuarioSesionId() ?? autor.UsuarioSesionId;
        _autorServicio.Delete(autor);

        return RedirectToPage();
    }

    public IActionResult OnPostCrear()
    {
        ModalActivo = "crear";

        Autor.UsuarioSesionId = ObtenerUsuarioSesionId();

        var validacion = _autorServicio.ValidarAutor(Autor);

        if (validacion.IsFailure)
        {
            AgregarError(validacion.Error, true);
        }

        if (!ModelState.IsValid)
        {
            CargarAutores();
            return Page();
        }

        _autorServicio.Create(Autor);

        return RedirectToPage();
    }

    public IActionResult OnPostEditar(
        string token,
        string Nombres,
        string? Apellidos,
        string? Nacionalidad,
        DateTime? FechaNacimiento,
        bool? Estado)
    {
        ModalActivo = "editar";

        if (!_routeTokenService.TryObtenerId(token, out var id))
            return NotFound();

        var autor = _autorServicio.GetById(id);
        if (autor == null)
            return NotFound();

        autor.Nombres = Nombres;
        autor.Apellidos = Apellidos;
        autor.Nacionalidad = Nacionalidad;
        autor.FechaNacimiento = FechaNacimiento;
        autor.Estado = Estado ?? false;

        var usuarioSesionId = ObtenerUsuarioSesionId();
        autor.UsuarioSesionId = usuarioSesionId ?? autor.UsuarioSesionId;

        var validacion = _autorServicio.ValidarAutor(autor);

        if (validacion.IsFailure)
        {
            AgregarError(validacion.Error);
        }

        if (!ModelState.IsValid)
        {
            ModelState.SetModelValue("token", new ValueProviderResult(token));
            ModelState.SetModelValue("Nombres", new ValueProviderResult(Nombres));
            ModelState.SetModelValue("Apellidos", new ValueProviderResult(Apellidos ?? ""));
            ModelState.SetModelValue("Nacionalidad", new ValueProviderResult(Nacionalidad ?? ""));
            ModelState.SetModelValue("FechaNacimiento", new ValueProviderResult(FechaNacimiento?.ToString("yyyy-MM-dd") ?? ""));
            ModelState.SetModelValue("Estado", new ValueProviderResult((Estado ?? false).ToString()));
            CargarAutores();
            return Page();
        }

        _autorServicio.Update(autor);

        return RedirectToPage();
    }

    private void AgregarError(Error error, bool esCrear = false)
    {
        var key = error.Code.Split('.').LastOrDefault() ?? string.Empty;

        if (esCrear)
        {
            ModelState.AddModelError($"Autor.{key}", error.Message);
        }
        else
        {
            ModelState.AddModelError(key, error.Message);
        }
    }

    private int? ObtenerUsuarioSesionId()
    {
        var claim = HttpContext.Session.GetString(SessionKeys.UsuarioId);
        if (int.TryParse(claim, out var usuarioId))
        {
            return usuarioId;
        }

        return null;
    }
}

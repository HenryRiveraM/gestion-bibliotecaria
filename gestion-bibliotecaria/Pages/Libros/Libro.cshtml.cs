using gestion_bibliotecaria.Aplicacion.Dtos;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Aplicacion.Interfaces;
using gestion_bibliotecaria.Domain.Validations;
using gestion_bibliotecaria.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Data;

namespace gestion_bibliotecaria.Pages;

public class LibroModel : PageModel
{
    private readonly RouteTokenService _routeTokenService;
    private readonly ILibroServicio _libroServicio;

    public IEnumerable<LibroDto> Libros { get; set; } = new List<LibroDto>();
    public Dictionary<int, string> AutoresNombres { get; set; } = new();
    public IEnumerable<AutorDto> Autores { get; set; } = new List<AutorDto>();
    public Dictionary<int, string> LibroTokens { get; set; } = new();

    public LibroModel(
        RouteTokenService routeTokenService,
        ILibroServicio libroServicio)
    {
        _routeTokenService = routeTokenService;
        _libroServicio = libroServicio;
    }

    public IActionResult OnGet()
    {
        if (!EsAdminOBibliotecario())
        {
            return RedirectToPage("/");
        }

        Libros = _libroServicio.Select();

        foreach (var l in Libros)
        {
            LibroTokens[l.LibroId] = _routeTokenService.CrearToken(l.LibroId);
        }

        AutoresNombres = _libroServicio.ObtenerNombresAutores();
        Autores = _libroServicio.ObtenerAutoresActivos();
        return Page();
    }

    public IActionResult OnPostEliminar(string token)
    {
        if (!EsAdminOBibliotecario())
        {
            return RedirectToPage("/");
        }

        if (!_routeTokenService.TryObtenerId(token, out var libroId))
        {
            return NotFound();
        }

        var result = _libroServicio.Delete(libroId, ObtenerUsuarioSesionId());

        if (result.IsFailure)
        {
            // Opcional: mostrar mensaje de error
        }

        return RedirectToPage();
    }

    public IActionResult OnPostEditar(
        string token,
        int? AutorId,
        string? Titulo,
        string? ISBN,
        string? Editorial,
        string? Genero,
        string? Edicion,
        int? AñoPublicacion,
        int? NumeroPaginas,
        string? Idioma,
        string? PaisPublicacion,
        string? Descripcion,
        bool Estado)
    {
        if (!EsAdminOBibliotecario())
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return new JsonResult(new { success = false, redirect = "/Index" });
            }

            return RedirectToPage("/");
        }

        if (!_routeTokenService.TryObtenerId(token, out var id))
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return new JsonResult(new { success = false, errors = new Dictionary<string, string> { { "", "Petición inválida o token expirado." } } });
            return NotFound();
        }

        var dto = new LibroDto
        {
            LibroId = id,
            UsuarioSesionId = ObtenerUsuarioSesionId(),
            AutorId = AutorId ?? 0,
            Titulo = Titulo ?? string.Empty,
            ISBN = ISBN,
            Editorial = Editorial,
            Genero = Genero,
            Edicion = Edicion,
            AñoPublicacion = AñoPublicacion,
            NumeroPaginas = NumeroPaginas,
            Idioma = Idioma,
            PaisPublicacion = PaisPublicacion,
            Descripcion = Descripcion,
            Estado = Estado
        };

        var resultado = _libroServicio.Update(dto);

        if (resultado.IsFailure)
        {
            ModelState.AddModelError(resultado.Error.Code.Split('.').LastOrDefault() ?? "Error", resultado.Error.Message);
        }

        if (!ModelState.IsValid)
        {
            var listaErrores = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).FirstOrDefault()
            );
            return new JsonResult(new { success = false, errors = listaErrores });
        }

        return new JsonResult(new { success = true });
    }

    public IActionResult OnPostCrear(
        string? NombreAutorNuevo,
        string? Titulo,
        string? ISBN,
        string? Editorial,
        string? Genero,
        string? Edicion,
        int? AñoPublicacion,
        int? NumeroPaginas,
        string? Idioma,
        string? PaisPublicacion,
        string? Descripcion,
        bool Estado = true)
    {
        if (!EsAdminOBibliotecario())
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return new JsonResult(new { success = false, redirect = "/Index" });
            }

            return RedirectToPage("/");
        }

        ModelState.Remove("AutorId");

        int? AutorId = null;
        if (int.TryParse(Request.Form["AutorId"], out var parsedId)) AutorId = parsedId;

        var dto = new LibroDto
        {
            UsuarioSesionId = ObtenerUsuarioSesionId(),
            AutorId = AutorId ?? 0,
            Titulo = Titulo ?? string.Empty,
            ISBN = ISBN,
            Editorial = Editorial,
            Genero = Genero,
            Edicion = Edicion,
            AñoPublicacion = AñoPublicacion,
            NumeroPaginas = NumeroPaginas,
            Idioma = Idioma,
            PaisPublicacion = PaisPublicacion,
            Descripcion = Descripcion,
            Estado = Estado
        };

        var resultado = _libroServicio.Create(dto, NombreAutorNuevo);

        if (resultado.IsFailure)
        {
            ModelState.AddModelError(resultado.Error.Code.Split('.').LastOrDefault() ?? "Error", resultado.Error.Message);
        }

        if (!ModelState.IsValid)
        {
            var listaErrores = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).FirstOrDefault()
            );
            return new JsonResult(new { success = false, errors = listaErrores });
        }

        return new JsonResult(new { success = true });
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

    private bool EsAdminOBibliotecario()
    {
        var rol = HttpContext.Session.GetString(SessionKeys.Rol);

        return string.Equals(rol, Usuario.RolAdmin, StringComparison.OrdinalIgnoreCase)
            || string.Equals(rol, Usuario.RolBibliotecario, StringComparison.OrdinalIgnoreCase);
    }
}

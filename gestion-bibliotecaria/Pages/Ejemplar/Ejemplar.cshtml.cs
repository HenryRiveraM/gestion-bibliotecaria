using gestion_bibliotecaria.Aplicacion.Dtos;
using gestion_bibliotecaria.Aplicacion.Fachadas;
using gestion_bibliotecaria.Aplicacion.Interfaces;
using gestion_bibliotecaria.Domain.Common;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Domain.Errors;
using gestion_bibliotecaria.Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;


namespace gestion_bibliotecaria.Pages;

public class EjemplarModel : PageModel
{
    private readonly IEjemplarServicio _ejemplarServicio;
    private readonly RouteTokenService _routeTokenService;
    

    public List<EjemplarDto> Ejemplares { get; set; } = new();
    public Dictionary<int, string> LibrosTitulos { get; set; } = new();
    public List<LibroDto> Libros { get; set; } = new();

    public string ErrorMessage { get; set; } = string.Empty;

    public EjemplarModel(
        IEjemplarServicio ejemplarServicio,
        RouteTokenService routeTokenService
        )
    {
        _ejemplarServicio = ejemplarServicio;
        _routeTokenService = routeTokenService;
   
    }

    public IActionResult OnGet()
    {
        if (!EsAdminOBibliotecario())
        {
            return LocalRedirect("/");
        }

        CargarPagina();
        return Page();
    }

    public IActionResult OnPostEliminar(string token)
    {
        if (!EsAdminOBibliotecario())
        {
            return LocalRedirect("/");
        }

        if (!_routeTokenService.TryObtenerId(token, out var id))
        {
            return NotFound();
        }

        try
        {
            var ejemplar = _ejemplarServicio.GetById(id);

            if (ejemplar == null)
            {
                return NotFound();
            }

            if (!ejemplar.Estado)
            {
                return RedirectToPage();
            }

            ejemplar.UsuarioSesionId = ObtenerUsuarioSesionId() ?? ejemplar.UsuarioSesionId;
            var result = _ejemplarServicio.Delete(ejemplar);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Error.Message);
                CargarPagina();
                return Page();
            }

            return RedirectToPage();
        }
        catch
        {
            CargarPagina();
            return Page();
        }
    }

    public IActionResult OnPostEditar(
        string token,
        int LibroId,
        string CodigoInventario,
        string? EstadoConservacion,
        bool? Disponible,
        bool? DadoDeBaja,
        string? MotivoBaja,
        string? Ubicacion,
        bool? Estado)
    {
        if (!EsAdminOBibliotecario())
        {
            return LocalRedirect("/");
        }

        if (!_routeTokenService.TryObtenerId(token, out var ejemplarId))
        {
            return NotFound();
        }

        var dto = new EjemplarDto
        {
            EjemplarId = ejemplarId,
            LibroId = LibroId,
            CodigoInventario = CodigoInventario ?? string.Empty,
            EstadoConservacion = EstadoConservacion,
            Disponible = Disponible ?? false,
            DadoDeBaja = DadoDeBaja ?? false,
            MotivoBaja = MotivoBaja,
            Ubicacion = Ubicacion,
            Estado = Estado ?? false,
            UsuarioSesionId = ObtenerUsuarioSesionId()
        };

        if (!ModelState.IsValid)
        {
            CargarPagina();
            return Page();
        }

        try
        {
            var result = _ejemplarServicio.Update(dto);
            if (!result.IsSuccess)
            {
                AgregarError(result.Error);
                CargarPagina();
                return Page();
            }
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            ModelState.AddModelError("CodigoInventario", EjemplarErrors.CodigoDuplicado.Message);
            CargarPagina();
            return Page();
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, EjemplarErrors.ErrorProcesado.Message);
            CargarPagina();
            return Page();
        }

        return RedirectToPage();
    }

    public IActionResult OnPostCrear(EjemplarDto Ejemplar)
    {
        if (!EsAdminOBibliotecario())
        {
            return LocalRedirect("/");
        }

        Ejemplar.UsuarioSesionId = ObtenerUsuarioSesionId();

        if (!ModelState.IsValid)
        {
            ErrorMessage = EjemplarErrors.CamposIncompletos.Message;
            CargarPagina();
            return Page();
        }

        try
        {
            var result = _ejemplarServicio.Create(Ejemplar);
            if (!result.IsSuccess)
            {
                AgregarError(result.Error);
                CargarPagina();
                return Page();
            }
            return RedirectToPage();
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            ModelState.AddModelError("Ejemplar.CodigoInventario", EjemplarErrors.CodigoDuplicado.Message);
            CargarPagina();
            return Page();
        }
        catch (Exception)
        {
            ErrorMessage = EjemplarErrors.ErrorProcesado.Message;
            CargarPagina();
            return Page();
        }
    }

    private void CargarPagina()
    {
        Ejemplares = _ejemplarServicio.Select().ToList();
        foreach (var ejemplar in Ejemplares)
        {
            ejemplar.RouteToken = _routeTokenService.CrearToken(ejemplar.EjemplarId);
        }
        LibrosTitulos = _ejemplarServicio.ObtenerTitulosLibros();
        Libros = _ejemplarServicio.ObtenerLibrosActivos().ToList();
    }

    private void AgregarError(Error error)
    {
        var key = error.Code.Split('.').LastOrDefault() ?? string.Empty;
        ModelState.AddModelError(key, error.Message);
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

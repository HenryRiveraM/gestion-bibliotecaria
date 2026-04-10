using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gestion_bibliotecaria.Aplicacion.Interfaces;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Domain.Common;
using gestion_bibliotecaria.Domain.Errors;
using gestion_bibliotecaria.Infrastructure.Security;
using MySql.Data.MySqlClient;
using System.Data;
using Microsoft.AspNetCore.Http;

namespace gestion_bibliotecaria.Pages;

public class EjemplarModel : PageModel
{
    private readonly IEjemplarServicio _ejemplarServicio;
    private readonly RouteTokenService _routeTokenService;

    public List<Ejemplar> Ejemplares { get; set; } = new();
    public Dictionary<int, string> LibrosTitulos { get; set; } = new();
    public List<Libro> Libros { get; set; } = new();

    public string ErrorMessage { get; set; } = string.Empty;

    public EjemplarModel(
        IEjemplarServicio ejemplarServicio,
        RouteTokenService routeTokenService)
    {
        _ejemplarServicio = ejemplarServicio;
        _routeTokenService = routeTokenService;
    }

    public void OnGet()
    {
        CargarPagina();
    }

    public IActionResult OnPostEliminar(string token)
    {
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
            _ejemplarServicio.Delete(ejemplar);

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
        if (!_routeTokenService.TryObtenerId(token, out var ejemplarId))
        {
            return NotFound();
        }

        var ejemplar = _ejemplarServicio.GetById(ejemplarId);

        if (ejemplar == null)
        {
            return NotFound();
        }

        ejemplar.LibroId = LibroId;
        ejemplar.CodigoInventario = CodigoInventario;
        ejemplar.EstadoConservacion = EstadoConservacion;
        ejemplar.Disponible = Disponible ?? false;
        ejemplar.DadoDeBaja = DadoDeBaja ?? false;
        ejemplar.MotivoBaja = MotivoBaja;
        ejemplar.Ubicacion = Ubicacion;
        ejemplar.Estado = Estado ?? false;

        var usuarioSesionId = ObtenerUsuarioSesionId();
        ejemplar.UsuarioSesionId = usuarioSesionId ?? ejemplar.UsuarioSesionId;

        var validacion = _ejemplarServicio.ValidarEjemplar(ejemplar);
        if (validacion.IsFailure)
        {
            AgregarError(validacion.Error);
        }

        if (!ModelState.IsValid)
        {
            CargarPagina();
            return Page();
        }

        if (!_ejemplarServicio.ExisteLibroActivo(LibroId))
        {
            ModelState.AddModelError("LibroId", EjemplarErrors.LibroInvalido.Message);
            CargarPagina();
            return Page();
        }

        try
        {
            _ejemplarServicio.Update(ejemplar);
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

    public IActionResult OnPostCrear(Ejemplar Ejemplar)
    {
        Ejemplar.UsuarioSesionId = ObtenerUsuarioSesionId();

        var validacion = _ejemplarServicio.ValidarEjemplar(Ejemplar);
        if (validacion.IsFailure)
        {
            AgregarError(validacion.Error);
        }

        if (!ModelState.IsValid)
        {
            ErrorMessage = EjemplarErrors.CamposIncompletos.Message;
            CargarPagina();
            return Page();
        }

        if (!_ejemplarServicio.ExisteLibroActivo(Ejemplar.LibroId))
        {
            ModelState.AddModelError("Ejemplar.LibroId", EjemplarErrors.LibroInvalido.Message);
            CargarPagina();
            return Page();
        }

        Ejemplar.FechaRegistro = DateTime.Now;

        try
        {
            _ejemplarServicio.Create(Ejemplar);
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
        var tabla = _ejemplarServicio.Select();

        Ejemplares = new List<Ejemplar>();

        foreach (DataRow row in tabla.Rows)
        {
            var ejemplar = new Ejemplar
            {
                EjemplarId = Convert.ToInt32(row["EjemplarId"]),
                LibroId = Convert.ToInt32(row["LibroId"]),
                CodigoInventario = row["CodigoInventario"].ToString()!,
                EstadoConservacion = row["EstadoConservacion"] == DBNull.Value ? null : row["EstadoConservacion"].ToString(),
                Disponible = Convert.ToBoolean(row["Disponible"]),
                DadoDeBaja = Convert.ToBoolean(row["DadoDeBaja"]),
                MotivoBaja = row["MotivoBaja"] == DBNull.Value ? null : row["MotivoBaja"].ToString(),
                Ubicacion = row["Ubicacion"] == DBNull.Value ? null : row["Ubicacion"].ToString(),
                Estado = Convert.ToBoolean(row["Estado"]),
                FechaRegistro = row.Table.Columns.Contains("FechaRegistro") && row["FechaRegistro"] != DBNull.Value
                    ? Convert.ToDateTime(row["FechaRegistro"])
                    : DateTime.MinValue,
                UltimaActualizacion = row.Table.Columns.Contains("UltimaActualizacion") && row["UltimaActualizacion"] != DBNull.Value
                    ? Convert.ToDateTime(row["UltimaActualizacion"])
                    : null
            };

            ejemplar.RouteToken = _routeTokenService.CrearToken(ejemplar.EjemplarId);
            Ejemplares.Add(ejemplar);
        }

        LibrosTitulos = _ejemplarServicio.ObtenerTitulosLibros();

        var librosActivos = _ejemplarServicio.ObtenerLibrosActivos();
        Libros = new List<Libro>();
        foreach (DataRow row in librosActivos.Rows)
        {
            Libros.Add(new Libro
            {
                LibroId = Convert.ToInt32(row["LibroId"]),
                Titulo = row["Titulo"].ToString()!,
                Editorial = row["Editorial"] == DBNull.Value ? null : row["Editorial"].ToString()
            });
        }
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
}

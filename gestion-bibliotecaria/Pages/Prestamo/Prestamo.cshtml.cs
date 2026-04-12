using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gestion_bibliotecaria.Aplicacion.Interfaces;
using gestion_bibliotecaria.Domain.Entities;
using PrestamoEntity = gestion_bibliotecaria.Domain.Entities.Prestamo;
using System.Linq;
using gestion_bibliotecaria.Infrastructure.Security;
using System.Data;

namespace gestion_bibliotecaria.Pages.Prestamo;

public class PrestamoModel : PageModel
{
    private readonly gestion_bibliotecaria.Aplicacion.Fachadas.IPrestamoFachada _prestamoFachada;
    private readonly IPrestamoServicio _prestamoServicio;
    private readonly RouteTokenService _routeTokenService;

    public List<PrestamoEntity> Prestamos { get; set; } = new();
    public Dictionary<int, string> LibrosTitulos { get; set; } = new();

    public DateTime FechaPrestamoDisplay { get; set; }
    public DateTime FechaDevolucionDefault { get; set; }

    [BindProperty]
    public PrestamoEntity NuevoPrestamo { get; set; } = new();

    public string? MensajeError { get; set; }
    public string? MensajeOk { get; set; }

    public PrestamoModel(gestion_bibliotecaria.Aplicacion.Fachadas.IPrestamoFachada prestamoFachada, IPrestamoServicio prestamoServicio, RouteTokenService routeTokenService)
    {
        _prestamoFachada = prestamoFachada;
        _prestamoServicio = prestamoServicio;
        _routeTokenService = routeTokenService;
    }

    public void OnGet()
    {
        CargarPrestamos();
        FechaPrestamoDisplay = DateTime.Now;
        FechaDevolucionDefault = FechaPrestamoDisplay.AddDays(14);
    }

    private void SetFechaDefaults()
    {
        FechaPrestamoDisplay = DateTime.Now;
        FechaDevolucionDefault = FechaPrestamoDisplay.AddDays(14);
    }

    public JsonResult OnGetAutocompleteEjemplares(string q)
    {
        var items = _prestamoFachada.BuscarEjemplaresActivos(q ?? string.Empty)
            .Select(kv => new { id = kv.Key, label = kv.Value });

        return new JsonResult(items);
    }

    public JsonResult OnGetEjemplarDetalle(int id)
    {
        var ejemplar = _prestamoFachada.ObtenerEjemplarPorId(id);
        if (ejemplar == null)
            return new JsonResult(null);

        return new JsonResult(new
        {
            id = ejemplar.EjemplarId,
            codigo = ejemplar.CodigoInventario,
            label = _prestamoFachada.ObtenerLabelEjemplar(id),
            estadoConservacion = ejemplar.EstadoConservacion,
            disponible = ejemplar.Disponible,
            observaciones = ejemplar.MotivoBaja
        });
    }

    public JsonResult OnGetAutocompleteLectores(string q)
    {
        var items = _prestamoFachada.BuscarLectoresPorCi(q ?? string.Empty)
            .Select(kv => new { id = kv.Key, label = kv.Value });

        return new JsonResult(items);
    }

    // Handler para creación desde la página Create. Recibe una lista de ids de ejemplar como cadena separada por comas.
    public IActionResult OnPostCrear(string EjemplarData, int LectorId, DateTime FechaDevolucionEsperada, string? LectorCi, string? LectorComp)
    {
        // Sólo bibliotecarios pueden realizar esta acción
        var rol = HttpContext.Session.GetString(SessionKeys.Rol);
        if (!string.Equals(rol, Domain.Entities.Usuario.RolBibliotecario, StringComparison.Ordinal))
        {
            return Forbid();
        }

        // Resolver lector: si LectorId no provisto, intentar buscar por CI+complemento
        if (LectorId <= 0)
        {
            var ciFull = string.IsNullOrWhiteSpace(LectorComp) ? (LectorCi ?? string.Empty) : $"{LectorCi}-{LectorComp}";
            if (string.IsNullOrWhiteSpace(ciFull))
            {
                ModelState.AddModelError(string.Empty, "Debe indicar el CI del lector.");
                CargarPrestamos();
                SetFechaDefaults();
                return Page();
            }

            var usuario = _prestamoFachada.ObtenerUsuarioPorCi(ciFull);
            if (usuario == null)
            {
                ModelState.AddModelError(string.Empty, "Lector no encontrado.");
                CargarPrestamos();
                SetFechaDefaults();
                return Page();
            }

            LectorId = usuario.UsuarioId;
        }

        // Parse EjemplarData JSON: expected array of { id: int, observaciones: string }
        List<(int Id, string? Observaciones)> items = new();
        if (string.IsNullOrWhiteSpace(EjemplarData))
        {
            ModelState.AddModelError(string.Empty, "Debe seleccionar al menos un ejemplar.");
            CargarPrestamos();
            SetFechaDefaults();
            return Page();
        }

        try
        {
            var arr = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement[]>(EjemplarData);
            if (arr == null || arr.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Debe seleccionar al menos un ejemplar.");
                CargarPrestamos();
                return Page();
            }

            foreach (var el in arr)
            {
                if (el.TryGetProperty("id", out var idEl) && idEl.TryGetInt32(out var idVal))
                {
                    var obs = el.TryGetProperty("observaciones", out var obsEl) ? obsEl.GetString() : null;
                    items.Add((idVal, obs));
                }
            }
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Datos de ejemplares inválidos.");
            CargarPrestamos();
            SetFechaDefaults();
            return Page();
        }

        if (items.Count > 3)
        {
            ModelState.AddModelError(string.Empty, "No se pueden prestar más de 3 ejemplares a la vez.");
            CargarPrestamos();
            SetFechaDefaults();
            return Page();
        }

        // Crear prestamos individuales por cada ejemplar
        foreach (var it in items)
        {
            var prestamo = new PrestamoEntity
            {
                EjemplarId = it.Id,
                LectorId = LectorId,
                FechaDevolucionEsperada = FechaDevolucionEsperada,
                ObservacionesSalida = it.Observaciones,
                UsuarioSesionId = ObtenerUsuarioSesionId()
            };

            var resultado = _prestamoFachada.CrearPrestamo(prestamo);
            if (resultado.IsFailure)
            {
                ModelState.AddModelError(string.Empty, resultado.Error.Message);
                CargarPrestamos();
                SetFechaDefaults();
                return Page();
            }
        }

        TempData["MensajeOk"] = "Préstamo(s) registrado(s) correctamente.";
        return RedirectToPage("/Prestamo");
    }

    private void CargarPrestamos()
    {
        var tabla = _prestamoServicio.Select();

        Prestamos = new List<PrestamoEntity>();
        foreach (DataRow row in tabla.Rows)
        {
            Prestamos.Add(new PrestamoEntity
            {
                PrestamoId = Convert.ToInt32(row["PrestamoId"]),
                EjemplarId = Convert.ToInt32(row["EjemplarId"]),
                LectorId = Convert.ToInt32(row["LectorId"]),
                FechaPrestamo = Convert.ToDateTime(row["FechaPrestamo"]),
                FechaDevolucionEsperada = Convert.ToDateTime(row["FechaDevolucionEsperada"]),
                FechaDevolucionReal = row.Table.Columns.Contains("FechaDevolucionReal") && row["FechaDevolucionReal"] != DBNull.Value
                    ? Convert.ToDateTime(row["FechaDevolucionReal"]) : null,
                ObservacionesSalida = row.Table.Columns.Contains("ObservacionesSalida") && row["ObservacionesSalida"] != DBNull.Value
                    ? row["ObservacionesSalida"].ToString() : null,
                ObservacionesEntrada = row.Table.Columns.Contains("ObservacionesEntrada") && row["ObservacionesEntrada"] != DBNull.Value
                    ? row["ObservacionesEntrada"].ToString() : null,
                Estado = Convert.ToInt32(row["Estado"]),
                FechaRegistro = row.Table.Columns.Contains("FechaRegistro") && row["FechaRegistro"] != DBNull.Value
                    ? Convert.ToDateTime(row["FechaRegistro"]) : DateTime.MinValue,
            });
        }

        // cargar titulos para desplegables
        LibrosTitulos = _prestamoFachada.BuscarEjemplaresActivos(string.Empty).ToDictionary(k => k.Key, v => v.Value);
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

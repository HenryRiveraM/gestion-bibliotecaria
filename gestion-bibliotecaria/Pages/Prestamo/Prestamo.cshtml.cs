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
        // Retorna solo CIs para el autocomplete
        var items = _prestamoFachada.BuscarLectoresPorCi(q ?? string.Empty)
            .Select(kv => new { id = kv.Key, label = kv.Value.Split(" - ")[0] }); // Solo el CI

        return new JsonResult(items);
    }

    public JsonResult OnGetBuscarLectorPorCi(string ci, string? complemento)
    {
        if (string.IsNullOrWhiteSpace(ci))
            return new JsonResult(new { success = false, message = "CI requerido" });

        var ciFull = string.IsNullOrWhiteSpace(complemento) ? ci : $"{ci}-{complemento}";
        var usuario = _prestamoFachada.ObtenerUsuarioPorCi(ciFull);

        if (usuario == null)
            return new JsonResult(new { success = false, message = "Lector no encontrado" });

        return new JsonResult(new
        {
            success = true,
            id = usuario.UsuarioId,
            nombreCompleto = $"{usuario.Nombres} {usuario.PrimerApellido} {usuario.SegundoApellido ?? ""}".Trim()
        });
    }

    // DEBUG: Ver todos los lectores en la BD
    public JsonResult OnGetDebugLectores()
    {
        var tabla = _prestamoFachada.ObtenerTodosLosLectores(); // Will implement this method
        return new JsonResult(tabla);
    }

    // Handler para creación desde la página Create. Recibe una lista de ids de ejemplar como cadena JSON.
    public IActionResult OnPostCrear(string EjemplarData, int LectorId, DateTime FechaDevolucionEsperada, string? LectorCi, string? LectorComp)
    {
        // Resolver lector: si LectorId no provisto, intentar buscar por CI+complemento
        if (LectorId <= 0)
        {
            var ciFull = string.IsNullOrWhiteSpace(LectorComp) ? (LectorCi ?? string.Empty) : $"{LectorCi}-{LectorComp}";
            if (string.IsNullOrWhiteSpace(ciFull))
            {
                MensajeError = "Debe indicar el CI del lector.";
                CargarPrestamos();
                SetFechaDefaults();
                return Page();
            }

            var usuario = _prestamoFachada.ObtenerUsuarioPorCi(ciFull);
            if (usuario == null)
            {
                MensajeError = "Lector no encontrado.";
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
            MensajeError = "Debe seleccionar al menos un ejemplar.";
            CargarPrestamos();
            SetFechaDefaults();
            return Page();
        }

        try
        {
            // Try to parse the JSON - handle both JsonElement[] and simpler formats
            var trimmedData = EjemplarData.Trim();
            if (trimmedData == "[]")
            {
                MensajeError = "Debe seleccionar al menos un ejemplar.";
                CargarPrestamos();
                SetFechaDefaults();
                return Page();
            }

            // Use JsonDocument for more reliable parsing
            using (var doc = System.Text.Json.JsonDocument.Parse(trimmedData))
            {
                var root = doc.RootElement;
                if (root.ValueKind != System.Text.Json.JsonValueKind.Array)
                {
                    MensajeError = "Formato de ejemplares inválido.";
                    CargarPrestamos();
                    SetFechaDefaults();
                    return Page();
                }

                foreach (var el in root.EnumerateArray())
                {
                    if (el.TryGetProperty("id", out var idEl))
                    {
                        int idVal = 0;
                        if (idEl.ValueKind == System.Text.Json.JsonValueKind.Number)
                        {
                            idEl.TryGetInt32(out idVal);
                        }
                        else if (idEl.ValueKind == System.Text.Json.JsonValueKind.String && int.TryParse(idEl.GetString(), out var parsedId))
                        {
                            idVal = parsedId;
                        }

                        if (idVal > 0)
                        {
                            var obs = el.TryGetProperty("observaciones", out var obsEl) ? obsEl.GetString() : null;
                            items.Add((idVal, obs));
                        }
                    }
                }
            }

            if (items.Count == 0)
            {
                MensajeError = "No se encontraron ejemplares válidos.";
                CargarPrestamos();
                SetFechaDefaults();
                return Page();
            }
        }
        catch (System.Text.Json.JsonException ex)
        {
            MensajeError = $"Error al procesar los datos: {ex.Message}";
            CargarPrestamos();
            SetFechaDefaults();
            return Page();
        }
        catch (Exception ex)
        {
            MensajeError = $"Datos de ejemplares inválidos: {ex.Message}";
            CargarPrestamos();
            SetFechaDefaults();
            return Page();
        }

        if (items.Count > 3)
        {
            MensajeError = "No se pueden prestar más de 3 ejemplares a la vez.";
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
                MensajeError = resultado.Error.Message;
                CargarPrestamos();
                SetFechaDefaults();
                return Page();
            }
        }

        MensajeOk = "Préstamo(s) registrado(s) correctamente.";
        CargarPrestamos();
        SetFechaDefaults();
        return Page();
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

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gestion_bibliotecaria.Aplicacion.Interfaces;
using gestion_bibliotecaria.Aplicacion.DTOs;
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
    private readonly IEjemplarServicio _ejemplarServicio;
    private readonly IUsuarioServicio _usuarioServicio;
    private readonly RouteTokenService _routeTokenService;

    public List<PrestamoEntity> Prestamos { get; set; } = new();
    public List<PrestamoDetalleDTO> PrestamosDetallados { get; set; } = new();
    public Dictionary<int, string> LibrosTitulos { get; set; } = new();

    public DateTime FechaPrestamoDisplay { get; set; }
    public DateTime FechaDevolucionDefault { get; set; }

    [BindProperty]
    public PrestamoEntity NuevoPrestamo { get; set; } = new();

    public string? MensajeError { get; set; }
    public string? MensajeOk { get; set; }

    public PrestamoModel(gestion_bibliotecaria.Aplicacion.Fachadas.IPrestamoFachada prestamoFachada, IPrestamoServicio prestamoServicio, IEjemplarServicio ejemplarServicio, IUsuarioServicio usuarioServicio, RouteTokenService routeTokenService)
    {
        _prestamoFachada = prestamoFachada;
        _prestamoServicio = prestamoServicio;
        _ejemplarServicio = ejemplarServicio;
        _usuarioServicio = usuarioServicio;
        _routeTokenService = routeTokenService;
    }

    public void OnGet()
    {
        CargarPrestamosDetallados();
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
                CargarPrestamosDetallados();
                SetFechaDefaults();
                return Page();
            }

            var usuario = _prestamoFachada.ObtenerUsuarioPorCi(ciFull);
            if (usuario == null)
            {
                MensajeError = "Lector no encontrado.";
                CargarPrestamosDetallados();
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
            CargarPrestamosDetallados();
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
                CargarPrestamosDetallados();
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
                    CargarPrestamosDetallados();
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
                CargarPrestamosDetallados();
                SetFechaDefaults();
                return Page();
            }
        }
        catch (System.Text.Json.JsonException ex)
        {
            MensajeError = $"Error al procesar los datos: {ex.Message}";
            CargarPrestamosDetallados();
            SetFechaDefaults();
            return Page();
        }
        catch (Exception ex)
        {
            MensajeError = $"Datos de ejemplares inválidos: {ex.Message}";
            CargarPrestamosDetallados();
            SetFechaDefaults();
            return Page();
        }

        if (items.Count > 3)
        {
            MensajeError = "No se pueden prestar más de 3 ejemplares a la vez.";
            CargarPrestamosDetallados();
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
                MensajeError = resultado.Error.Message;
                CargarPrestamosDetallados();
                SetFechaDefaults();
                return Page();
            }
        }

        MensajeOk = "Préstamo(s) registrado(s) correctamente.";
        CargarPrestamosDetallados();
        SetFechaDefaults();
        return Page();
    }

    private void CargarPrestamos()
    {
        var prestamosDto = _prestamoServicio.Select();

        Prestamos = new List<PrestamoEntity>();
        foreach (var row in prestamosDto)
        {
            Prestamos.Add(new PrestamoEntity
            {
                PrestamoId = row.PrestamoId,
                EjemplarId = row.EjemplarId,
                LectorId = row.LectorId,
                FechaPrestamo = row.FechaPrestamo,
                FechaDevolucionEsperada = row.FechaDevolucionEsperada,
                FechaDevolucionReal = row.FechaDevolucionReal,
                ObservacionesSalida = row.ObservacionesSalida,
                ObservacionesEntrada = row.ObservacionesEntrada,
                Estado = row.Estado
            });
        }

        // cargar titulos para desplegables
        LibrosTitulos = _prestamoFachada.BuscarEjemplaresActivos(string.Empty).ToDictionary(k => k.Key, v => v.Value);
    }

    private void CargarPrestamosDetallados()
    {
        var tabla = _prestamoServicio.Select();
        PrestamosDetallados = new List<PrestamoDetalleDTO>();

        // Cargar todos los usuarios en memoria para búsqueda rápida
        var usuariosTabla = _usuarioServicio.Select();
        var usuariosDict = new Dictionary<int, (string Nombres, string PrimerApellido, string? SegundoApellido)>();
        foreach (var u in usuariosTabla)
        {
            try
            {
                int usuarioId = u.UsuarioId;
                string nombres = u.Nombres ?? string.Empty;
                string primerApellido = u.PrimerApellido ?? string.Empty;
                string? segundoApellido = u.SegundoApellido;
                usuariosDict[usuarioId] = (nombres, primerApellido, segundoApellido);
            }
            catch { }
        }

        // Procesar cada préstamo
        foreach (var row in tabla)
        {
            int ejemplarId = row.EjemplarId;
            int lectorId = row.LectorId;

            // Obtener info del ejemplar
            var ejemplar = _ejemplarServicio.GetById(ejemplarId);
            string tituloLibro = "Desconocido";
            string codigoInventario = "N/A";

            if (ejemplar != null)
            {
                codigoInventario = ejemplar.CodigoInventario ?? "N/A";
                // Obtener el título del libro asociado (usar la fachada o acceso directo)
                var etiquetaEjemplar = _prestamoFachada.ObtenerLabelEjemplar(ejemplarId);
                if (!string.IsNullOrEmpty(etiquetaEjemplar))
                {
                    // Extraer solo el título de la etiqueta "Título (INV-XXX)"
                    var parts = etiquetaEjemplar.Split('(');
                    tituloLibro = parts[0].Trim();
                }
            }

            // Obtener nombre del lector
            string nombreLector = "Desconocido";
            if (usuariosDict.TryGetValue(lectorId, out var usuario))
            {
                nombreLector = $"{usuario.Nombres} {usuario.PrimerApellido}".Trim();
                if (!string.IsNullOrWhiteSpace(usuario.SegundoApellido))
                {
                    nombreLector += $" {usuario.SegundoApellido}";
                }
            }

            PrestamosDetallados.Add(new PrestamoDetalleDTO
            {
                PrestamoId = row.PrestamoId,
                EjemplarId = ejemplarId,
                LectorId = lectorId,
                TituloLibro = tituloLibro,
                CodigoInventario = codigoInventario,
                NombreLector = nombreLector,
                FechaPrestamo = row.FechaPrestamo,
                FechaDevolucionEsperada = row.FechaDevolucionEsperada,
                FechaDevolucionReal = row.FechaDevolucionReal,
                ObservacionesSalida = row.ObservacionesSalida,
                ObservacionesEntrada = row.ObservacionesEntrada,
                Estado = row.Estado
            });
        }
    }

    public JsonResult OnGetDetallesPrestamo(int id)
    {
        var prestamo = PrestamosDetallados.FirstOrDefault(p => p.PrestamoId == id);
        if (prestamo == null)
        {
            // Recargar si no está en memoria
            CargarPrestamosDetallados();
            prestamo = PrestamosDetallados.FirstOrDefault(p => p.PrestamoId == id);
        }

        if (prestamo == null)
            return new JsonResult(new { success = false });

        return new JsonResult(new { success = true, data = prestamo });
    }

    public IActionResult OnPostAnularPrestamo(int prestamoId)
    {
        try
        {
            // Obtener el préstamo actual
            var tabla = _prestamoServicio.Select();
            gestion_bibliotecaria.Aplicacion.Dtos.PrestamoDto? prestamoRow = null;
            foreach (var row in tabla)
            {
                if (row.PrestamoId == prestamoId)
                {
                    prestamoRow = row;
                    break;
                }
            }

            if (prestamoRow == null)
            {
                MensajeError = "Préstamo no encontrado.";
                CargarPrestamosDetallados();
                return Page();
            }

            // Marcar ejemplar como disponible nuevamente
            int ejemplarId = prestamoRow.EjemplarId;
            var ejemplar = _ejemplarServicio.GetById(ejemplarId);
            if (ejemplar != null)
            {
                ejemplar.Disponible = true;
                _ejemplarServicio.Update(ejemplar);
            }

            var deleteResult = _prestamoServicio.Delete(prestamoRow);
            if (deleteResult.IsFailure)
            {
                ModelState.AddModelError(string.Empty, deleteResult.Error.Message);
                MensajeError = deleteResult.Error.Message;
                CargarPrestamosDetallados();
                return Page();
            }

            MensajeOk = "Préstamo anulado correctamente.";
            CargarPrestamosDetallados();
            return Page();
        }
        catch (Exception ex)
        {
            MensajeError = $"Error al anular préstamo: {ex.Message}";
            CargarPrestamosDetallados();
            return Page();
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

using gestion_bibliotecaria.Aplicacion.Fachadas;
using gestion_bibliotecaria.Aplicacion.Factories;
using gestion_bibliotecaria.Aplicacion.Interfaces;
using gestion_bibliotecaria.Domain.Common;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Domain.Ports;
using System.Collections.Generic;
using System.Linq;

namespace gestion_bibliotecaria.Aplicacion.Fachadas;

/// <summary>
/// FACHADA DE PRÉSTAMO - Lógica Profesional
/// 
/// Estructura de BD:
/// - Prestamo: almacena la TRANSACCIÓN (1 solo registro)
/// - Detalle: almacena las LÍNEAS del préstamo (N registros, uno por ejemplar)
/// 
/// Esto permite:
/// ✓ Múltiples ejemplares por préstamo
/// ✓ Devoluciones parciales
/// ✓ Auditoría clara
/// </summary>
public class PrestamoFachada : IPrestamoFachada
{
    private readonly IPrestamoServicio _prestamoServicio;
    private readonly IEjemplarServicio _ejemplarServicio;
    private readonly IUsuarioServicio _usuarioServicio;
    private readonly IDetalleServicio _detalleServicio;

    public PrestamoFachada(IPrestamoServicio prestamoServicio, IEjemplarServicio ejemplarServicio, IUsuarioServicio usuarioServicio, IDetalleServicio detalleServicio)
    {
        _prestamoServicio = prestamoServicio;
        _ejemplar_servicio_check(ejemplarServicio);
        _ejemplarServicio = ejemplarServicio;
        _usuarioServicio = usuarioServicio;
        _detalleServicio = detalleServicio;
    }

    /// <summary>
    /// NUEVO MÉTODO PROFESIONAL
    /// Crear un préstamo con múltiples ejemplares
    /// 
    /// Lógica:
    /// 1. Crear UN SOLO registro en Prestamo
    /// 2. Crear UN DETALLE por cada ejemplar
    /// 3. Marcar ejemplares como NO disponibles
    /// </summary>
    public Result CrearPrestamoMultiple(int lectorId, IEnumerable<int> ejemplarIds, DateTime fechaDevolucionEsperada, int? usuarioSesionId = null, string? observacionesSalida = null)
    {
        var detallesEjemplares = (ejemplarIds ?? Enumerable.Empty<int>())
            .Select(id => (EjemplarId: id, ObservacionesSalida: observacionesSalida));

        return CrearPrestamoMultiple(lectorId, detallesEjemplares, fechaDevolucionEsperada, usuarioSesionId);
    }

    public Result CrearPrestamoMultiple(int lectorId, IEnumerable<(int EjemplarId, string? ObservacionesSalida)> detallesEjemplares, DateTime fechaDevolucionEsperada, int? usuarioSesionId = null)
    {
        var detallesEntrada = detallesEjemplares?.ToList() ?? new List<(int EjemplarId, string? ObservacionesSalida)>();
        var ejemplares = detallesEntrada.Select(x => x.EjemplarId).ToList();

        // Validaciones básicas
        if (!ejemplares.Any())
            return Result.Failure(new Error("Prestamo.Error", "Debes seleccionar al menos un ejemplar."));

        if (ejemplares.Count > 5)
            return Result.Failure(new Error("Prestamo.Error", "No se pueden prestar más de 5 ejemplares a la vez."));

        // Validar límite de préstamos activos
        var actuales = _prestamoServicio.CountPrestamosActivos(lectorId);
        if (actuales >= 5)
            return Result.Failure(new Error("Prestamo.Limite", "El lector ya tiene el máximo de préstamos activos (5)."));

        try
        {
            // 1️⃣ CREAR UN SOLO PRÉSTAMO (sin referencia a ejemplar)
            var prestamo = new Prestamo
            {
                LectorId = lectorId,
                FechaPrestamo = DateTime.Now,
                FechaDevolucionEsperada = fechaDevolucionEsperada,
                ObservacionesSalida = detallesEntrada.FirstOrDefault().ObservacionesSalida,
                Estado = 1,  // ACTIVO
                UsuarioSesionId = usuarioSesionId
            };

            // Validar préstamo
            var validacion = _prestamoServicio.ValidarPrestamo(prestamo);
            if (validacion.IsFailure)
                return validacion;

            // Insertar y obtener ID
            _prestamoServicio.InsertAndReturnId(prestamo);
            if (prestamo.PrestamoId <= 0)
                return Result.Failure(new Error("Prestamo.Error", "No se pudo obtener el ID del préstamo."));

            // 2️⃣ CREAR UN DETALLE POR CADA EJEMPLAR
            var detalles = new List<Detalle>();
            foreach (var item in detallesEntrada)
            {
                var detalle = DetalleFactory.CrearDetalle(prestamo.PrestamoId, item.EjemplarId, usuarioSesionId, item.ObservacionesSalida);
                detalles.Add(detalle);
            }

            // Insertar detalles
            var resultadoDetalles = _detalleServicio.CrearMultiples(detalles);
            if (resultadoDetalles.IsFailure)
                return resultadoDetalles;

            // 3️⃣ MARCAR EJEMPLARES COMO NO DISPONIBLES
            foreach (var ejemplarId in ejemplares)
            {
                var ejemplar = _ejemplarServicio.GetById(ejemplarId);
                if (ejemplar != null)
                {
                    ejemplar.Disponible = false;
                    ejemplar.UsuarioSesionId = usuarioSesionId;
                    _ejemplarServicio.Update(ejemplar);
                }
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Prestamo.Error", $"Error al crear préstamo: {ex.Message}"));
        }
    }

    public Result CrearPrestamos(IEnumerable<Prestamo> prestamos)
    {
        // Este método es DEPRECATED - usa CrearPrestamoMultiple en su lugar
        // Se mantiene solo para compatibilidad temporal
        throw new NotImplementedException("Use CrearPrestamoMultiple instead");
    }

    private void _ejemplar_servicio_check(IEjemplarServicio dummy) { }

    public IEnumerable<KeyValuePair<int, string>> BuscarEjemplaresActivos(string q)
    {
        var disponibles = _ejemplarServicio.ObtenerEjemplaresDisponibles();
        if (string.IsNullOrWhiteSpace(q))
            return disponibles;

        var lower = q.ToLowerInvariant();
        return disponibles.Where(kv => kv.Value.ToLowerInvariant().Contains(lower));
    }

    public IEnumerable<KeyValuePair<int, string>> BuscarLectoresPorCi(string q)
    {
        var lista = new List<KeyValuePair<int, string>>();
        var usuarios = _usuarioServicio.Select();

        foreach (var u in usuarios)
        {
            try
            {
                var estado = u.Estado;
                if (!estado) continue;

                var rol = u.Rol ?? string.Empty;
                if (!rol.Equals("Lector", StringComparison.OrdinalIgnoreCase)) continue;

                var ci = u.CI ?? string.Empty;
                if (string.IsNullOrWhiteSpace(ci)) continue;

                var nombres = u.Nombres ?? string.Empty;

                if (string.IsNullOrWhiteSpace(q) || ci.StartsWith(q, StringComparison.OrdinalIgnoreCase))
                {
                    lista.Add(new KeyValuePair<int, string>(u.UsuarioId, ci + " - " + nombres));
                }
            }
            catch
            {
                continue;
            }
        }

        return lista;
    }

    public List<object> ObtenerTodosLosLectores()
    {
        var lista = new List<object>();
        var usuarios = _usuarioServicio.Select();

        foreach (var u in usuarios)
        {
            try
            {
                lista.Add(new
                {
                    usuarioId = u.UsuarioId,
                    ci = u.CI ?? "NULL_CI",
                    nombres = u.Nombres ?? "NO_NOMBRES",
                    rol = u.Rol ?? "NO_ROL",
                    estado = u.Estado,
                    esLector = (u.Rol ?? "").Equals("Lector", StringComparison.OrdinalIgnoreCase),
                    ciNoVacio = !string.IsNullOrWhiteSpace(u.CI)
                });
            }
            catch (Exception ex)
            {
                lista.Add(new { error = ex.Message });
            }
        }

        return lista;
    }

    public int CountPrestamosActivos(int lectorId)
    {
        return _prestamoServicio.CountPrestamosActivos(lectorId);
    }

    public Prestamo? ObtenerPrestamoPorId(int id)
    {
        return _prestamoServicio.GetById(id);
    }

    public gestion_bibliotecaria.Aplicacion.Dtos.EjemplarDto? ObtenerEjemplarPorId(int id)
    {
        return _ejemplarServicio.GetById(id);
    }

    public string? ObtenerLabelEjemplar(int ejemplarId)
    {
        var ejemplar = _ejemplarServicio.GetById(ejemplarId);
        if (ejemplar == null)
            return null;

        var titulos = _ejemplarServicio.ObtenerTitulosLibros();
        if (titulos.TryGetValue(ejemplar.LibroId, out var titulo))
        {
            return $"{titulo} ({ejemplar.CodigoInventario})";
        }

        return null;
    }

    public gestion_bibliotecaria.Domain.Entities.Usuario? ObtenerUsuarioPorCi(string ci)
    {
        var usuarios = _usuarioServicio.Select();
        foreach (var u in usuarios)
        {
            var full = u.CI ?? string.Empty;
            if (string.Equals(full, ci, StringComparison.OrdinalIgnoreCase))
            {
                return new gestion_bibliotecaria.Domain.Entities.Usuario
                {
                    UsuarioId = u.UsuarioId,
                    Nombres = u.Nombres ?? string.Empty,
                    PrimerApellido = u.PrimerApellido ?? string.Empty,
                    SegundoApellido = u.SegundoApellido,
                    CI = full
                };
            }
        }

        return null;
    }

    public Result CrearPrestamo(Prestamo prestamo)
    {
        // Este método es DEPRECATED - usa CrearPrestamoMultiple en su lugar
        throw new NotImplementedException("Use CrearPrestamoMultiple instead");
    }
}

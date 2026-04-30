using gestion_bibliotecaria.Aplicacion.Fachadas;
using gestion_bibliotecaria.Aplicacion.Interfaces;
using gestion_bibliotecaria.Domain.Common;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Domain.Ports;
using System.Collections.Generic;
using System.Linq;

namespace gestion_bibliotecaria.Aplicacion.Fachadas;


public class PrestamoFachada : IPrestamoFachada
{
    private readonly IPrestamoServicio _prestamoServicio;
    private readonly IEjemplarServicio _ejemplarServicio;
    private readonly IUsuarioServicio _usuarioServicio;
    private readonly IDetalleServicio _detalleServicio;
    private readonly IEjemplarDisponibilidadFachada _disponibilidadFachada;

    public PrestamoFachada(IPrestamoServicio prestamoServicio, IEjemplarServicio ejemplarServicio, IUsuarioServicio usuarioServicio, IDetalleServicio detalleServicio, IEjemplarDisponibilidadFachada disponibilidadFachada)
    {
        _prestamoServicio = prestamoServicio;
        _ejemplar_servicio_check(ejemplarServicio);
        _ejemplarServicio = ejemplarServicio;
        _usuarioServicio = usuarioServicio;
        _detalleServicio = detalleServicio;
        _disponibilidadFachada = disponibilidadFachada;
    }

    
    public Result<int> CrearPrestamoMultiple(int lectorId, IEnumerable<int> ejemplarIds, DateTime fechaDevolucionEsperada, int? usuarioSesionId = null, string? observacionesSalida = null)
    {
        var detallesEjemplares = (ejemplarIds ?? Enumerable.Empty<int>())
            .Select(id => (EjemplarId: id, ObservacionesSalida: observacionesSalida));

        return CrearPrestamoMultiple(lectorId, detallesEjemplares, fechaDevolucionEsperada, usuarioSesionId);
    }

    public Result<int> CrearPrestamoMultiple(int lectorId, IEnumerable<(int EjemplarId, string? ObservacionesSalida)> detallesEjemplares, DateTime fechaDevolucionEsperada, int? usuarioSesionId = null)
    {
        var detallesEntrada = detallesEjemplares?.ToList() ?? new List<(int EjemplarId, string? ObservacionesSalida)>();
        var ejemplares = detallesEntrada.Select(x => x.EjemplarId).ToList();

        
        if (!ejemplares.Any())
            return Result<int>.Failure(new Error("Prestamo.Error", "Debes seleccionar al menos un ejemplar."));

        if (ejemplares.Count > 5)
            return Result<int>.Failure(new Error("Prestamo.Error", "No se pueden prestar más de 5 ejemplares a la vez."));

      
        var actuales = _prestamoServicio.CountPrestamosActivos(lectorId);
        if (actuales >= 5)
            return Result<int>.Failure(new Error("Prestamo.Limite", "El lector ya tiene el máximo de préstamos activos (5)."));

        foreach (var ejemplarId in ejemplares)
        {
            var ejemplar = _ejemplarServicio.GetById(ejemplarId);
            if (ejemplar == null || !ejemplar.Disponible)
                return Result<int>.Failure(new Error("Prestamo.Error", $"El ejemplar {ejemplarId} no está disponible."));
        }

        try
        {
            
            var prestamo = new Prestamo
            {
                LectorId = lectorId,
                FechaPrestamo = DateTime.Now,
                FechaDevolucionEsperada = fechaDevolucionEsperada,
                ObservacionesSalida = detallesEntrada.FirstOrDefault().ObservacionesSalida,
                Estado = 1,  
                UsuarioSesionId = usuarioSesionId
            };

            
            var validacion = _prestamoServicio.ValidarPrestamo(prestamo);
            if (validacion.IsFailure)
                return Result<int>.Failure(validacion.Error);

            
            _prestamoServicio.InsertAndReturnId(prestamo);
            if (prestamo.PrestamoId <= 0)
                return Result<int>.Failure(new Error("Prestamo.Error", "No se pudo obtener el ID del préstamo."));

            
            var detalles = new List<Detalle>();
            foreach (var item in detallesEntrada)
            {
                var detalle = new Detalle
                {
                    PrestamoId = prestamo.PrestamoId,
                    EjemplarId = item.EjemplarId,
                    EstadoDetalle = 1, // PRESTADO
                    FechaDevolucionReal = null,
                    ObservacionesSalida = item.ObservacionesSalida,
                    ObservacionesEntrada = null,
                    UsuarioSesionId = usuarioSesionId,
                    FechaRegistro = DateTime.Now,
                    UltimaActualizacion = null
                };
                detalles.Add(detalle);
            }

           
            var resultadoDetalles = _detalleServicio.CrearMultiples(detalles);
            if (resultadoDetalles.IsFailure)
                return Result<int>.Failure(resultadoDetalles.Error);


            foreach (var ejemplarId in ejemplares)
            {
                var result = _disponibilidadFachada.CambiarDisponibilidad(ejemplarId, false, usuarioSesionId);
                if (result.IsFailure)
                    return Result<int>.Failure(result.Error);
            }

            return Result<int>.Success(prestamo.PrestamoId);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure(new Error("Prestamo.Error", $"Error al crear préstamo: {ex.Message}"));
        }
    }

    public Result CrearPrestamos(IEnumerable<Prestamo> prestamos)
    {
        
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
       
        throw new NotImplementedException("Use CrearPrestamoMultiple instead");
    }
}

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

    public PrestamoFachada(IPrestamoServicio prestamoServicio, IEjemplarServicio ejemplarServicio, IUsuarioServicio usuarioServicio)
    {
        _prestamoServicio = prestamoServicio;
        _ejemplar_servicio_check(ejemplarServicio);
        _ejemplarServicio = ejemplarServicio;
        _usuarioServicio = usuarioServicio;
    }

    public Result CrearPrestamos(IEnumerable<Prestamo> prestamos)
    {
        // Validate each prestamo
        foreach (var p in prestamos)
        {
            var validacion = _prestamoServicio.ValidarPrestamo(p);
            if (validacion.IsFailure)
                return Result.Failure(validacion.Error);
        }

        try
        {
            if (_prestamoServicio is gestion_bibliotecaria.Aplicacion.Servicios.PrestamoServicio svc)
            {
                svc.CreateManyAndMarkEjemplares(prestamos);
            }
            else
            {
                // fallback: create one by one
                foreach (var p in prestamos)
                    _prestamoServicio.Create(p);
            }

            return Result.Success();
        }
        catch
        {
            return Result.Failure(new Error("Prestamo.Error", "No se pudo crear los préstamos."));
        }
    }

    // simple helper to avoid unused warning in older compilers (keeps constructor logic similar)
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
                    lista.Add(new KeyValuePair<int, string>(
                        u.UsuarioId, 
                        ci + " - " + nombres
                    ));
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
                var estado = u.Estado;
                var rol = u.Rol ?? "NO_ROL";
                var ci = u.CI ?? "NULL_CI";
                var nombres = u.Nombres ?? "NO_NOMBRES";

                lista.Add(new
                {
                    usuarioId = u.UsuarioId,
                    ci = ci,
                    nombres = nombres,
                    rol = rol,
                    estado = estado,
                    esLector = rol.Equals("Lector", StringComparison.OrdinalIgnoreCase),
                    ciNoVacio = !string.IsNullOrWhiteSpace(ci) && ci != "NULL_CI"
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

        // Get the title from the servicios
        var titulos = _ejemplarServicio.ObtenerTitulosLibros();
        if (titulos.TryGetValue(ejemplar.LibroId, out var titulo))
        {
            return $"{titulo} ({ejemplar.CodigoInventario})";
        }

        return null;
    }

    public gestion_bibliotecaria.Domain.Entities.Usuario? ObtenerUsuarioPorCi(string ci)
    {
        // buscar en repo de usuarios (no existe método por ci, iterar tabla)
        var usuarios = _usuarioServicio.Select();
        foreach (var u in usuarios)
        {
            var full = u.CI ?? string.Empty; // En el DTO ya está unido el CI con el complemento
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
        var validacion = _prestamoServicio.ValidarPrestamo(prestamo);
        if (validacion.IsFailure)
            return Result.Failure(validacion.Error);

        // Limitar prestamos activos por lector a 3
        var actuales = _prestamoServicio.CountPrestamosActivos(prestamo.LectorId);
        if (actuales >= 3)
        {
            return Result.Failure(new Error("Prestamo.Limite", "El lector ya tiene el máximo de préstamos activos (3)."));
        }

        try
        {
            prestamo.FechaPrestamo = DateTime.Now;
            // Use service method that also marks ejemplar as not available
            if (_prestamoServicio is gestion_bibliotecaria.Aplicacion.Servicios.PrestamoServicio svc)
            {
                svc.CreateAndMarkEjemplar(prestamo);
            }
            else
            {
                _prestamoServicio.Create(prestamo);
            }
            return Result.Success();
        }
        catch
        {
            return Result.Failure(new Error("Prestamo.Error", "No se pudo crear el préstamo."));
        }
    }
}

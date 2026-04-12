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
        var tabla = _usuarioServicio.Select();
        foreach (System.Data.DataRow row in tabla.Rows)
        {
            var estado = Convert.ToBoolean(row["Estado"]);
            if (!estado) continue;

            var rol = row.Table.Columns.Contains("Rol") && row["Rol"] != DBNull.Value ? row["Rol"].ToString()! : string.Empty;
            if (!string.Equals(rol, Usuario.RolLector, StringComparison.Ordinal)) continue;

            var ci = row.Table.Columns.Contains("CI") && row["CI"] != DBNull.Value ? row["CI"].ToString()! : string.Empty;
            if (ci.Contains(q, StringComparison.OrdinalIgnoreCase))
            {
                lista.Add(new KeyValuePair<int, string>(Convert.ToInt32(row["UsuarioId"]), ci + " - " + row["Nombres"].ToString()));
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

    public gestion_bibliotecaria.Domain.Entities.Ejemplar? ObtenerEjemplarPorId(int id)
    {
        return _ejemplarServicio.GetById(id);
    }

    public string? ObtenerLabelEjemplar(int ejemplarId)
    {
        var dict = _ejemplarServicio.ObtenerEjemplaresDisponibles();
        return dict.TryGetValue(ejemplarId, out var v) ? v : null;
    }

    public gestion_bibliotecaria.Domain.Entities.Usuario? ObtenerUsuarioPorCi(string ci)
    {
        // buscar en repo de usuarios (no existe método por ci, iterar tabla)
        var tabla = _usuarioServicio.Select();
        foreach (System.Data.DataRow row in tabla.Rows)
        {
            var ciRow = row.Table.Columns.Contains("CI") && row["CI"] != DBNull.Value ? row["CI"].ToString()! : string.Empty;
            var complemento = row.Table.Columns.Contains("Complemento") && row["Complemento"] != DBNull.Value ? row["Complemento"].ToString()! : string.Empty;
            var full = string.IsNullOrWhiteSpace(complemento) ? ciRow : $"{ciRow}-{complemento}";
            if (string.Equals(full, ci, StringComparison.OrdinalIgnoreCase))
            {
                return new gestion_bibliotecaria.Domain.Entities.Usuario
                {
                    UsuarioId = Convert.ToInt32(row["UsuarioId"]),
                    Nombres = row["Nombres"].ToString() ?? string.Empty,
                    PrimerApellido = row["PrimerApellido"].ToString() ?? string.Empty,
                    SegundoApellido = row.Table.Columns.Contains("SegundoApellido") && row["SegundoApellido"] != DBNull.Value ? row["SegundoApellido"].ToString() : null,
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

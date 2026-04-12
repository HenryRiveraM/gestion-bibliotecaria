using System.Data;
using gestion_bibliotecaria.Aplicacion.Interfaces;
using gestion_bibliotecaria.Domain.Common;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Domain.Errors;
using gestion_bibliotecaria.Domain.Ports;
using gestion_bibliotecaria.Domain.Validations;

namespace gestion_bibliotecaria.Aplicacion.Servicios;

public class PrestamoServicio : IPrestamoServicio
{
    private readonly IPrestamoRepositorio _prestamoRepositorio;
    private readonly IEjemplarRepositorio _ejemplarRepositorio;
    private readonly IUsuarioRepositorio _usuarioRepositorio;

    public PrestamoServicio(IPrestamoRepositorio prestamoRepositorio, IEjemplarRepositorio ejemplarRepositorio, IUsuarioRepositorio usuarioRepositorio)
    {
        _prestamoRepositorio = prestamoRepositorio;
        _ejemplarRepositorio = ejemplarRepositorio;
        _usuarioRepositorio = usuarioRepositorio;
    }

    public DataTable Select()
    {
        var prestamos = _prestamoRepositorio.GetAll();
        var dt = new DataTable();
        dt.Columns.Add("PrestamoId", typeof(int));
        dt.Columns.Add("EjemplarId", typeof(int));
        dt.Columns.Add("LectorId", typeof(int));
        dt.Columns.Add("FechaPrestamo", typeof(DateTime));
        dt.Columns.Add("FechaDevolucionEsperada", typeof(DateTime));
        dt.Columns.Add("FechaDevolucionReal", typeof(DateTime));
        dt.Columns.Add("ObservacionesSalida", typeof(string));
        dt.Columns.Add("ObservacionesEntrada", typeof(string));
        dt.Columns.Add("Estado", typeof(int));
        dt.Columns.Add("UsuarioSesionId", typeof(int));
        dt.Columns.Add("FechaRegistro", typeof(DateTime));
        dt.Columns.Add("UltimaActualizacion", typeof(DateTime));

        foreach (var p in prestamos)
        {
            dt.Rows.Add(
                p.PrestamoId,
                p.EjemplarId,
                p.LectorId,
                p.FechaPrestamo,
                p.FechaDevolucionEsperada,
                p.FechaDevolucionReal.HasValue ? (object)p.FechaDevolucionReal.Value : DBNull.Value,
                p.ObservacionesSalida ?? (object)DBNull.Value,
                p.ObservacionesEntrada ?? (object)DBNull.Value,
                p.Estado,
                p.UsuarioSesionId.HasValue ? (object)p.UsuarioSesionId.Value : DBNull.Value,
                p.FechaRegistro,
                p.UltimaActualizacion.HasValue ? (object)p.UltimaActualizacion.Value : DBNull.Value
            );
        }

        return dt;
    }

    public void Create(Prestamo prestamo) => _prestamoRepositorio.Insert(prestamo);

    // Create and mark ejemplar as not available
    public void CreateAndMarkEjemplar(Prestamo prestamo)
    {
        _prestamoRepositorio.Insert(prestamo);

        // Intentar marcar ejemplar como no disponible
        var ejemplar = _ejemplarRepositorio.GetById(prestamo.EjemplarId);
        if (ejemplar != null)
        {
            ejemplar.Disponible = false;
            ejemplar.UsuarioSesionId = prestamo.UsuarioSesionId ?? ejemplar.UsuarioSesionId;
            _ejemplarRepositorio.Update(ejemplar);
        }
    }

    public void CreateManyAndMarkEjemplares(IEnumerable<Prestamo> prestamos)
    {
        // Delegate to repository which handles transaction and ejemplar update
        _prestamoRepositorio.InsertManyWithTransaction(prestamos);
    }

    public void Update(Prestamo prestamo) => _prestamoRepositorio.Update(prestamo);

    public void Delete(Prestamo prestamo) => _prestamoRepositorio.Delete(prestamo);

    public Prestamo? GetById(int id) => _prestamoRepositorio.GetById(id);

    public Result ValidarPrestamo(Prestamo prestamo)
    {
        if (prestamo is null)
            return Result.Failure(PrestamoErrors.DatosObligatorios);

        // Verificar que el ejemplar exista y esté disponible
        var ejemplar = _ejemplarRepositorio.GetById(prestamo.EjemplarId);
        if (ejemplar is null || !ejemplar.Disponible)
            return Result.Failure(PrestamoErrors.EjemplarNoDisponible);

        // Verificar que el lector exista y esté activo
        var lector = _usuarioRepositorio.GetById(prestamo.LectorId);
        if (lector is null || !lector.Estado || !string.Equals(lector.Rol, Usuario.RolLector, StringComparison.Ordinal))
            return Result.Failure(PrestamoErrors.LectorInvalido);

        // Fecha de devolución esperada debe ser mayor o igual a fecha de préstamo
        if (prestamo.FechaDevolucionEsperada < prestamo.FechaPrestamo)
            return Result.Failure(PrestamoErrors.FechaDevolucionInvalida);

        return Result.Success();
    }

    public int CountPrestamosActivos(int lectorId)
    {
        var prestamos = _prestamoRepositorio.GetAll();
        return prestamos.Count(p => p.Estado == 1 && p.LectorId == lectorId);
    }
}
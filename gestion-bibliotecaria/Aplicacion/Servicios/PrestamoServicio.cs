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

    public DataTable Select() => _prestamoRepositorio.GetAll();

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
        var tabla = _prestamoRepositorio.GetAll();
        var count = 0;
        foreach (DataRow row in tabla.Rows)
        {
            var estado = Convert.ToInt32(row["Estado"]);
            var lector = Convert.ToInt32(row["LectorId"]);
            // Estado 1 = activo
            if (estado == 1 && lector == lectorId)
                count++;
        }

        return count;
    }
}

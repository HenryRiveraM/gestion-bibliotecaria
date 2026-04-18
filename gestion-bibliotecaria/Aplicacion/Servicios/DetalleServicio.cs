using gestion_bibliotecaria.Aplicacion.Interfaces;
using gestion_bibliotecaria.Domain.Common;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Domain.Ports;

namespace gestion_bibliotecaria.Aplicacion.Servicios;

public class DetalleServicio : IDetalleServicio
{
    private readonly IDetalleRepositorio _detalleRepositorio;
    private readonly IEjemplarRepositorio _ejemplarRepositorio;

    public DetalleServicio(IDetalleRepositorio detalleRepositorio, IEjemplarRepositorio ejemplarRepositorio)
    {
        _detalleRepositorio = detalleRepositorio;
        _ejemplarRepositorio = ejemplarRepositorio;
    }

    public IEnumerable<Detalle> ObtenerTodos()
    {
        return _detalleRepositorio.GetAll();
    }

    public IEnumerable<Detalle> ObtenerPorPrestamo(int prestamoId)
    {
        return _detalleRepositorio.GetByPrestamoId(prestamoId);
    }

    public Detalle? ObtenerPorId(int id)
    {
        return _detalleRepositorio.GetById(id);
    }

    public Result Crear(Detalle detalle)
    {
        try
        {
            ValidarDetalle(detalle);
            _detalleRepositorio.Insert(detalle);
            
            // Marcar ejemplar como no disponible
            var ejemplar = _ejemplarRepositorio.GetById(detalle.EjemplarId);
            if (ejemplar != null)
            {
                ejemplar.Disponible = false;
                ejemplar.UsuarioSesionId = detalle.UsuarioSesionId ?? ejemplar.UsuarioSesionId;
                _ejemplarRepositorio.Update(ejemplar);
            }
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Detalle.Error", ex.Message));
        }
    }

    public Result CrearMultiples(IEnumerable<Detalle> detalles)
    {
        try
        {
            foreach (var detalle in detalles)
            {
                ValidarDetalle(detalle);
            }

            _detalleRepositorio.InsertMany(detalles);
            
            // Marcar ejemplares como no disponibles
            foreach (var detalle in detalles)
            {
                var ejemplar = _ejemplarRepositorio.GetById(detalle.EjemplarId);
                if (ejemplar != null)
                {
                    ejemplar.Disponible = false;
                    ejemplar.UsuarioSesionId = detalle.UsuarioSesionId ?? ejemplar.UsuarioSesionId;
                    _ejemplarRepositorio.Update(ejemplar);
                }
            }
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Detalle.Error", ex.Message));
        }
    }

    public Result Actualizar(Detalle detalle)
    {
        try
        {
            ValidarDetalle(detalle);
            _detalleRepositorio.Update(detalle);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Detalle.Error", ex.Message));
        }
    }

    public Result Eliminar(Detalle detalle)
    {
        try
        {
            _detalleRepositorio.Delete(detalle);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Detalle.Error", ex.Message));
        }
    }

    private void ValidarDetalle(Detalle detalle)
    {
        if (detalle.PrestamoId <= 0)
            throw new ArgumentException("PrestamoId debe ser mayor a 0");

        if (detalle.EjemplarId <= 0)
            throw new ArgumentException("EjemplarId debe ser mayor a 0");

        if (detalle.EstadoDetalle < 0 || detalle.EstadoDetalle > 2)
            throw new ArgumentException("EstadoDetalle debe estar entre 0 y 2");
    }
}

using gestion_bibliotecaria.Domain.Common;
using gestion_bibliotecaria.Domain.Entities;

namespace gestion_bibliotecaria.Aplicacion.Interfaces;

public interface IDetalleServicio
{
    IEnumerable<Detalle> ObtenerTodos();
    IEnumerable<Detalle> ObtenerPorPrestamo(int prestamoId);
    Detalle? ObtenerPorId(int id);
    Result Crear(Detalle detalle);
    Result CrearMultiples(IEnumerable<Detalle> detalles);
    Result Actualizar(Detalle detalle);
    Result Eliminar(Detalle detalle);
}

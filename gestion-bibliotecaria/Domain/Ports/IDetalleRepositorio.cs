using gestion_bibliotecaria.Domain.Entities;

namespace gestion_bibliotecaria.Domain.Ports;

public interface IDetalleRepositorio : IRepository<Detalle, int>
{
    IEnumerable<Detalle> GetByPrestamoId(int prestamoId);
    void InsertMany(IEnumerable<Detalle> detalles);
}

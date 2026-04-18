using gestion_bibliotecaria.Domain.Entities;

namespace gestion_bibliotecaria.Domain.Ports;

public interface IDetalleRepositorio
{
    IEnumerable<Detalle> GetAll();
    IEnumerable<Detalle> GetByPrestamoId(int prestamoId);
    Detalle? GetById(int id);
    void Insert(Detalle detalle);
    void InsertMany(IEnumerable<Detalle> detalles);
    void Update(Detalle detalle);
    void Delete(Detalle detalle);
}

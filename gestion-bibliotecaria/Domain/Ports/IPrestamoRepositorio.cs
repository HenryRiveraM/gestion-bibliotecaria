using gestion_bibliotecaria.Domain.Entities;

namespace gestion_bibliotecaria.Domain.Ports;

public interface IPrestamoRepositorio : IRepository<Prestamo, int>
{
    new int Insert(Prestamo prestamo);
    void InsertManyWithTransaction(IEnumerable<Prestamo> prestamos);
}

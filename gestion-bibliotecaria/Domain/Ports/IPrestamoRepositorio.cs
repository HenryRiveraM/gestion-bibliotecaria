using gestion_bibliotecaria.Domain.Entities;

namespace gestion_bibliotecaria.Domain.Ports;

public interface IPrestamoRepositorio
{
    IEnumerable<Prestamo> GetAll();
    void Insert(Prestamo prestamo);
    void Update(Prestamo prestamo);
    void Delete(Prestamo prestamo);
    Prestamo? GetById(int id);
    void InsertManyWithTransaction(IEnumerable<Prestamo> prestamos);
}

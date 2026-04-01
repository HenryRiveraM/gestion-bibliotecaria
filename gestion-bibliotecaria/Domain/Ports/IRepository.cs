using System.Data;

namespace gestion_bibliotecaria.Domain.Ports;
public interface IRepository<T,TId>
{
    DataTable GetAll();
    void Insert(T t);
    void Update(T t);
    void Delete(T t);
    T? GetById(TId id); 
}

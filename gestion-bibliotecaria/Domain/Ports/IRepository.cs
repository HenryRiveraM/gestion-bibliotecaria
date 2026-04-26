using System.Collections.Generic;

namespace gestion_bibliotecaria.Domain.Ports;
public interface IRepository<T,TId>
{
    IEnumerable<T> GetAll();
    void Insert(T t);
    void Update(T t);
    void Delete(T t);
    T? GetById(TId id); 
}


using System.Data;

namespace gestion_bibliotecaria.FactoryProducts;
public interface IRepository<T,TId>
{
    DataTable GetAll();
    void Insert(T t);
    void Update(T t);
    void Delete(T t);
    T? GetById(TId id); 
}

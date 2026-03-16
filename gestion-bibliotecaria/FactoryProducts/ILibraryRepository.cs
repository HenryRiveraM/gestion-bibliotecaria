using System.Data;

namespace gestion_bibliotecaria.FactoryProducts;

public interface ILibraryRepository<T>
{
    void Create(T item);
    T GetById(int id);
    DataTable GetAll();
    void Update(T item);
    void Delete(int id);
}
using System.Data;

namespace gestion_bibliotecaria.FactoryProducts;
public interface ILibraryRepository<T>// nom interface
{
    DataTable GetAll();
    void Insert(T t);
    void Update(T t);
    void Delete(T t);
    T? GetById(int id); // generico
}

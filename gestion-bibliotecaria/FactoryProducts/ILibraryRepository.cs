using System.Data;

namespace gestion_bibliotecaria.FactoryProducts;

/// <summary>
/// Interfaz genérica para repositorios (Repository Pattern).
/// </summary>
/// <typeparam name="T">Entidad del dominio</typeparam>
public interface ILibraryRepository<T>
{
    DataTable GetAll();
    void Insert(T t);
    void Update(T t);
    void Delete(T t);
    T? GetById(int id);
}

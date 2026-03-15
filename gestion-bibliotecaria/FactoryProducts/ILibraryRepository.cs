namespace gestion_bibliotecaria.FactoryProducts;

public interface ILibraryRepository<T>
{
    void DoStuff(T item);
}

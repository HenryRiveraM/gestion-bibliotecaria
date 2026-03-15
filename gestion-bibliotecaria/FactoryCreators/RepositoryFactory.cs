using gestion_bibliotecaria.FactoryProducts;

namespace gestion_bibliotecaria.FactoryCreators;

public abstract class RepositoryFactory<T>
{
    public abstract ILibraryRepository<T> CreateRepository();

}

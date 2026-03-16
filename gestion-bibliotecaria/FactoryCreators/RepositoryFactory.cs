using gestion_bibliotecaria.FactoryProducts;

namespace gestion_bibliotecaria.FactoryCreators;

public abstract class RepositoryFactory<T>
{
    protected readonly string ConnectionString;

    protected RepositoryFactory(string connectionString)
    {
        ConnectionString = connectionString;
    }
    public abstract ILibraryRepository<T> CreateRepository();
}
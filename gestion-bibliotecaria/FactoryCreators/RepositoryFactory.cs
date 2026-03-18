using gestion_bibliotecaria.FactoryProducts;

namespace gestion_bibliotecaria.FactoryCreators;

public abstract class RepositoryFactory<T,TId>
{
    protected readonly string ConnectionString;

    protected RepositoryFactory(string connectionString)
    {
        ConnectionString = connectionString;
    }
    public abstract IRepository<T,TId> CreateRepository();
}
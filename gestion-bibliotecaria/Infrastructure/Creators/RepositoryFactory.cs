using gestion_bibliotecaria.Domain.Ports;

namespace gestion_bibliotecaria.Infrastructure.Creators;

public abstract class RepositoryFactory<T,TId>
{
    protected readonly string ConnectionString;

    protected RepositoryFactory(string connectionString)
    {
        ConnectionString = connectionString;
    }
    public abstract IRepository<T,TId> CreateRepository();
}
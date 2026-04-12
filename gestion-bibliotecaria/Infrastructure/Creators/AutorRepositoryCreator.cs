using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Domain.Ports;
using gestion_bibliotecaria.Infrastructure.Persistence;

namespace gestion_bibliotecaria.Infrastructure.Creators;

public class AutorRepositoryCreator : RepositoryFactory<Autor,int>
{
    private readonly IConfiguration _configuration;

    public AutorRepositoryCreator(IConfiguration configuration)
        : base(configuration.GetConnectionString("DefaultConnection")!)
    {
        _configuration = configuration;
    }

    public override IRepository<Autor,int> CreateRepository()
    {
        return new AutorRepository();
    }
}

using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Domain.Ports;
using gestion_bibliotecaria.Infrastructure.Persistence;

namespace gestion_bibliotecaria.Infrastructure.Creators;

public class EjemplarRepositoryCreator : RepositoryFactory<Ejemplar,int>
{
    private readonly IConfiguration _configuration;

    public EjemplarRepositoryCreator(IConfiguration configuration)
        : base(configuration.GetConnectionString("DefaultConnection")!)
    {
        _configuration = configuration;
    }

    public override IRepository<Ejemplar,int> CreateRepository()
    {
        return new EjemplarRepository();
    }
}
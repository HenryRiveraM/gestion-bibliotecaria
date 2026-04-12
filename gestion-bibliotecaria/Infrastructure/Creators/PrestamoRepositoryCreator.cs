using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Domain.Ports;
using gestion_bibliotecaria.Infrastructure.Persistence;

namespace gestion_bibliotecaria.Infrastructure.Creators;

public class PrestamoRepositoryCreator : RepositoryFactory<Prestamo,int>
{
    private readonly IConfiguration _configuration;

    public PrestamoRepositoryCreator(IConfiguration configuration)
        : base(configuration.GetConnectionString("DefaultConnection")!)
    {
        _configuration = configuration;
    }

    public override IRepository<Prestamo,int> CreateRepository()
    {
        return new PrestamoRepository(_configuration);
    }
}

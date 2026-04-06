using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Domain.Ports;
using gestion_bibliotecaria.Infrastructure.Persistence;

namespace gestion_bibliotecaria.Infrastructure.Creators;

public class UsuarioRepositoryCreator : RepositoryFactory<Usuario, int>
{
    private readonly IConfiguration _configuration;

    public UsuarioRepositoryCreator(IConfiguration configuration)
        : base(configuration.GetConnectionString("DefaultConnection")!)
    {
        _configuration = configuration;
    }

    public override IRepository<Usuario, int> CreateRepository()
    {
        return new UsuarioRepository(_configuration);
    }
}

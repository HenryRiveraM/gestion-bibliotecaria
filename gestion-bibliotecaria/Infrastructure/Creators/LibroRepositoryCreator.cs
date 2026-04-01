using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Domain.Ports;
using gestion_bibliotecaria.Infrastructure.Persistence;

namespace gestion_bibliotecaria.Infrastructure.Creators;

public class LibroRepositoryCreator : RepositoryFactory<Libro, int>
{
    private readonly IConfiguration _configuration;

    public LibroRepositoryCreator(IConfiguration configuration)
        : base(configuration.GetConnectionString("DefaultConnection")!)
    {
        _configuration = configuration;
    }

    public override IRepository<Libro, int> CreateRepository()
    {
        return new LibroRepository(_configuration);
    }
}
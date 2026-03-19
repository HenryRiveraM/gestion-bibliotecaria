using gestion_bibliotecaria.FactoryProducts;
using gestion_bibliotecaria.Models;

namespace gestion_bibliotecaria.FactoryCreators;

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
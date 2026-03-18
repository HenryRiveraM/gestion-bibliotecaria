using gestion_bibliotecaria.FactoryProducts;
using gestion_bibliotecaria.Models;

namespace gestion_bibliotecaria.FactoryCreators;

public class EjemplarRepositoryCreator : RepositoryFactory<Ejemplar>
{
    private readonly IConfiguration _configuration;

    public EjemplarRepositoryCreator(IConfiguration configuration)
        : base(configuration.GetConnectionString("DefaultConnection")!)
    {
        _configuration = configuration;
    }

    public override ILibraryRepository<Ejemplar> CreateRepository()
    {
        return new EjemplarRepository(_configuration);
    }
}
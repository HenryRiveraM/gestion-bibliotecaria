using gestion_bibliotecaria.FactoryProducts;
using gestion_bibliotecaria.Models;

namespace gestion_bibliotecaria.FactoryCreators;

public class AutorRepositoryCreator : RepositoryFactory<Autor>
{
    private readonly IConfiguration _configuration;

    public AutorRepositoryCreator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public override ILibraryRepository<Autor> CreateRepository()
    {
        return new AutorRepository(_configuration);
    }
}

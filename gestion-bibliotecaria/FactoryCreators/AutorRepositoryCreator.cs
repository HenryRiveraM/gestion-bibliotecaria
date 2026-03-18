using gestion_bibliotecaria.FactoryProducts;
using gestion_bibliotecaria.Models;

namespace gestion_bibliotecaria.FactoryCreators;

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
        return new AutorRepository(_configuration);
    }
}

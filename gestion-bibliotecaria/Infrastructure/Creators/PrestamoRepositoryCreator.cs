using gestion_bibliotecaria.Domain.Ports;
using gestion_bibliotecaria.Infrastructure.Persistence;

namespace gestion_bibliotecaria.Infrastructure.Creators;

public class PrestamoRepositoryCreator
{
    private readonly IConfiguration _configuration;

    public PrestamoRepositoryCreator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IPrestamoRepositorio CreateRepository()
    {
        return new PrestamoRepository();
    }
}

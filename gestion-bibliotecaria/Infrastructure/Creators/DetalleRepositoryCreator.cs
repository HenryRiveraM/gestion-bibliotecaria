using gestion_bibliotecaria.Domain.Ports;
using gestion_bibliotecaria.Infrastructure.Persistence;

namespace gestion_bibliotecaria.Infrastructure.Creators;

public class DetalleRepositoryCreator
{
    private readonly IConfiguration _configuration;

    public DetalleRepositoryCreator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IDetalleRepositorio CreateRepository()
    {
        return new DetalleRepository();
    }
}

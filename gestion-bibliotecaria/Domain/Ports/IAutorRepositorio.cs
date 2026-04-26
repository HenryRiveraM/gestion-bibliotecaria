using gestion_bibliotecaria.Domain.Entities;

namespace gestion_bibliotecaria.Domain.Ports;

public interface IAutorRepositorio : IRepository<Autor, int>
{
    IEnumerable<Autor> ObtenerAutoresActivos();
    IEnumerable<Autor> ObtenerAutoresActivosTabla();
    bool ExisteAutorActivo(int autorId);
}

using gestion_bibliotecaria.Domain.Entities;

namespace gestion_bibliotecaria.Domain.Ports;

public interface IAutorRepositorio
{
    IEnumerable<Autor> GetAll();
    void Insert(Autor autor);
    void Update(Autor autor);
    void Delete(Autor autor);
    Autor? GetById(int id);

    IEnumerable<Autor> ObtenerAutoresActivos();
    IEnumerable<Autor> ObtenerAutoresActivosTabla();
    bool ExisteAutorActivo(int autorId);
}

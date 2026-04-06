using System.Data;
using gestion_bibliotecaria.Domain.Entities;

namespace gestion_bibliotecaria.Domain.Ports;

public interface IAutorRepositorio
{
    DataTable GetAll();
    void Insert(Autor autor);
    void Update(Autor autor);
    void Delete(Autor autor);
    Autor? GetById(int id);

    Dictionary<int, string> ObtenerAutoresActivos();
    DataTable ObtenerAutoresActivosTabla();
    bool ExisteAutorActivo(int autorId);
}
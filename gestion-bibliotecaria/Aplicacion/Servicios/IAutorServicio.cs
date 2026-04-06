using System.Data;
using gestion_bibliotecaria.Domain.Common;
using gestion_bibliotecaria.Domain.Entities;

namespace gestion_bibliotecaria.Aplicacion.Servicios;

public interface IAutorServicio
{
    DataTable Select();
    void Create(Autor autor);
    void Update(Autor autor);
    void Delete(Autor autor);
    Autor? GetById(int id);

    Dictionary<int, string> ObtenerAutoresActivos();
    DataTable ObtenerAutoresActivosTabla();
    bool ExisteAutorActivo(int autorId);

    Result ValidarAutor(Autor autor);
}
using System.Data;
using gestion_bibliotecaria.Domain.Entities;

namespace gestion_bibliotecaria.Domain.Ports;

public interface ILibroRepositorio
{
    DataTable Select();
    Libro? GetById(int id);
    void Create(Libro libro);
    void Update(Libro libro);
    void Delete(Libro libro);

    Dictionary<int, string> ObtenerNombresAutores();
    DataTable ObtenerAutoresActivos();
    bool ExisteAutorActivo(int autorId);
    int InsertarAutorYObtenerID(string nombreCompleto, int? usuarioSesionId);
}

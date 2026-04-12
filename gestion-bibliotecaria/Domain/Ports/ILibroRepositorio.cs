using gestion_bibliotecaria.Domain.Entities;

namespace gestion_bibliotecaria.Domain.Ports;

public interface ILibroRepositorio
{
    IEnumerable<Libro> Select();
    Libro? GetById(int id);
    void Create(Libro libro);
    void Update(Libro libro);
    void Delete(Libro libro);

    IEnumerable<Autor> ObtenerNombresAutores();
    IEnumerable<Autor> ObtenerAutoresActivos();
    bool ExisteAutorActivo(int autorId);
    int InsertarAutorYObtenerID(string nombreCompleto, int? usuarioSesionId);
}

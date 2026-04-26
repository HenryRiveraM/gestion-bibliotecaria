using gestion_bibliotecaria.Domain.Entities;

namespace gestion_bibliotecaria.Domain.Ports;

public interface ILibroRepositorio : IRepository<Libro, int>
{
    IEnumerable<Autor> ObtenerNombresAutores();
    IEnumerable<Autor> ObtenerAutoresActivos();
    bool ExisteAutorActivo(int autorId);
    int InsertarAutorYObtenerID(string nombreCompleto, int? usuarioSesionId);
}

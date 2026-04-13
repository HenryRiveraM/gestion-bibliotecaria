using gestion_bibliotecaria.Domain.Entities;

namespace gestion_bibliotecaria.Domain.Ports;

public interface IEjemplarRepositorio
{
    IEnumerable<Ejemplar> GetAll();
    void Insert(Ejemplar ejemplar);
    void Update(Ejemplar ejemplar);
    void Delete(Ejemplar ejemplar);
    Ejemplar? GetById(int id);

    Dictionary<int, string> ObtenerTitulosLibros();
    IEnumerable<Libro> ObtenerLibrosActivos();
    bool ExisteLibroActivo(int libroId);
    Dictionary<int, string> ObtenerEjemplaresDisponibles();
}
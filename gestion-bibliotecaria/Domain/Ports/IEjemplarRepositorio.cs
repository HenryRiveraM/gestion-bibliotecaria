using System.Data;
using gestion_bibliotecaria.Domain.Entities;

namespace gestion_bibliotecaria.Domain.Ports;

public interface IEjemplarRepositorio
{
    DataTable GetAll();
    void Insert(Ejemplar ejemplar);
    void Update(Ejemplar ejemplar);
    void Delete(Ejemplar ejemplar);
    Ejemplar? GetById(int id);

    Dictionary<int, string> ObtenerTitulosLibros();
    DataTable ObtenerLibrosActivos();
    bool ExisteLibroActivo(int libroId);
}
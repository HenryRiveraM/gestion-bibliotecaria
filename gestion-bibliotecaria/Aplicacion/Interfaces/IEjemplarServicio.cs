using System.Data;
using gestion_bibliotecaria.Domain.Common;
using gestion_bibliotecaria.Domain.Entities;

namespace gestion_bibliotecaria.Aplicacion.Interfaces;

public interface IEjemplarServicio
{
    DataTable Select();
    void Create(Ejemplar ejemplar);
    void Update(Ejemplar ejemplar);
    void Delete(Ejemplar ejemplar);
    Ejemplar? GetById(int id);

    Dictionary<int, string> ObtenerTitulosLibros();
    DataTable ObtenerLibrosActivos();
    bool ExisteLibroActivo(int libroId);
    Dictionary<int, string> ObtenerEjemplaresDisponibles();

    Result ValidarEjemplar(Ejemplar ejemplar);
}

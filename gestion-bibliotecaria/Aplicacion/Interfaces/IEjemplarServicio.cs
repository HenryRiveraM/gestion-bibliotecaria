using gestion_bibliotecaria.Aplicacion.Dtos;
using gestion_bibliotecaria.Domain.Common;
using gestion_bibliotecaria.Domain.Entities;

namespace gestion_bibliotecaria.Aplicacion.Interfaces;

public interface IEjemplarServicio
{
    IEnumerable<EjemplarDto> Select();
    Result<EjemplarDto> Create(EjemplarDto dto);
    Result<EjemplarDto> Update(EjemplarDto dto);
    Result Delete(EjemplarDto dto);
    EjemplarDto? GetById(int id);

    Dictionary<int, string> ObtenerTitulosLibros();
    IEnumerable<LibroDto> ObtenerLibrosActivos();
    bool ExisteLibroActivo(int libroId);
    Dictionary<int, string> ObtenerEjemplaresDisponibles();

    Result ValidarEjemplar(Ejemplar ejemplar);
}

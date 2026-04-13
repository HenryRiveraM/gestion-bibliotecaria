using gestion_bibliotecaria.Aplicacion.Dtos;
using gestion_bibliotecaria.Domain.Common;
using gestion_bibliotecaria.Domain.Entities;

namespace gestion_bibliotecaria.Aplicacion.Interfaces;

public interface ILibroServicio
{
    IEnumerable<LibroDto> Select();
    LibroDto? GetById(int id);

    Result Create(LibroDto libroDto, string? nombreAutorNuevo);
    Result Update(LibroDto libroDto);
    Result Delete(int libroId, int? usuarioSesionId);

    Dictionary<int, string> ObtenerNombresAutores();
    IEnumerable<AutorDto> ObtenerAutoresActivos();
    bool ExisteAutorActivo(int autorId);
    int InsertarAutorYObtenerID(string nombreCompleto, int? usuarioSesionId);
}

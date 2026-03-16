using gestion_bibliotecaria.Models;

namespace gestion_bibliotecaria.FactoryProducts;

public interface ILibroFactory
{
    Libro CreateForInsert(
        int autorId,
        string titulo,
        string? editorial,
        string? edicion,
        int? anioPublicacion,
        string? descripcion,
        bool estado);

    Libro CreateForUpdate(
        int libroId,
        int autorId,
        string titulo,
        string? editorial,
        string? edicion,
        int? anioPublicacion,
        string? descripcion,
        bool estado);
}
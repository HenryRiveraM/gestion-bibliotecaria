using gestion_bibliotecaria.FactoryProducts;
using gestion_bibliotecaria.Models;

namespace gestion_bibliotecaria.FactoryCreators;

public class LibroFactory : ILibroFactory
{
    public Libro CreateForInsert(
        int autorId,
        string titulo,
        string? editorial,
        string? edicion,
        int? anioPublicacion,
        string? descripcion,
        bool estado)
    {
        return new Libro
        {
            AutorId = autorId,
            Titulo = titulo,
            Editorial = editorial,
            Edicion = edicion,
            AñoPublicacion = anioPublicacion,
            Descripcion = descripcion,
            Estado = estado,
            FechaRegistro = DateTime.Now
        };
    }

    public Libro CreateForUpdate(
        int libroId,
        int autorId,
        string titulo,
        string? editorial,
        string? edicion,
        int? anioPublicacion,
        string? descripcion,
        bool estado)
    {
        return new Libro
        {
            LibroId = libroId,
            AutorId = autorId,
            Titulo = titulo,
            Editorial = editorial,
            Edicion = edicion,
            AñoPublicacion = anioPublicacion,
            Descripcion = descripcion,
            Estado = estado,
            UltimaActualizacion = DateTime.Now
        };
    }
}
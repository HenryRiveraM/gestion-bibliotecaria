using gestion_bibliotecaria.Models;

namespace gestion_bibliotecaria.FactoryProducts;

public interface IAutorFactory
{
    Autor CreateForInsert(
        string nombres,
        string? apellidos,
        string? nacionalidad,
        DateTime? fechaNacimiento,
        bool estado
    );

    Autor CreateForUpdate(
        int autorId,
        string nombres,
        string? apellidos,
        string? nacionalidad,
        DateTime? fechaNacimiento,
        bool estado
    );
}

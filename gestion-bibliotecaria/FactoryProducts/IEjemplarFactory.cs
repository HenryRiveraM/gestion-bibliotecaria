using gestion_bibliotecaria.Models;

namespace gestion_bibliotecaria.FactoryProducts;

public interface IEjemplarFactory
{
    Ejemplar CreateForInsert(
        int libroId,
        string codigoInventario,
        string? estadoConservacion,
        bool disponible,
        bool dadoDeBaja,
        string? motivoBaja,
        string? ubicacion,
        bool estado
    );

    Ejemplar CreateForUpdate(
        int ejemplarId,
        int libroId,
        string codigoInventario,
        string? estadoConservacion,
        bool disponible,
        bool dadoDeBaja,
        string? motivoBaja,
        string? ubicacion,
        bool estado
    );
}
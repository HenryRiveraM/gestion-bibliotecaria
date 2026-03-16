using gestion_bibliotecaria.FactoryProducts;
using gestion_bibliotecaria.Models;

namespace gestion_bibliotecaria.FactoryCreators;

public class EjemplarFactory : IEjemplarFactory
{
    public Ejemplar CreateForInsert(
        int libroId,
        string codigoInventario,
        string? estadoConservacion,
        bool disponible,
        bool dadoDeBaja,
        string? motivoBaja,
        string? ubicacion,
        bool estado)
    {
        return new Ejemplar
        {
            LibroId = libroId,
            CodigoInventario = codigoInventario,
            EstadoConservacion = estadoConservacion,
            Disponible = disponible,
            DadoDeBaja = dadoDeBaja,
            MotivoBaja = motivoBaja,
            Ubicacion = ubicacion,
            Estado = estado,
            FechaRegistro = DateTime.Now
        };
    }

    public Ejemplar CreateForUpdate(
        int ejemplarId,
        int libroId,
        string codigoInventario,
        string? estadoConservacion,
        bool disponible,
        bool dadoDeBaja,
        string? motivoBaja,
        string? ubicacion,
        bool estado)
    {
        return new Ejemplar
        {
            EjemplarId = ejemplarId,
            LibroId = libroId,
            CodigoInventario = codigoInventario,
            EstadoConservacion = estadoConservacion,
            Disponible = disponible,
            DadoDeBaja = dadoDeBaja,
            MotivoBaja = motivoBaja,
            Ubicacion = ubicacion,
            Estado = estado,
            UltimaActualizacion = DateTime.Now
        };
    }
}
using gestion_bibliotecaria.FactoryProducts;
using gestion_bibliotecaria.Models;

namespace gestion_bibliotecaria.FactoryCreators;

public class AutorFactory : IAutorFactory
{
    public Autor CreateForInsert(
        string nombres,
        string? apellidos,
        string? nacionalidad,
        DateTime? fechaNacimiento,
        bool estado)
    {
        return new Autor
        {
            Nombres = nombres,
            Apellidos = apellidos,
            Nacionalidad = nacionalidad,
            FechaNacimiento = fechaNacimiento,
            Estado = estado,
            FechaRegistro = DateTime.Now
        };
    }

    public Autor CreateForUpdate(
        int autorId,
        string nombres,
        string? apellidos,
        string? nacionalidad,
        DateTime? fechaNacimiento,
        bool estado)
    {
        return new Autor
        {
            AutorId = autorId,
            Nombres = nombres,
            Apellidos = apellidos,
            Nacionalidad = nacionalidad,
            FechaNacimiento = fechaNacimiento,
            Estado = estado,
            UltimaActualizacion = DateTime.Now
        };
    }
}

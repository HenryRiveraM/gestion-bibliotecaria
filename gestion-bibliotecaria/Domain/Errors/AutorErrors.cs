using gestion_bibliotecaria.Domain.Common;

namespace gestion_bibliotecaria.Domain.Errors;

public static class AutorErrors
{
    public static readonly Error NombresObligatorios = new("Autor.Nombres", "Los nombres son obligatorios.");
    public static readonly Error NombresFormato = new("Autor.Nombres", "Los nombres solo pueden contener letras y espacios.");
    public static readonly Error NombresLongitud = new("Autor.Nombres", "Los nombres exceden la longitud máxima de 100 caracteres.");
    public static readonly Error ApellidosFormato = new("Autor.Apellidos", "Los apellidos solo pueden contener letras y espacios.");
    public static readonly Error ApellidosLongitud = new("Autor.Apellidos", "Los apellidos exceden la longitud máxima de 100 caracteres.");
    public static readonly Error NacionalidadFormato = new("Autor.Nacionalidad", "La nacionalidad solo puede contener letras y espacios.");
    public static readonly Error NacionalidadLongitud = new("Autor.Nacionalidad", "La nacionalidad excede la longitud máxima de 100 caracteres.");
    public static readonly Error FechaFutura = new("Autor.FechaNacimiento", "La fecha de nacimiento no puede ser futura.");
}
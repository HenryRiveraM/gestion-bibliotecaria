using gestion_bibliotecaria.Domain.Common;

namespace gestion_bibliotecaria.Domain.Errors;

public static class LibroErrors
{
    public static readonly Error TituloObligatorio = new("Libro.Titulo", "El título es obligatorio.");
    public static readonly Error TituloLongitud = new("Libro.Titulo", "El título excede la longitud máxima de 200 caracteres.");
    public static readonly Error IsbnLongitud = new("Libro.ISBN", "El ISBN excede la longitud máxima de 20 caracteres.");
    public static readonly Error IsbnInvalido = new("Libro.ISBN", "El ISBN debe contener 10 o 13 dígitos.");
    public static readonly Error EditorialLongitud = new("Libro.Editorial", "La editorial excede la longitud máxima de 100 caracteres.");
    public static readonly Error GeneroLongitud = new("Libro.Genero", "El género excede la longitud máxima de 100 caracteres.");
    public static readonly Error EdicionLongitud = new("Libro.Edicion", "La edición excede la longitud máxima de 50 caracteres.");
    public static readonly Error PaginasInvalidas = new("Libro.NumeroPaginas", "El número de páginas debe ser mayor a 0.");
    public static readonly Error AnioInvalido = new("Libro.AñoPublicacion", "El año de publicación no es válido.");
    public static readonly Error IdiomaLongitud = new("Libro.Idioma", "El idioma excede la longitud máxima de 50 caracteres.");
    public static readonly Error IdiomaInvalido = new("Libro.Idioma", "Seleccione un idioma válido.");
    public static readonly Error PaisLongitud = new("Libro.PaisPublicacion", "El país de publicación excede la longitud máxima de 100 caracteres.");
    public static readonly Error DescripcionLongitud = new("Libro.Descripcion", "La descripción excede la longitud máxima de 500 caracteres.");
    public static readonly Error AutorRequerido = new("Libro.AutorId", "Seleccione un autor o escriba el nombre de uno nuevo.");
}
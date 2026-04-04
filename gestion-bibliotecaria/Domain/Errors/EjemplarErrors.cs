using gestion_bibliotecaria.Domain.Common;

namespace gestion_bibliotecaria.Domain.Errors;

public static class EjemplarErrors
{
    public static readonly Error CodigoRequerido = new("Ejemplar.CodigoInventario", "El código de inventario es obligatorio.");
    public static readonly Error CodigoFormato = new("Ejemplar.CodigoInventario", "El código de inventario solo puede contener letras, números y guiones.");
    public static readonly Error CodigoLongitud = new("Ejemplar.CodigoInventario", "El código de inventario excede la longitud máxima de 30 caracteres.");
    public static readonly Error CodigoDuplicado = new("Ejemplar.CodigoInventario", "Ya existe un ejemplar con ese código de inventario.");
    public static readonly Error LibroInvalido = new("Ejemplar.LibroId", "El libro seleccionado está inactivo o no existe.");
    public static readonly Error CamposIncompletos = new("Ejemplar.General", "Por favor completa todos los campos requeridos.");
    public static readonly Error ErrorProcesado = new("Ejemplar.General", "Ocurrió un error al procesar el ejemplar. Por favor, intentá nuevamente.");
}
using gestion_bibliotecaria.Domain.Common;

namespace gestion_bibliotecaria.Domain.Errors;

public static class PrestamoErrors
{
    public static readonly Error DatosObligatorios = new("Prestamo.Datos", "Complete los campos obligatorios del prestamo.");
    public static readonly Error EjemplarNoDisponible = new("Prestamo.EjemplarNoDisponible", "El ejemplar seleccionado no está disponible.");
    public static readonly Error LectorInvalido = new("Prestamo.LectorInvalido", "El lector seleccionado no es válido o no está activo.");
    public static readonly Error FechaDevolucionInvalida = new("Prestamo.FechaDevolucionInvalida", "La fecha de devolución esperada debe ser mayor o igual a la fecha de préstamo.");
}

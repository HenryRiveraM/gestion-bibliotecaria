namespace gestion_bibliotecaria.Aplicacion.DTOs;

public class PrestamoDetalleDTO
{
    public int PrestamoId { get; set; }
    public int EjemplarId { get; set; }
    public int LectorId { get; set; }
    
    // Información del Libro
    public string TituloLibro { get; set; } = string.Empty;
    public string CodigoInventario { get; set; } = string.Empty;
    
    // Información del Lector
    public string NombreLector { get; set; } = string.Empty;
    
    // Fechas
    public DateTime FechaPrestamo { get; set; }
    public DateTime FechaDevolucionEsperada { get; set; }
    public DateTime? FechaDevolucionReal { get; set; }
    
    // Observaciones
    public string? ObservacionesSalida { get; set; }
    public string? ObservacionesEntrada { get; set; }
    
    // Estado
    public int Estado { get; set; }
    public DateTime FechaRegistro { get; set; }
    
    public bool EstaDevuelto => FechaDevolucionReal.HasValue;
}

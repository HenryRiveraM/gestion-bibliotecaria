namespace gestion_bibliotecaria.Domain.Entities;

public class Detalle
{
    public int DetalleId { get; set; }
    public int PrestamoId { get; set; }
    public int EjemplarId { get; set; }
    
    // Estado por ejemplar en el detalle del préstamo:
    // 1 = PRESTADO (pendiente de devolución)
    // 2 = DEVUELTO
    // 0 = ANULADO
    public byte EstadoDetalle { get; set; } = 1;
    
    public DateTime? FechaDevolucionReal { get; set; }
    public string? ObservacionesSalida { get; set; }
    public string? ObservacionesEntrada { get; set; }
    
    public int? UsuarioSesionId { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
    public DateTime? UltimaActualizacion { get; set; }
}

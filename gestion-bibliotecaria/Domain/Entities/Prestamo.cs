namespace gestion_bibliotecaria.Domain.Entities;

public class Prestamo
{
    public int PrestamoId { get; set; }
    public int EjemplarId { get; set; }
    public int LectorId { get; set; } 
    public DateTime FechaPrestamo { get; set; } = DateTime.Now;
    public DateTime FechaDevolucionEsperada { get; set; }
    public DateTime? FechaDevolucionReal { get; set; }
    public string? ObservacionesSalida { get; set; }
    public string? ObservacionesEntrada { get; set; }
    public int Estado { get; set; } = 1; 
    public int? UsuarioSesionId { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
    public DateTime? UltimaActualizacion { get; set; }
}
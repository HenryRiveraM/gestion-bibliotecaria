namespace gestion_bibliotecaria.Domain.Entities;

public class Ejemplar
{
    public int EjemplarId { get; set; }
    public int? UsuarioSesionId { get; set; }
    public string RouteToken { get; set; } = string.Empty;
    public int LibroId { get; set; }
    public string CodigoInventario { get; set; } = string.Empty;
    public string? EstadoConservacion { get; set; }
    public bool Disponible { get; set; }
    public bool DadoDeBaja { get; set; }
    public string? MotivoBaja { get; set; }
    public string? Ubicacion { get; set; }
    public bool Estado { get; set; }
    public DateTime FechaRegistro { get; set; }
    public DateTime? UltimaActualizacion { get; set; }
    public string? LibroTitulo { get; set; }
}

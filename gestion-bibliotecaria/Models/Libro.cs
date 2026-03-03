namespace gestion_bibliotecaria.Models;

public class Libro
{
    public int LibroId { get; set; }
    public int AutorId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Editorial { get; set; }
    public string? Edicion { get; set; }
    public int? AñoPublicacion { get; set; }
    public string? Descripcion { get; set; }
    public bool Estado { get; set; }
    public DateTime FechaRegistro { get; set; }
    public DateTime? UltimaActualizacion { get; set; }
}

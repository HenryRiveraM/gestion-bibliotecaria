namespace gestion_bibliotecaria.Domain.Entities;

public class Libro
{
    public int LibroId { get; set; }
    public int AutorId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? ISBN  { get; set; }
    public string? Editorial { get; set; }
    public string? Genero { get; set; }
    public string? Edicion { get; set; }
    public int? AñoPublicacion { get; set; }
    public int? NumeroPaginas { get; set; }
    public string? Idioma { get; set; }
    public string? PaisPublicacion { get; set; }
    public string? Descripcion { get; set; }
    public bool Estado { get; set; }
    public DateTime FechaRegistro { get; set; }
    public DateTime? UltimaActualizacion { get; set; }
}

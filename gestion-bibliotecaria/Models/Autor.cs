namespace gestion_bibliotecaria.Models;

public class Autor
{
    public int AutorId { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string? Nacionalidad { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public bool Estado { get; set; }
    public DateTime FechaRegistro { get; set; }
    public DateTime? UltimaActualizacion { get; set; }
}

namespace gestion_bibliotecaria.Domain.Entities;

public class Autor
{
    public int AutorId { get; set; }
    public string RouteToken { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string? Apellidos { get; set; }
    public string? Nacionalidad { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public bool Estado { get; set; }
    public DateTime FechaRegistro { get; set; }
    public DateTime? UltimaActualizacion { get; set; }
}

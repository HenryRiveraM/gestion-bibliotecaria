using System;

namespace gestion_bibliotecaria.Aplicacion.Dtos;

public class AutorDto
{
    public int AutorId { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string? Apellidos { get; set; }
    public string? Nacionalidad { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public bool Estado { get; set; }
    public string RouteToken { get; set; } = string.Empty;
}

namespace gestion_bibliotecaria.Domain.Entities;

public class Usuario
{
    public const string RolAdmin = "Admin";
    public const string RolBibliotecario = "Bibliotecario";
    public const string RolLector = "Lector";

    public int UsuarioId { get; set; }
    public string? CI { get; set; }
    public int? UsuarioSesionId { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string PrimerApellido { get; set; } = string.Empty;
    public string? SegundoApellido { get; set; }
    public string Email { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Salt { get; set; }
    public string Rol { get; set; } = RolBibliotecario;
    public bool Estado { get; set; } = true;
    public DateTime FechaRegistro { get; set; }
    public DateTime? UltimaActualizacion { get; set; }
}

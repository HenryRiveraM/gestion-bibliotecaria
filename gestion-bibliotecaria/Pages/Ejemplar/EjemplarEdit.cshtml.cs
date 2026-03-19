using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gestion_bibliotecaria.Models;
using gestion_bibliotecaria.Security;
using MySql.Data.MySqlClient;
using gestion_bibliotecaria.Validaciones;
using gestion_bibliotecaria.FactoryProducts;

namespace gestion_bibliotecaria.Pages;

public class EjemplarEditModel : PageModel
{
    private const string QueryEjemplarPorId = @"SELECT EjemplarId, LibroId, CodigoInventario, EstadoConservacion, Disponible, DadoDeBaja, MotivoBaja, Ubicacion, Estado, FechaRegistro, UltimaActualizacion
                                                FROM ejemplar
                                                WHERE EjemplarId = @EjemplarId";

    private const string QueryLibros = @"SELECT LibroId, AutorId, Titulo, Editorial, Edicion, AñoPublicacion, Descripcion, Estado, FechaRegistro, UltimaActualizacion
                                        FROM libro
                                        ORDER BY Titulo";

    private const string QueryUpdateEjemplar = @"UPDATE ejemplar
                                                 SET LibroId = @LibroId,
                                                     CodigoInventario = @CodigoInventario,
                                                     EstadoConservacion = @EstadoConservacion,
                                                     Disponible = @Disponible,
                                                     DadoDeBaja = @DadoDeBaja,
                                                     MotivoBaja = @MotivoBaja,
                                                     Ubicacion = @Ubicacion,
                                                     Estado = @Estado,
                                                     UltimaActualizacion = @UltimaActualizacion
                                                 WHERE EjemplarId = @EjemplarId";

    private readonly IConfiguration _configuration;
    private readonly RouteTokenService _routeTokenService;
    private readonly IEjemplarFactory _ejemplarFactory;

    [BindProperty]
    public Ejemplar Ejemplar { get; set; } = new Ejemplar();

    [BindProperty]
    public string EjemplarToken { get; set; } = string.Empty;

    public List<Libro> Libros { get; set; } = new();

    public string ErrorMessage { get; set; } = string.Empty;

    public EjemplarEditModel(
        IConfiguration configuration,
        RouteTokenService routeTokenService,
        IEjemplarFactory ejemplarFactory)
    {
        _configuration = configuration;
        _routeTokenService = routeTokenService;
        _ejemplarFactory = ejemplarFactory;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!_routeTokenService.TryObtenerId(EjemplarToken, out var ejemplarId))
        {
            return NotFound();
        }

        Ejemplar.CodigoInventario = ValidadorEntrada.NormalizarEspacios(Ejemplar.CodigoInventario);
        Ejemplar.EstadoConservacion = ValidadorEntrada.NormalizarEspacios(Ejemplar.EstadoConservacion);
        Ejemplar.Ubicacion = ValidadorEntrada.NormalizarEspacios(Ejemplar.Ubicacion);
        Ejemplar.MotivoBaja = ValidadorEntrada.NormalizarEspacios(Ejemplar.MotivoBaja);

        if (ValidadorEntrada.EstaVacio(Ejemplar.CodigoInventario))
        {
            ModelState.AddModelError("CodigoInventario", "El código de inventario es obligatorio.");
        }
        else if (!ValidadorEntrada.CodigoInventarioValido(Ejemplar.CodigoInventario))
        {
            ModelState.AddModelError("CodigoInventario", "El código de inventario solo puede tener letras, números o guiones.");
        }
        else if (ValidadorEntrada.ExcedeLongitud(Ejemplar.CodigoInventario, 30))
        {
            ModelState.AddModelError("CodigoInventario", "El código de inventario excede la longitud máxima de 30 caracteres.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var ejemplar = _ejemplarFactory.CreateForUpdate(
            ejemplarId,
            Ejemplar.LibroId,
            Ejemplar.CodigoInventario,
            Ejemplar.EstadoConservacion,
            Ejemplar.Disponible,
            Ejemplar.DadoDeBaja,
            Ejemplar.MotivoBaja,
            Ejemplar.Ubicacion,
            Ejemplar.Estado
        );

        try
        {
            await ActualizarEjemplarAsync(ejemplar);
        }
        catch (MySql.Data.MySqlClient.MySqlException ex) when (ex.Number == 1062)
        {
            ModelState.AddModelError("CodigoInventario", "Ya existe un ejemplar con ese código de inventario.");
            return Page();
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Ocurrió un error al actualizar el ejemplar. Por favor, intentá nuevamente.");
            return Page();
        }

        return Redirect("/Ejemplar");
    }

    private string ConnectionString => _configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    private async Task<bool> ActualizarEjemplarAsync(Ejemplar ejemplar)
    {
        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        using var command = new MySqlCommand(QueryUpdateEjemplar, connection);

        command.Parameters.AddWithValue("@EjemplarId", ejemplar.EjemplarId);
        command.Parameters.AddWithValue("@LibroId", ejemplar.LibroId);
        command.Parameters.AddWithValue("@CodigoInventario", ejemplar.CodigoInventario);
        command.Parameters.AddWithValue("@EstadoConservacion", ejemplar.EstadoConservacion ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Disponible", ejemplar.Disponible);
        command.Parameters.AddWithValue("@DadoDeBaja", ejemplar.DadoDeBaja);
        command.Parameters.AddWithValue("@MotivoBaja", ejemplar.MotivoBaja ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Ubicacion", ejemplar.Ubicacion ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Estado", ejemplar.Estado);
        command.Parameters.AddWithValue("@UltimaActualizacion", ejemplar.UltimaActualizacion ?? DateTime.Now);

        var rows = await command.ExecuteNonQueryAsync();
        return rows > 0;
    }
}
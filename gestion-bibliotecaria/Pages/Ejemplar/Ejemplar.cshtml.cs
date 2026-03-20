using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gestion_bibliotecaria.Models;
using gestion_bibliotecaria.Security;
using gestion_bibliotecaria.FactoryProducts;
using gestion_bibliotecaria.FactoryCreators;
using gestion_bibliotecaria.Validaciones;
using MySql.Data.MySqlClient;
using System.Data;

namespace gestion_bibliotecaria.Pages;

public class EjemplarModel : PageModel
{
    private readonly IRepository<Ejemplar, int> _repository;
    private readonly RouteTokenService _routeTokenService;
    private readonly IConfiguration _configuration;

    public List<Ejemplar> Ejemplares { get; set; } = new();
    public Dictionary<int, string> LibrosTitulos { get; set; } = new();
    public List<Libro> Libros { get; set; } = new();

    public string ErrorMessage { get; set; } = string.Empty;

    public EjemplarModel(
        RepositoryFactory<Ejemplar, int> factory,
        RouteTokenService routeTokenService,
        IConfiguration configuration)
    {
        _repository = factory.CreateRepository();
        _routeTokenService = routeTokenService;
        _configuration = configuration;
    }

    public async Task OnGetAsync()
    {
        await CargarPaginaAsync();
    }

    public async Task<IActionResult> OnPostEliminarAsync(string token)
    {
        if (!_routeTokenService.TryObtenerId(token, out var id))
        {
            return NotFound();
        }

        try
        {
            var ejemplar = _repository.GetById(id);

            if (ejemplar == null)
            {
                return NotFound();
            }

            _repository.Delete(ejemplar);

            return RedirectToPage();
        }
        catch
        {
            await CargarPaginaAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostEditarAsync(
        string token,
        int LibroId,
        string CodigoInventario,
        string? EstadoConservacion,
        bool? Disponible,
        bool? DadoDeBaja,
        string? MotivoBaja,
        string? Ubicacion,
        bool? Estado)
    {
        if (!_routeTokenService.TryObtenerId(token, out var ejemplarId))
        {
            return NotFound();
        }

        CodigoInventario = ValidadorEntrada.NormalizarEspacios(CodigoInventario);
        EstadoConservacion = ValidadorEntrada.NormalizarEspacios(EstadoConservacion);
        Ubicacion = ValidadorEntrada.NormalizarEspacios(Ubicacion);
        MotivoBaja = ValidadorEntrada.NormalizarEspacios(MotivoBaja);

        if (ValidadorEntrada.EstaVacio(CodigoInventario))
        {
            ModelState.AddModelError("CodigoInventario", "El código de inventario es obligatorio.");
        }
        else if (!ValidadorEntrada.CodigoInventarioValido(CodigoInventario))
        {
            ModelState.AddModelError("CodigoInventario", "El código de inventario solo puede tener letras, números o guiones.");
        }
        else if (ValidadorEntrada.ExcedeLongitud(CodigoInventario, 30))
        {
            ModelState.AddModelError("CodigoInventario", "El código de inventario excede la longitud máxima de 30 caracteres.");
        }

        if (!ModelState.IsValid)
        {
            await CargarPaginaAsync();
            return Page();
        }

        if (!await ExisteLibroActivoAsync(LibroId))
        {
            ModelState.AddModelError("LibroId", "El libro seleccionado está inactivo o no existe.");
            await CargarPaginaAsync();
            return Page();
        }

        var ejemplar = new Ejemplar
        {
            EjemplarId = ejemplarId,
            LibroId = LibroId,
            CodigoInventario = CodigoInventario,
            EstadoConservacion = EstadoConservacion,
            Disponible = Disponible ?? false,
            DadoDeBaja = DadoDeBaja ?? false,
            MotivoBaja = MotivoBaja,
            Ubicacion = Ubicacion,
            Estado = Estado ?? false
        };

        try
        {
            _repository.Update(ejemplar);
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            ModelState.AddModelError("CodigoInventario", "Ya existe un ejemplar con ese código de inventario.");
            await CargarPaginaAsync();
            return Page();
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Ocurrió un error al actualizar el ejemplar. Por favor, intentá nuevamente.");
            await CargarPaginaAsync();
            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCrearAsync(Ejemplar Ejemplar)
    {
        Ejemplar.CodigoInventario = NormalizarCodigoInventario(
            ValidadorEntrada.NormalizarEspacios(Ejemplar.CodigoInventario)
        );
        Ejemplar.EstadoConservacion = ValidadorEntrada.NormalizarEspacios(Ejemplar.EstadoConservacion);
        Ejemplar.Ubicacion = ValidadorEntrada.NormalizarEspacios(Ejemplar.Ubicacion);
        Ejemplar.MotivoBaja = ValidadorEntrada.NormalizarEspacios(Ejemplar.MotivoBaja);

        if (ValidadorEntrada.EstaVacio(Ejemplar.CodigoInventario))
        {
            ModelState.AddModelError("Ejemplar.CodigoInventario", "El código de inventario es obligatorio.");
        }
        else if (!ValidadorEntrada.CodigoInventarioValido(Ejemplar.CodigoInventario))
        {
            ModelState.AddModelError("Ejemplar.CodigoInventario", "El código de inventario solo puede contener letras, números y guiones.");
        }
        else if (ValidadorEntrada.ExcedeLongitud(Ejemplar.CodigoInventario, 30))
        {
            ModelState.AddModelError("Ejemplar.CodigoInventario", "El código de inventario excede la longitud máxima de 30 caracteres.");
        }

        if (!ModelState.IsValid)
        {
            ErrorMessage = "Por favor completa todos los campos requeridos.";
            await CargarPaginaAsync();
            return Page();
        }

        if (!await ExisteLibroActivoAsync(Ejemplar.LibroId))
        {
            ModelState.AddModelError("Ejemplar.LibroId", "El libro seleccionado está inactivo o no existe.");
            await CargarPaginaAsync();
            return Page();
        }

        try
        {
            _repository.Insert(Ejemplar);
            return RedirectToPage();
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            ModelState.AddModelError("Ejemplar.CodigoInventario", "Ya existe un ejemplar con ese código de inventario.");
            await CargarPaginaAsync();
            return Page();
        }
        catch (Exception)
        {
            ErrorMessage = "Ocurrió un error al agregar el ejemplar. Por favor, intentá nuevamente.";
            await CargarPaginaAsync();
            return Page();
        }
    }

    public static string NormalizarCodigoInventario(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        input = input.Trim().ToUpper();

        var numero = new string(input.Where(char.IsDigit).ToArray());

        if (string.IsNullOrEmpty(numero))
            return input;

        int num = int.Parse(numero);
        string numeroFormateado = num.ToString("D3");
        int año = DateTime.Now.Year;

        return $"INV-{numeroFormateado}-{año}";
    }

    private async Task CargarPaginaAsync()
    {
        var tabla = _repository.GetAll();

        Ejemplares = new List<Ejemplar>();

        foreach (DataRow row in tabla.Rows)
        {
            var ejemplar = new Ejemplar
            {
                EjemplarId = Convert.ToInt32(row["EjemplarId"]),
                LibroId = Convert.ToInt32(row["LibroId"]),
                CodigoInventario = row["CodigoInventario"].ToString()!,
                EstadoConservacion = row["EstadoConservacion"] == DBNull.Value ? null : row["EstadoConservacion"].ToString(),
                Disponible = Convert.ToBoolean(row["Disponible"]),
                DadoDeBaja = Convert.ToBoolean(row["DadoDeBaja"]),
                MotivoBaja = row["MotivoBaja"] == DBNull.Value ? null : row["MotivoBaja"].ToString(),
                Ubicacion = row["Ubicacion"] == DBNull.Value ? null : row["Ubicacion"].ToString(),
                Estado = Convert.ToBoolean(row["Estado"]),
                FechaRegistro = row.Table.Columns.Contains("FechaRegistro") && row["FechaRegistro"] != DBNull.Value
                    ? Convert.ToDateTime(row["FechaRegistro"])
                    : DateTime.MinValue,
                UltimaActualizacion = row.Table.Columns.Contains("UltimaActualizacion") && row["UltimaActualizacion"] != DBNull.Value
                    ? Convert.ToDateTime(row["UltimaActualizacion"])
                    : null
            };

            ejemplar.RouteToken = _routeTokenService.CrearToken(ejemplar.EjemplarId);
            Ejemplares.Add(ejemplar);
        }

        LibrosTitulos = await ObtenerTitulosLibrosAsync();
        Libros = await ObtenerLibrosAsync();
    }

    private string ConnectionString => _configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string not found.");

    private async Task<Dictionary<int, string>> ObtenerTitulosLibrosAsync()
    {
        var titulos = new Dictionary<int, string>();

        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        string query = "SELECT LibroId, Titulo FROM libro";

        using var command = new MySqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            titulos[reader.GetInt32("LibroId")] = reader.GetString("Titulo");
        }

        return titulos;
    }

    private async Task<List<Libro>> ObtenerLibrosAsync()
    {
        var libros = new List<Libro>();

        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        string query = "SELECT LibroId, Titulo, Editorial FROM libro WHERE Estado = 1 ORDER BY Titulo";

        using var command = new MySqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            libros.Add(new Libro
            {
                LibroId = reader.GetInt32("LibroId"),
                Titulo = reader.GetString("Titulo"),
                Editorial = reader["Editorial"] == DBNull.Value ? null : reader["Editorial"].ToString()
            });
        }

        return libros;
    }

    private async Task<bool> ExisteLibroActivoAsync(int libroId)
    {
        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        const string query = "SELECT COUNT(1) FROM libro WHERE LibroId = @LibroId AND Estado = 1";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@LibroId", libroId);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }
}
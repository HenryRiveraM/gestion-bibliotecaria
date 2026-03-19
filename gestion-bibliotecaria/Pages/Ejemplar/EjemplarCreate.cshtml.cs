using gestion_bibliotecaria.FactoryCreators;
using gestion_bibliotecaria.FactoryProducts;
using gestion_bibliotecaria.Models;
using gestion_bibliotecaria.Validaciones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.Data;

namespace gestion_bibliotecaria.Pages;

public class EjemplarCreateModel : PageModel
{
    private readonly IRepository<Ejemplar, int> _repository;
    private readonly IConfiguration _configuration;

    [BindProperty]
    public Ejemplar Ejemplar { get; set; } = new Ejemplar
    {
        Estado = true,
        Disponible = true
    };

    public List<Libro> Libros { get; set; } = new();

    public string ErrorMessage { get; set; } = string.Empty;

    public EjemplarCreateModel(
        RepositoryFactory<Ejemplar, int> factory,
        IConfiguration configuration)
    {
        _repository = factory.CreateRepository();
        _configuration = configuration;
    }

    public async Task OnGetAsync()
    {
        await CargarPaginaAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Ejemplar.CodigoInventario = NormalizarCodigoInventario( ValidadorEntrada.NormalizarEspacios(Ejemplar.CodigoInventario));
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

        try
        {
            _repository.Insert(Ejemplar);

            return Redirect("/Ejemplar");
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

    //NormalizarCodInventario
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


    private string ConnectionString => _configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string not found.");

    private async Task CargarPaginaAsync()
    {
        Libros = await ObtenerLibrosAsync();
    }

    private async Task<List<Libro>> ObtenerLibrosAsync()
    {
        var libros = new List<Libro>();

        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        string query = "SELECT LibroId, Titulo, Editorial  FROM libro ORDER BY Titulo";

        using var command = new MySqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            libros.Add(new Libro
            {
                LibroId = reader.GetInt32("LibroId"),
                Titulo = reader.GetString("Titulo"),
                Editorial = reader["Editorial"] == DBNull.Value
                ? null
                : reader["Editorial"].ToString()
            });
        }

        return libros;
    }
}
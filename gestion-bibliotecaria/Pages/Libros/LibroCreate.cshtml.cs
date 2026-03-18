using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using gestion_bibliotecaria.Validaciones;
using gestion_bibliotecaria.FactoryProducts;

namespace gestion_bibliotecaria.Pages;

public class LibroCreateModel : PageModel
{
    private readonly ILibroFactory _libroFactory;
    private readonly LibroRepository _repository;
    
    public LibroCreateModel(ILibroFactory libroFactory, LibroRepository repository)
    {
        _libroFactory = libroFactory;
        _repository = repository;
    }
    
    [BindProperty]
    public int AutorId { get; set; }

    [BindProperty]
    public string Titulo { get; set; } = string.Empty;

    [BindProperty]
    public string? Editorial { get; set; }

    [BindProperty]
    public string? Edicion { get; set; }

    [BindProperty]
    public int? AñoPublicacion { get; set; }

    [BindProperty]
    public string? Descripcion { get; set; }

    [BindProperty]
    public bool Estado { get; set; } = true;

    public DataTable Autores { get; set; } = new DataTable();

    public void OnGet()
    {
        CargarPagina();
    }

    public IActionResult OnPost()
    {
        Titulo = ValidadorEntrada.NormalizarEspacios(Titulo);
        Editorial = ValidadorEntrada.NormalizarEspacios(Editorial);
        Edicion = ValidadorEntrada.NormalizarEspacios(Edicion);
        Descripcion = ValidadorEntrada.NormalizarEspacios(Descripcion);

        if (ValidadorEntrada.EstaVacio(Titulo))
        {
            ModelState.AddModelError("Titulo", "El título es obligatorio.");
        }
        else if (ValidadorEntrada.ExcedeLongitud(Titulo, 100))
        {
            ModelState.AddModelError("Titulo", "El título excede la longitud máxima de 100 caracteres.");
        }

        if (!string.IsNullOrWhiteSpace(Editorial))
        {
            if (ValidadorEntrada.ExcedeLongitud(Editorial, 100))
            {
                ModelState.AddModelError("Editorial", "La editorial excede la longitud máxima de 100 caracteres.");
            }
        }

        if (!string.IsNullOrWhiteSpace(Edicion))
        {
            if (ValidadorEntrada.ExcedeLongitud(Edicion, 50))
            {
                ModelState.AddModelError("Edicion", "La edición excede la longitud máxima de 50 caracteres.");
            }
        }

        if (!string.IsNullOrWhiteSpace(Descripcion))
        {
            if (ValidadorEntrada.ExcedeLongitud(Descripcion, 500))
            {
                ModelState.AddModelError("Descripcion", "La descripción excede la longitud máxima de 500 caracteres.");
            }
        }

        if (!ValidadorEntrada.ValidYear(AñoPublicacion))
        {
            ModelState.AddModelError("AñoPublicacion", "El año de publicación no es válido.");
        }

        if (!ModelState.IsValid)
        {
            CargarPagina();
            return Page();
        }

        var libro = _libroFactory.CreateForInsert(
            AutorId,
            Titulo,
            Editorial,
            Edicion,
            AñoPublicacion,
            Descripcion,
            Estado);

        _repository.InsertarLibro(libro);
        return Redirect("/Libro");
    }

    private void CargarPagina()
    {
        Autores = _repository.ObtenerAutoresActivos();
    }
}
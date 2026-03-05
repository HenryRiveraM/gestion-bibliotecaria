using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gestion_bibliotecaria.Services;
using gestion_bibliotecaria.Models;

namespace gestion_bibliotecaria.Pages;

public class LibroCreateModel : PageModel
{
    private readonly DatabaseService _databaseService;

    [BindProperty]
    public Libro Libro { get; set; } = new Libro { Estado = true };

    public List<Autor> Autores { get; set; } = new List<Autor>();

    public string ErrorMessage { get; set; } = string.Empty;
    public string SuccessMessage { get; set; } = string.Empty;

    public LibroCreateModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task OnGetAsync()
    {
        Autores = await _databaseService.ObtenerAutores();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Por favor completa todos los campos requeridos.";
            Autores = await _databaseService.ObtenerAutores();
            return Page();
        }

        try
        {
            var libroId = await _databaseService.InsertarLibro(Libro);
            SuccessMessage = $"Libro creado exitosamente con ID: {libroId}";
            
            return RedirectToPage("Libro");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al crear el libro: {ex.Message}";
            Autores = await _databaseService.ObtenerAutores();
            return Page();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gestion_bibliotecaria.Services;
using gestion_bibliotecaria.Models;

namespace gestion_bibliotecaria.Pages;

public class AutorCreateModel : PageModel
{
    private readonly DatabaseService _databaseService;

    [BindProperty]
    public Autor Autor { get; set; } = new Autor { Estado = true };

    public string ErrorMessage { get; set; } = string.Empty;
    public string SuccessMessage { get; set; } = string.Empty;

    public AutorCreateModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Por favor completa todos los campos requeridos.";
            return Page();
        }

        try
        {
            var autorId = await _databaseService.InsertarAutor(Autor);
            SuccessMessage = $"Autor creado exitosamente con ID: {autorId}";
            
            return RedirectToPage("Autor");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al crear el autor: {ex.Message}";
            return Page();
        }
    }
}

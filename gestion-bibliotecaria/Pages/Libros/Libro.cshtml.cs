using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Domain.Ports;
using gestion_bibliotecaria.Infrastructure.Creators;
using gestion_bibliotecaria.Infrastructure.Persistence;
using gestion_bibliotecaria.Domain.Validations;
using gestion_bibliotecaria.Infrastructure.Security;

namespace gestion_bibliotecaria.Pages;

public class LibroModel : PageModel
{
    private readonly RepositoryFactory<Libro, int> _libroRepositoryFactory;
    private readonly RouteTokenService _routeTokenService;

    public DataTable Libros { get; set; } = new DataTable();
    public Dictionary<int, string> AutoresNombres { get; set; } = new();
    public DataTable Autores { get; set; } = new();

    public LibroModel(
        RepositoryFactory<Libro, int> libroRepositoryFactory,
        RouteTokenService routeTokenService)
    {
        _libroRepositoryFactory = libroRepositoryFactory;
        _routeTokenService = routeTokenService;
    }

    public void OnGet()
    {
        var repository = _libroRepositoryFactory.CreateRepository();
        Libros = repository.GetAll();

        if (!Libros.Columns.Contains("LibroToken"))
        {
            Libros.Columns.Add("LibroToken", typeof(string));
        }

        foreach (DataRow row in Libros.Rows)
        {
            var libroId = row.Field<int>("LibroId");
            row["LibroToken"] = _routeTokenService.CrearToken(libroId);
        }

        if (repository is LibroRepository libroRepository)
        {
            AutoresNombres = libroRepository.ObtenerNombresAutores();
            Autores = libroRepository.ObtenerAutoresActivos();
        }
    }

    public IActionResult OnPostEliminar(string token)
    {
        if (!_routeTokenService.TryObtenerId(token, out var libroId))
        {
            return NotFound();
        }

        var repository = _libroRepositoryFactory.CreateRepository();

        repository.Delete(new Libro
        {
            LibroId = libroId
        });

        return RedirectToPage();
    }

    public IActionResult OnPostEditar(
        string token,
        int AutorId,
        string Titulo,
        string? ISBN,
        string? Editorial,
        string? Genero,
        string? Edicion,
        int? AñoPublicacion,
        int? NumeroPaginas,
        string? Idioma,
        string? PaisPublicacion,
        string? Descripcion,
        bool Estado)
    {
        if (!_routeTokenService.TryObtenerId(token, out var id))
        {
            return NotFound();
        }

        Titulo = ValidadorEntrada.NormalizarEspacios(Titulo);
        ISBN = ValidadorEntrada.NormalizarEspacios(ISBN);
        Editorial = ValidadorEntrada.NormalizarEspacios(Editorial);
        Genero = ValidadorEntrada.NormalizarEspacios(Genero);
        Edicion = ValidadorEntrada.NormalizarEspacios(Edicion);
        Idioma = ValidadorEntrada.NormalizarEspacios(Idioma);
        PaisPublicacion = ValidadorEntrada.NormalizarEspacios(PaisPublicacion);
        Descripcion = ValidadorEntrada.NormalizarEspacios(Descripcion);

        if (ValidadorEntrada.EstaVacio(Titulo))
            ModelState.AddModelError("Titulo", "El título es obligatorio.");
        else if (ValidadorEntrada.ExcedeLongitud(Titulo, 200))
            ModelState.AddModelError("Titulo", "El título excede la longitud máxima de 200 caracteres.");

        if (!string.IsNullOrWhiteSpace(ISBN) && ValidadorEntrada.ExcedeLongitud(ISBN, 20))
            ModelState.AddModelError("ISBN", "El ISBN excede la longitud máxima de 20 caracteres.");

        if (!ValidadorEntrada.ISBNValido(ISBN))
            ModelState.AddModelError("ISBN", "El ISBN debe contener 10 o 13 dígitos.");

        if (!string.IsNullOrWhiteSpace(Editorial) && ValidadorEntrada.ExcedeLongitud(Editorial, 100))
            ModelState.AddModelError("Editorial", "La editorial excede la longitud máxima de 100 caracteres.");

        if (!string.IsNullOrWhiteSpace(Genero) && ValidadorEntrada.ExcedeLongitud(Genero, 100))
            ModelState.AddModelError("Genero", "El género excede la longitud máxima de 100 caracteres.");

        if (!string.IsNullOrWhiteSpace(Edicion) && ValidadorEntrada.ExcedeLongitud(Edicion, 50))
            ModelState.AddModelError("Edicion", "La edición excede la longitud máxima de 50 caracteres.");

        if (NumeroPaginas.HasValue && NumeroPaginas <= 0)
            ModelState.AddModelError("NumeroPaginas", "El número de páginas debe ser mayor a 0.");

        if (!ValidadorEntrada.ValidYear(AñoPublicacion))
            ModelState.AddModelError("AñoPublicacion", "El año de publicación no es válido.");

        if (!string.IsNullOrWhiteSpace(Idioma) && ValidadorEntrada.ExcedeLongitud(Idioma, 50))
            ModelState.AddModelError("Idioma", "El idioma excede la longitud máxima de 50 caracteres.");

        if (!ValidadorEntrada.IdiomaPermitido(Idioma))
            ModelState.AddModelError("Idioma", "Seleccione un idioma válido.");

        if (!string.IsNullOrWhiteSpace(PaisPublicacion) && ValidadorEntrada.ExcedeLongitud(PaisPublicacion, 100))
            ModelState.AddModelError("PaisPublicacion", "El país de publicación excede la longitud máxima de 100 caracteres.");

        if (!string.IsNullOrWhiteSpace(Descripcion) && ValidadorEntrada.ExcedeLongitud(Descripcion, 500))
            ModelState.AddModelError("Descripcion", "La descripción excede la longitud máxima de 500 caracteres.");

        if (!EsAutorActivo(AutorId))
            ModelState.AddModelError("AutorId", "El autor seleccionado está inactivo o no existe.");

        if (!ModelState.IsValid)
        {
            OnGet();
            return Page();
        }

        var repository = _libroRepositoryFactory.CreateRepository();

        var libro = new Libro
        {
            LibroId = id,
            AutorId = AutorId,
            Titulo = Titulo,
            ISBN = ISBN,
            Editorial = Editorial,
            Genero = Genero,
            Edicion = Edicion,
            AñoPublicacion = AñoPublicacion,
            NumeroPaginas = NumeroPaginas,
            Idioma = Idioma,
            PaisPublicacion = PaisPublicacion,
            Descripcion = Descripcion,
            Estado = Estado,
            FechaRegistro = DateTime.Now,
            UltimaActualizacion = DateTime.Now
        };

        repository.Update(libro);

        return RedirectToPage();
    }

    public IActionResult OnPostCrear(
        string? NombreAutorNuevo,
        string Titulo,
        string? ISBN,
        string? Editorial,
        string? Genero,
        string? Edicion,
        int? AñoPublicacion,
        int? NumeroPaginas,
        string? Idioma,
        string? PaisPublicacion,
        string? Descripcion)
    {
        ModelState.Remove("AutorId");

        int AutorId = 0;
        if (int.TryParse(Request.Form["AutorId"], out var parsedId))
        {
            AutorId = parsedId;
        }

        Titulo = ValidadorEntrada.NormalizarEspacios(Titulo);
        ISBN = ValidadorEntrada.NormalizarEspacios(ISBN);
        Editorial = ValidadorEntrada.NormalizarEspacios(Editorial);
        Genero = ValidadorEntrada.NormalizarEspacios(Genero);
        Edicion = ValidadorEntrada.NormalizarEspacios(Edicion);
        Idioma = ValidadorEntrada.NormalizarEspacios(Idioma);
        PaisPublicacion = ValidadorEntrada.NormalizarEspacios(PaisPublicacion);
        Descripcion = ValidadorEntrada.NormalizarEspacios(Descripcion);
        NombreAutorNuevo = ValidadorEntrada.NormalizarEspacios(NombreAutorNuevo);

        if (ValidadorEntrada.EstaVacio(Titulo))
            ModelState.AddModelError("Titulo", "El título es obligatorio.");
        else if (ValidadorEntrada.ExcedeLongitud(Titulo, 200))
            ModelState.AddModelError("Titulo", "El título excede la longitud máxima de 200 caracteres.");

        if (!string.IsNullOrWhiteSpace(ISBN) && ValidadorEntrada.ExcedeLongitud(ISBN, 20))
            ModelState.AddModelError("ISBN", "El ISBN excede la longitud máxima de 20 caracteres.");

        if (!ValidadorEntrada.ISBNValido(ISBN))
            ModelState.AddModelError("ISBN", "El ISBN debe contener 10 o 13 dígitos.");

        if (!string.IsNullOrWhiteSpace(Editorial) && ValidadorEntrada.ExcedeLongitud(Editorial, 100))
            ModelState.AddModelError("Editorial", "La editorial excede la longitud máxima de 100 caracteres.");

        if (!string.IsNullOrWhiteSpace(Genero) && ValidadorEntrada.ExcedeLongitud(Genero, 100))
            ModelState.AddModelError("Genero", "El género excede la longitud máxima de 100 caracteres.");

        if (!string.IsNullOrWhiteSpace(Edicion) && ValidadorEntrada.ExcedeLongitud(Edicion, 50))
            ModelState.AddModelError("Edicion", "La edición excede la longitud máxima de 50 caracteres.");

        if (NumeroPaginas.HasValue && NumeroPaginas <= 0)
            ModelState.AddModelError("NumeroPaginas", "El número de páginas debe ser mayor a 0.");

        if (!ValidadorEntrada.ValidYear(AñoPublicacion))
            ModelState.AddModelError("AñoPublicacion", "El año de publicación no es válido.");

        if (!string.IsNullOrWhiteSpace(Idioma) && ValidadorEntrada.ExcedeLongitud(Idioma, 50))
            ModelState.AddModelError("Idioma", "El idioma excede la longitud máxima de 50 caracteres.");

        if (!ValidadorEntrada.IdiomaPermitido(Idioma))
            ModelState.AddModelError("Idioma", "Seleccione un idioma válido.");

        if (!string.IsNullOrWhiteSpace(PaisPublicacion) && ValidadorEntrada.ExcedeLongitud(PaisPublicacion, 100))
            ModelState.AddModelError("PaisPublicacion", "El país de publicación excede la longitud máxima de 100 caracteres.");

        if (!string.IsNullOrWhiteSpace(Descripcion) && ValidadorEntrada.ExcedeLongitud(Descripcion, 500))
            ModelState.AddModelError("Descripcion", "La descripción excede la longitud máxima de 500 caracteres.");

        if (AutorId == 0 && string.IsNullOrWhiteSpace(NombreAutorNuevo))
            ModelState.AddModelError("AutorId", "Seleccione un autor o escriba el nombre de uno nuevo.");
        else if (AutorId != 0 && !EsAutorActivo(AutorId))
            ModelState.AddModelError("AutorId", "El autor seleccionado está inactivo o no existe.");

        if (!ModelState.IsValid)
        {
            OnGet();
            return Page();
        }

        if (AutorId == 0 && !string.IsNullOrWhiteSpace(NombreAutorNuevo))
        {
            var repo = _libroRepositoryFactory.CreateRepository();
            if (repo is LibroRepository lr)
            {
                AutorId = lr.InsertarAutorYObtenerID(NombreAutorNuevo);
            }
        }

        var libro = new Libro
        {
            AutorId = AutorId,
            Titulo = Titulo,
            ISBN = ISBN,
            Editorial = Editorial,
            Genero = Genero,
            Edicion = Edicion,
            AñoPublicacion = AñoPublicacion,
            NumeroPaginas = NumeroPaginas,
            Idioma = Idioma,
            PaisPublicacion = PaisPublicacion,
            Descripcion = Descripcion,
            Estado = true,
            FechaRegistro = DateTime.Now
        };

        var repository = _libroRepositoryFactory.CreateRepository();
        repository.Insert(libro);

        return RedirectToPage();
    }

    private bool EsAutorActivo(int autorId)
    {
        var repository = _libroRepositoryFactory.CreateRepository();
        return repository is LibroRepository libroRepository && libroRepository.ExisteAutorActivo(autorId);
    }
}
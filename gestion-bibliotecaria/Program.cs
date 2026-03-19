using gestion_bibliotecaria.FactoryCreators;
using gestion_bibliotecaria.FactoryProducts;
using gestion_bibliotecaria.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<gestion_bibliotecaria.Security.RouteTokenService>();


// Factory Method ya usado por Autor
builder.Services.AddScoped<RepositoryFactory<Autor, int>, AutorRepositoryCreator>();

// Factory Method agregado para Libro
builder.Services.AddScoped<RepositoryFactory<Libro, int>, LibroRepositoryCreator>();

// Factory Method ya usado por Ejemplar
builder.Services.AddScoped<RepositoryFactory<Ejemplar, int>, EjemplarRepositoryCreator>();

builder.Services.AddRazorPages(options =>
{
    options.RootDirectory = "/";

    options.Conventions.AddPageRoute("/Pages/Index", "");
    options.Conventions.AddPageRoute("/Pages/Index", "Index");
    options.Conventions.AddPageRoute("/Pages/Privacy", "Privacy");
    options.Conventions.AddPageRoute("/Pages/Error", "Error");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

app.Run();
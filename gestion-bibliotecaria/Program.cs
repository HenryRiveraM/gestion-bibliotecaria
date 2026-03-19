using gestion_bibliotecaria.FactoryCreators;
using gestion_bibliotecaria.FactoryProducts;
using gestion_bibliotecaria.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<gestion_bibliotecaria.Security.RouteTokenService>();

// Se mantiene Ejemplar para no generar conflictos con trabajo de otro dev
builder.Services.AddScoped<IEjemplarFactory, EjemplarFactory>();

// Factory Method ya usado por Autor
builder.Services.AddScoped<RepositoryFactory<Autor, int>, AutorRepositoryCreator>();

// Factory Method agregado para Libro
builder.Services.AddScoped<RepositoryFactory<Libro, int>, LibroRepositoryCreator>();

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
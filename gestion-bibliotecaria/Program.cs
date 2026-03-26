using gestion_bibliotecaria.FactoryCreators;
using gestion_bibliotecaria.FactoryProducts;
using gestion_bibliotecaria.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<gestion_bibliotecaria.Security.RouteTokenService>();

builder.Services.AddSingleton<RepositoryFactory<Autor, int>, AutorRepositoryCreator>();
builder.Services.AddSingleton<RepositoryFactory<Libro, int>, LibroRepositoryCreator>();
builder.Services.AddSingleton<RepositoryFactory<Ejemplar, int>, EjemplarRepositoryCreator>();

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
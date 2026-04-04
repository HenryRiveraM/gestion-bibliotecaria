using gestion_bibliotecaria.Infrastructure.Creators;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Infrastructure.Security;
using gestion_bibliotecaria.Aplicacion.Servicios;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<RouteTokenService>();

builder.Services.AddScoped<LibroService>();

builder.Services.AddScoped<RepositoryFactory<Autor, int>, AutorRepositoryCreator>();
builder.Services.AddScoped<RepositoryFactory<Libro, int>, LibroRepositoryCreator>();
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
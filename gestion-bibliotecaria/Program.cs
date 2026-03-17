using gestion_bibliotecaria.FactoryCreators;
using gestion_bibliotecaria.FactoryProducts;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddScoped<gestion_bibliotecaria.Security.RouteTokenService>();
builder.Services.AddScoped<ILibroFactory, LibroFactory>();

// Registro de Factory Method para Autor impelmtar using para <gestion_bibliotecaria.FactoryCreators.
builder.Services.AddScoped<gestion_bibliotecaria.FactoryCreators.RepositoryFactory<gestion_bibliotecaria.Models.Autor>, gestion_bibliotecaria.FactoryCreators.AutorRepositoryCreator>();

builder.Services.AddRazorPages(options =>
{
    // Keep Ejemplar pages under Services without moving them back to Pages.
    options.RootDirectory = "/";

    // Preserve friendly URLs for default pages under /Pages.
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

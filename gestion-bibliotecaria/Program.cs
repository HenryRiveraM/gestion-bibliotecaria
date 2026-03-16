using gestion_bibliotecaria.FactoryCreators;
using gestion_bibliotecaria.FactoryProducts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection();
builder.Services.AddScoped<gestion_bibliotecaria.Security.RouteTokenService>();
builder.Services.AddScoped<ILibroFactory, LibroFactory>();
builder.Services.AddScoped<IEjemplarFactory, EjemplarFactory>();

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

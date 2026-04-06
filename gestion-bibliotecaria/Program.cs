using gestion_bibliotecaria.Aplicacion.Servicios;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Domain.Ports;
using gestion_bibliotecaria.Infrastructure.Creators;
using gestion_bibliotecaria.Infrastructure.Persistence;
using gestion_bibliotecaria.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddScoped<RouteTokenService>();

builder.Services.AddScoped<IAutorRepositorio>(sp => new AutorRepository(connectionString));
builder.Services.AddSingleton<ILibroRepositorio>(new LibroRepository(connectionString));
builder.Services.AddSingleton<IEjemplarRepositorio>(new EjemplarRepository(connectionString));
builder.Services.AddScoped<IAutorServicio, AutorServicio>();
builder.Services.AddScoped<LibroServicio>();
builder.Services.AddScoped<IEjemplarServicio, EjemplarServicio>();
// builder.Services.AddScoped<AutorService>(); 
// builder.Services.AddScoped<EjemplarService>();



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
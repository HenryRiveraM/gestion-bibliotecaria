using gestion_bibliotecaria.Aplicacion.Servicios;
using gestion_bibliotecaria.Aplicacion.Interfaces;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Domain.Ports;
using gestion_bibliotecaria.Infrastructure.Creators;
using gestion_bibliotecaria.Infrastructure.Persistence;
using gestion_bibliotecaria.Infrastructure.Security;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddScoped<RouteTokenService>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<IAutorRepositorio>(sp => new AutorRepository(connectionString));
builder.Services.AddSingleton<ILibroRepositorio>(new LibroRepository(connectionString));
builder.Services.AddSingleton<IEjemplarRepositorio>(new EjemplarRepository(connectionString));
builder.Services.AddScoped<IUsuarioRepositorio>(sp => new UsuarioRepository(connectionString));
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

app.UseSession();

app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    var isPublicPath =
        path.StartsWithSegments("/Login", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWithSegments("/Error", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWithSegments("/Privacy", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWithSegments("/css", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWithSegments("/js", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWithSegments("/lib", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWithSegments("/assets", StringComparison.OrdinalIgnoreCase) ||
        path.Equals("/favicon.ico", StringComparison.OrdinalIgnoreCase) ||
        path.Equals("/gestion_bibliotecaria.styles.css", StringComparison.OrdinalIgnoreCase);

    if (isPublicPath)
    {
        await next();
        return;
    }

    var usuarioSesion = context.Session.GetString(SessionKeys.UsuarioId);

    if (string.IsNullOrWhiteSpace(usuarioSesion))
    {
        context.Response.Redirect("/Login");
        return;
    }

    await next();
});

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

app.Run();

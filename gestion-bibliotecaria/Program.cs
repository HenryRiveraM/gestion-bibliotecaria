using gestion_bibliotecaria.Aplicacion.Servicios;
using gestion_bibliotecaria.Aplicacion.Interfaces;
using gestion_bibliotecaria.Domain.Ports;
using gestion_bibliotecaria.Infrastructure.Configuration;
using gestion_bibliotecaria.Infrastructure.Email;
using gestion_bibliotecaria.Infrastructure.Persistence;
using gestion_bibliotecaria.Infrastructure.Security;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Inicializamos el Singleton Gestor de Base de Datos
ConfigurationSingleton.Initialize(builder.Configuration);

builder.Services.AddScoped<RouteTokenService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(EmailSettings.SectionName));
builder.Services.AddHttpClient();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<IAutorRepositorio>(sp => new AutorRepository());
builder.Services.AddSingleton<ILibroRepositorio>(new LibroRepository());
builder.Services.AddSingleton<IEjemplarRepositorio>(new EjemplarRepository());
builder.Services.AddScoped<IUsuarioRepositorio>(sp => new UsuarioRepository());
builder.Services.AddScoped<IEmailSender>(EmailSenderFactory.Create);
builder.Services.AddScoped<IAutorServicio, AutorServicio>();
builder.Services.AddScoped<ILibroServicio, LibroServicio>();
builder.Services.AddScoped<IEjemplarServicio, EjemplarServicio>();
builder.Services.AddScoped<IPrestamoRepositorio>(sp => new PrestamoRepository(connectionString));
builder.Services.AddScoped<IPrestamoServicio, PrestamoServicio>();
builder.Services.AddScoped<gestion_bibliotecaria.Aplicacion.Fachadas.IPrestamoFachada, gestion_bibliotecaria.Aplicacion.Fachadas.PrestamoFachada>();
builder.Services.AddScoped<IUserCredentialProvisioningService, UserCredentialProvisioningService>();
builder.Services.AddScoped<IUsuarioServicio, UsuarioServicio>();

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
        path.Equals("/", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWithSegments("/Index", StringComparison.OrdinalIgnoreCase) ||
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

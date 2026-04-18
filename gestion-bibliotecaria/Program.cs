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

    // Mejora de seguridad para la cookie de sesión
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddScoped<IAutorRepositorio>(sp => new AutorRepository());
builder.Services.AddSingleton<ILibroRepositorio>(new LibroRepository());
builder.Services.AddSingleton<IEjemplarRepositorio>(new EjemplarRepository());
builder.Services.AddScoped<IUsuarioRepositorio>(sp => new UsuarioRepository());

builder.Services.AddScoped<IEmailSender>(EmailSenderFactory.Create);

builder.Services.AddScoped<IAutorServicio, AutorServicio>();
builder.Services.AddScoped<ILibroServicio, LibroServicio>();
builder.Services.AddScoped<IEjemplarServicio, EjemplarServicio>();

builder.Services.AddScoped<IPrestamoRepositorio>(sp => new PrestamoRepository());
builder.Services.AddScoped<IPrestamoServicio, PrestamoServicio>();

builder.Services.AddScoped<
    gestion_bibliotecaria.Aplicacion.Fachadas.IPrestamoFachada,
    gestion_bibliotecaria.Aplicacion.Fachadas.PrestamoFachada>();

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

static bool IsPublicPath(PathString path)
{
    return
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
}

/*
    Middleware anti-caché.
    Evita que páginas privadas queden visibles al usar la flecha atrás
    después de cerrar sesión.
*/
app.Use(async (context, next) =>
{
    var path = context.Request.Path;

    if (!IsPublicPath(path))
    {
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
            context.Response.Headers["Pragma"] = "no-cache";
            context.Response.Headers["Expires"] = "0";

            return Task.CompletedTask;
        });
    }

    await next();
});

/*
    Middleware de control de acceso.
    Las rutas públicas pueden verse sin sesión.
    Las rutas privadas requieren sesión activa.
*/
app.Use(async (context, next) =>
{
    var path = context.Request.Path;

    if (IsPublicPath(path))
    {
        await next();
        return;
    }

    var usuarioSesion = context.Session.GetString(SessionKeys.UsuarioId);

    if (string.IsNullOrWhiteSpace(usuarioSesion))
    {
        context.Response.Redirect("/");
        return;
    }

    await next();
});

app.UseAuthorization();

app.MapStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

app.Run();
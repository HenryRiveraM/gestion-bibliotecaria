var builder = WebApplication.CreateBuilder(args);

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

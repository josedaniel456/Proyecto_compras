using Microsoft.EntityFrameworkCore;
using Proyecto_compras.Data;

var builder = WebApplication.CreateBuilder(args);

// ✅ Agregar controladores y vistas
builder.Services.AddControllersWithViews();

// ✅ Registrar tu clase de conexión
builder.Services.AddSingleton<ConexionBD>();

// ✅ Agregar servicios para manejo de sesión
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de expiración
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ Agregar acceso a HttpContext (por si lo necesitas luego)
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ✅ Configurar el middleware de sesión ANTES de UseAuthorization
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 🔹 Activar sesiones
app.UseSession();

app.UseAuthorization();

// ✅ Configurar la ruta por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://*:{port}");


app.Run();

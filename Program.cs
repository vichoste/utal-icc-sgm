using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;
using Utal.Icc.Sgm.Seeders;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Environment.IsDevelopment() ? builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("No se encuentra el string de conexión hacia la base de datos.") : Environment.GetEnvironmentVariable("CONNECTION_STRING_DEFAULT_CONNECTION");
var rootEmail = builder.Environment.IsDevelopment() ? builder.Configuration["RootEmail"] ?? throw new InvalidOperationException("No se encuentra el string del email del administrador.") : Environment.GetEnvironmentVariable("ROOT_EMAIL");
var rootPassword = builder.Environment.IsDevelopment() ? builder.Configuration["RootPassword"] ?? throw new InvalidOperationException("No se encuentra el string de la contraseña del administrador.") : Environment.GetEnvironmentVariable("ROOT_PASSWORD");
var rootFirstName = builder.Environment.IsDevelopment() ? builder.Configuration["RootFirstName"] ?? throw new InvalidOperationException("No se encuentra el string del nombre del administrador.") : Environment.GetEnvironmentVariable("ROOT_FIRST_NAME");
var rootLastName = builder.Environment.IsDevelopment() ? builder.Configuration["RootLastName"] ?? throw new InvalidOperationException("No se encuentra el string del apellido del administrador.") : Environment.GetEnvironmentVariable("ROOT_LAST_NAME");
var rootRut = builder.Environment.IsDevelopment() ? builder.Configuration["RootRut"] ?? throw new InvalidOperationException("No se encuentra el string del RUT del administrador.") : Environment.GetEnvironmentVariable("ROOT_RUT");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

var app = builder.Build();
if (app.Environment.IsDevelopment()) {
	_ = app.UseMigrationsEndPoint();
} else {
	_ = app.UseExceptionHandler("/Home/Error");
	_ = app.UseHsts();
}
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var context = services.GetRequiredService<ApplicationDbContext>();
var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
await StartupSeeder.SeedRolesAsync(roleManager);
await StartupSeeder.SeedAdministratorAsync(rootEmail!, rootPassword!, rootFirstName!, rootLastName!, rootRut!, userManager);
_ = app.UseHttpsRedirection();
_ = app.UseStaticFiles();
_ = app.UseRouting();
_ = app.UseAuthentication();
_ = app.UseAuthorization();
_ = app.MapAreaControllerRoute(
	name: "Account",
	areaName: "Account",
	pattern: "Account/{controller=SignIn}/{action=Index}/{id?}"
);
_ = app.MapAreaControllerRoute(
	name: "Administrator",
	areaName: "Administrator",
	pattern: "Administrator/{controller=Account}/{action=Index}/{id?}"
);
_ = app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}"
);
_ = app.MapRazorPages();
app.Run();
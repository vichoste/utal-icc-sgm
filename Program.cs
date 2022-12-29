using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;
using Utal.Icc.Sgm.Seeders;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Environment.IsDevelopment() ? builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("No se encuentra el string de conexión hacia la base de datos.") : Environment.GetEnvironmentVariable("CONNECTION_STRING_DEFAULT_CONNECTION");
var rootEmail = builder.Environment.IsDevelopment() ? builder.Configuration["RootEmail"] ?? throw new InvalidOperationException("No se encuentra el string del email del administrador.") : Environment.GetEnvironmentVariable("ROOT_EMAIL");
var rootPassword = builder.Environment.IsDevelopment() ? builder.Configuration["RootPassword"] ?? throw new InvalidOperationException("No se encuentra el string de la contraseña del administrador.") : Environment.GetEnvironmentVariable("ROOT_PASSWORD");
var rootFirstName = builder.Environment.IsDevelopment() ? builder.Configuration["RootFirstName"] ?? throw new InvalidOperationException("No se encuentra el string del nombre del administrador.") : Environment.GetEnvironmentVariable("ROOT_FIRST_NAME");
var rootLastName = builder.Environment.IsDevelopment() ? builder.Configuration["RootLastName"] ?? throw new InvalidOperationException("No se encuentra el string del apellido del administrador.") : Environment.GetEnvironmentVariable("ROOT_LAST_NAME");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
_ = builder.Environment.IsDevelopment() ?
	builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
		options.Lockout.DefaultLockoutTimeSpan = TimeSpan.Zero;
		options.Password.RequireDigit = false;
		options.Password.RequiredLength = 0;
		options.Password.RequiredUniqueChars = 0;
		options.Password.RequireNonAlphanumeric = false;
		options.Password.RequireUppercase = false;
		options.Password.RequireLowercase = false;
	}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultUI().AddDefaultTokenProviders()
	: builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultUI().AddDefaultTokenProviders();
builder.Services.AddControllersWithViews().AddNewtonsoftJson();

var app = builder.Build();
if (app.Environment.IsDevelopment()) {
	_ = app.UseMigrationsEndPoint();
} else {
	_ = app.UseExceptionHandler("/Home/Error");
	_ = app.UseHsts();
}
var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try {
	var context = services.GetRequiredService<ApplicationDbContext>();
	var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
	var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
	await StartupSeeder.SeedRolesAsync(roleManager);
	await StartupSeeder.SeedAdministratorAsync(rootEmail!, rootPassword!, rootFirstName!, rootLastName!, userManager);
} catch {
	var logger = loggerFactory.CreateLogger<Program>();
	logger.LogError("Error al poblar la base de datos con roles.");
}
_ = app.UseHttpsRedirection();
_ = app.UseStaticFiles();
_ = app.UseRouting();
_ = app.UseAuthentication();
_ = app.UseAuthorization();
_ = app.MapAreaControllerRoute(
	name: "role",
	areaName: "Role",
	pattern: "Role/{controller=Playground}/{action=Index}/{id?}"
);
_ = app.MapControllerRoute(
	name: "areas",
	pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);
_ = app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
_ = app.MapRazorPages();
_ = app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);
app.Run();
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;
using Utal.Icc.Sgm.Seeders;

var builder = WebApplication.CreateBuilder(args);
var defaultConnection = builder.Environment.IsDevelopment() ? builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("No se encuentra el string de conexión hacia la base de datos.") : Environment.GetEnvironmentVariable("DEFAULT_CONNECTION");
var directorTeacherEmail = builder.Environment.IsDevelopment() ? builder.Configuration["DirectorTeacherEmail"] ?? throw new InvalidOperationException("No se encuentra el string del email del director de carrera.") : Environment.GetEnvironmentVariable("DIRECTOR_TEACHER_EMAIL");
var directorTeacherPassword = builder.Environment.IsDevelopment() ? builder.Configuration["DirectorTeacherPassword"] ?? throw new InvalidOperationException("No se encuentra el string de la contraseña del director de carrera.") : Environment.GetEnvironmentVariable("DIRECTOR_TEACHER_PASSWORD");
var directorTeacherFirstName = builder.Environment.IsDevelopment() ? builder.Configuration["DirectorTeacherFirstName"] ?? throw new InvalidOperationException("No se encuentra el string del nombre del director de carrera.") : Environment.GetEnvironmentVariable("DIRECTOR_TEACHER_FIRST_NAME");
var directorTeacherLastName = builder.Environment.IsDevelopment() ? builder.Configuration["DirectorTeacherLastName"] ?? throw new InvalidOperationException("No se encuentra el string del apellido del director de carrera.") : Environment.GetEnvironmentVariable("DIRECTOR_TEACHER_LAST_NAME");
var directorTeacherRut = builder.Environment.IsDevelopment() ? builder.Configuration["DirectorTeacherRut"] ?? throw new InvalidOperationException("No se encuentra el string del RUT del director de carrera.") : Environment.GetEnvironmentVariable("DIRECTOR_TEACHER_RUT");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(defaultConnection));
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
var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
var userStore = services.GetRequiredService<IUserStore<ApplicationUser>>();
var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
await StartupSeeder.SeedRolesAsync(roleManager);
await StartupSeeder.SeedDirectorTeacherAsync(directorTeacherEmail!, directorTeacherPassword!, directorTeacherFirstName!, directorTeacherLastName!, directorTeacherRut!, userManager, userStore);
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
	name: "DirectorTeacher",
	areaName: "DirectorTeacher",
	pattern: "DirectorTeacher/{controller=Student}/{action=Index}/{id?}"
);
_ = app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}"
);
_ = app.MapRazorPages();
app.Run();
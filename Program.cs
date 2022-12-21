using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Utal.Icc.Sgm.Data;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Environment.IsDevelopment() ? builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.") : Environment.GetEnvironmentVariable("CONNECTION_STRING_DEFAULT_CONNECTION");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
_ = builder.Environment.IsDevelopment() ?
	builder.Services.AddDefaultIdentity<IdentityUser>(options => {
		options.Lockout.DefaultLockoutTimeSpan = TimeSpan.Zero;
		options.Password.RequireDigit = false;
		options.Password.RequiredLength = 0;
		options.Password.RequiredUniqueChars = 0;
		options.Password.RequireNonAlphanumeric = false;
		options.Password.RequireUppercase = false;
		options.Password.RequireLowercase = false;
	}).AddEntityFrameworkStores<ApplicationDbContext>()
	: builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews().AddNewtonsoftJson();

var app = builder.Build();
if (app.Environment.IsDevelopment()) {
	_ = app.UseMigrationsEndPoint();
} else {
	_ = app.UseExceptionHandler("/Home/Error");
	_ = app.UseHsts();
}
_ = app.UseHttpsRedirection();
_ = app.UseStaticFiles();
_ = app.UseRouting();
_ = app.UseAuthorization();
_ = app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
_ = app.MapRazorPages();
_ = app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);
app.Run();
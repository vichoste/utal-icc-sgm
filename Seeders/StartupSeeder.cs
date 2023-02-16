using Microsoft.AspNetCore.Identity;

using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Seeders;

public static class StartupSeeder {
	public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager) {
		if (!await roleManager.RoleExistsAsync(nameof(Role.Director))) {
			_ = await roleManager.CreateAsync(new IdentityRole(nameof(Role.Director)));
		}
		if (!await roleManager.RoleExistsAsync(nameof(Role.Committee))) {
			_ = await roleManager.CreateAsync(new IdentityRole(nameof(Role.Committee)));
		}
		if (!await roleManager.RoleExistsAsync(nameof(Role.Course))) {
			_ = await roleManager.CreateAsync(new IdentityRole(nameof(Role.Course)));
		}
		if (!await roleManager.RoleExistsAsync(nameof(Role.Guide))) {
			_ = await roleManager.CreateAsync(new IdentityRole(nameof(Role.Guide)));
		}
		if (!await roleManager.RoleExistsAsync(nameof(Role.Assistant))) {
			_ = await roleManager.CreateAsync(new IdentityRole(nameof(Role.Assistant)));
		}
		if (!await roleManager.RoleExistsAsync(nameof(Role.Teacher))) {
			_ = await roleManager.CreateAsync(new IdentityRole(nameof(Role.Teacher)));
		}
		if (!await roleManager.RoleExistsAsync(nameof(Role.Engineer))) {
			_ = await roleManager.CreateAsync(new IdentityRole(nameof(Role.Engineer)));
		}
		if (!await roleManager.RoleExistsAsync(nameof(Role.Completed))) {
			_ = await roleManager.CreateAsync(new IdentityRole(nameof(Role.Completed)));
		}
		if (!await roleManager.RoleExistsAsync(nameof(Role.Qualification))) {
			_ = await roleManager.CreateAsync(new IdentityRole(nameof(Role.Qualification)));
		}
		if (!await roleManager.RoleExistsAsync(nameof(Role.Candidate))) {
			_ = await roleManager.CreateAsync(new IdentityRole(nameof(Role.Candidate)));
		}
		if (!await roleManager.RoleExistsAsync(nameof(Role.Student))) {
			_ = await roleManager.CreateAsync(new IdentityRole(nameof(Role.Student)));
		}
	}

	public static async Task SeedDirectorTeacherAsync(string email, string password, string firstName, string lastName, string rut, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore) {
		var emailStore = (IUserEmailStore<ApplicationUser>)userStore;
		var directorTeacher = new ApplicationUser {
			FirstName = firstName,
			LastName = lastName,
			Rut = rut,
			CreatedAt = DateTimeOffset.Now,
			UpdatedAt = DateTimeOffset.Now
		};
		var check = await userManager.FindByIdAsync(email);
		if (check is null) {
			await userStore.SetUserNameAsync(directorTeacher, email, CancellationToken.None);
			await emailStore.SetEmailAsync(directorTeacher, email, CancellationToken.None);
			_ = await userManager.CreateAsync(directorTeacher, password);
			_ = await userManager.AddToRolesAsync(directorTeacher, new List<string> { nameof(Role.Teacher), nameof(Role.Director) }.AsEnumerable());
		}
	}
}
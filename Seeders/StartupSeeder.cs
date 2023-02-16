using Microsoft.AspNetCore.Identity;

using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Seeders;

public static class StartupSeeder {
	public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager) {
		if (!await roleManager.RoleExistsAsync("Director")) {
			_ = await roleManager.CreateAsync(new IdentityRole("Director"));
		}
		if (!await roleManager.RoleExistsAsync("Committee")) {
			_ = await roleManager.CreateAsync(new IdentityRole("Committee"));
		}
		if (!await roleManager.RoleExistsAsync("Course")) {
			_ = await roleManager.CreateAsync(new IdentityRole("Course"));
		}
		if (!await roleManager.RoleExistsAsync("Guide")) {
			_ = await roleManager.CreateAsync(new IdentityRole("Guide"));
		}
		if (!await roleManager.RoleExistsAsync("Assistant")) {
			_ = await roleManager.CreateAsync(new IdentityRole("Assistant"));
		}
		if (!await roleManager.RoleExistsAsync("Teacher")) {
			_ = await roleManager.CreateAsync(new IdentityRole("Teacher"));
		}
		if (!await roleManager.RoleExistsAsync("Engineer")) {
			_ = await roleManager.CreateAsync(new IdentityRole("Engineer"));
		}
		if (!await roleManager.RoleExistsAsync("Completed")) {
			_ = await roleManager.CreateAsync(new IdentityRole("Completed"));
		}
		if (!await roleManager.RoleExistsAsync("Memoir")) {
			_ = await roleManager.CreateAsync(new IdentityRole("Memoir"));
		}
		if (!await roleManager.RoleExistsAsync("Candidate")) {
			_ = await roleManager.CreateAsync(new IdentityRole("Candidate"));
		}
		if (!await roleManager.RoleExistsAsync("Student")) {
			_ = await roleManager.CreateAsync(new IdentityRole("Student"));
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
			_ = await userManager.AddToRolesAsync(directorTeacher, new List<string> { "Teacher", "Director" }.AsEnumerable());
		}
	}
}
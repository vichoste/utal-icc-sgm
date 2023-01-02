using Microsoft.AspNetCore.Identity;

using Utal.Icc.Sgm.Areas.Account.Models;

namespace Utal.Icc.Sgm.Seeders;

public static class StartupSeeder {
	public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager) {
		_ = await roleManager.CreateAsync(new IdentityRole(Roles.Administrator.ToString()));
		_ = await roleManager.CreateAsync(new IdentityRole(Roles.DirectorTeacher.ToString()));
		_ = await roleManager.CreateAsync(new IdentityRole(Roles.CommitteeTeacher.ToString()));
		_ = await roleManager.CreateAsync(new IdentityRole(Roles.CourseTeacher.ToString()));
		_ = await roleManager.CreateAsync(new IdentityRole(Roles.MainTeacher.ToString()));
		_ = await roleManager.CreateAsync(new IdentityRole(Roles.AssistantTeacher.ToString()));
		_ = await roleManager.CreateAsync(new IdentityRole(Roles.EngineerStudent.ToString()));
		_ = await roleManager.CreateAsync(new IdentityRole(Roles.FinishedStudent.ToString()));
		_ = await roleManager.CreateAsync(new IdentityRole(Roles.ThesisStudent.ToString()));
		_ = await roleManager.CreateAsync(new IdentityRole(Roles.RegularStudent.ToString()));
	}

	public static async Task SeedAdministratorAsync(string email, string password, string firstName, string lastName, UserManager<ApplicationUser> userManager) {
		var defaultUser = new ApplicationUser {
			UserName = email,
			Email = email,
			EmailConfirmed = true,
			FirstName = firstName,
			LastName = lastName
		};
		if (userManager.Users.All(u => u.Id != defaultUser.Id)) {
			var user = await userManager.FindByEmailAsync(email);
			if (user == null) {
				_ = await userManager.CreateAsync(defaultUser, password);
				_ = await userManager.AddToRoleAsync(defaultUser, Roles.Administrator.ToString());
			}
		}
	}
}
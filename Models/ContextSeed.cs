using Microsoft.AspNetCore.Identity;

namespace Utal.Icc.Sgm.Models;

public static class ContextSeed {
	public static async Task SeedRolesAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) {
		_ = await roleManager.CreateAsync(new IdentityRole(Roles.Administrator.ToString()));
		_ = await roleManager.CreateAsync(new IdentityRole(Roles.DirectorTeacher.ToString()));
		_ = await roleManager.CreateAsync(new IdentityRole(Roles.CommitteeTeacher.ToString()));
		_ = await roleManager.CreateAsync(new IdentityRole(Roles.CourseTeacher.ToString()));
		_ = await roleManager.CreateAsync(new IdentityRole(Roles.MainGuideTeacher.ToString()));
		_ = await roleManager.CreateAsync(new IdentityRole(Roles.AssistantGuideTeacher.ToString()));
		_ = await roleManager.CreateAsync(new IdentityRole(Roles.Teacher.ToString()));
		_ = await roleManager.CreateAsync(new IdentityRole(Roles.GraduatedStudent.ToString()));
		_ = await roleManager.CreateAsync(new IdentityRole(Roles.CompletedStudent.ToString()));
		_ = await roleManager.CreateAsync(new IdentityRole(Roles.ThesisStudent.ToString()));
		_ = await roleManager.CreateAsync(new IdentityRole(Roles.Student.ToString()));
	}
	public static async Task SeedAdministratorAsync(string email, string password, string firstName, string lastName, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) {
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
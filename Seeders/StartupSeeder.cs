using Microsoft.AspNetCore.Identity;

using Utal.Icc.Sgm.Models;

using static Utal.Icc.Sgm.Models.ApplicationUser;

namespace Utal.Icc.Sgm.Seeders;

public static class StartupSeeder {
	public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager) {
		if (!await roleManager.RoleExistsAsync(Roles.DirectorTeacher.ToString())) {
			_ = await roleManager.CreateAsync(new IdentityRole(Roles.DirectorTeacher.ToString()));
		}
		if (!await roleManager.RoleExistsAsync(Roles.CommitteeTeacher.ToString())) {
			_ = await roleManager.CreateAsync(new IdentityRole(Roles.CommitteeTeacher.ToString()));
		}
		if (!await roleManager.RoleExistsAsync(Roles.CourseTeacher.ToString())) {
			_ = await roleManager.CreateAsync(new IdentityRole(Roles.CourseTeacher.ToString()));
		}
		if (!await roleManager.RoleExistsAsync(Roles.GuideTeacher.ToString())) {
			_ = await roleManager.CreateAsync(new IdentityRole(Roles.GuideTeacher.ToString()));
		}
		if (!await roleManager.RoleExistsAsync(Roles.AssistantTeacher.ToString())) {
			_ = await roleManager.CreateAsync(new IdentityRole(Roles.AssistantTeacher.ToString()));
		}
		if (!await roleManager.RoleExistsAsync(Roles.Teacher.ToString())) {
			_ = await roleManager.CreateAsync(new IdentityRole(Roles.Teacher.ToString()));
		}
		if (!await roleManager.RoleExistsAsync(Roles.EngineerStudent.ToString())) {
			_ = await roleManager.CreateAsync(new IdentityRole(Roles.EngineerStudent.ToString()));
		}
		if (!await roleManager.RoleExistsAsync(Roles.CompletedStudent.ToString())) {
			_ = await roleManager.CreateAsync(new IdentityRole(Roles.CompletedStudent.ToString()));
		}
		if (!await roleManager.RoleExistsAsync(Roles.ThesisStudent.ToString())) {
			_ = await roleManager.CreateAsync(new IdentityRole(Roles.ThesisStudent.ToString()));
		}
		if (!await roleManager.RoleExistsAsync(Roles.Student.ToString())) {
			_ = await roleManager.CreateAsync(new IdentityRole(Roles.Student.ToString()));
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
		var directorTeacherCheck = await userManager.FindByIdAsync(email);
		if (directorTeacherCheck is null) {
			await userStore.SetUserNameAsync(directorTeacher, email, CancellationToken.None);
			await emailStore.SetEmailAsync(directorTeacher, email, CancellationToken.None);
			_ = await userManager.CreateAsync(directorTeacher, password);
			_ = await userManager.AddToRolesAsync(directorTeacher, new List<string> { Roles.Teacher.ToString(), Roles.DirectorTeacher.ToString() });
		}
	}
}
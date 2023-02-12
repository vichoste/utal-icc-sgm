using Microsoft.AspNetCore.Identity;

using Utal.Icc.Sgm.Models;

using static Utal.Icc.Sgm.Models.ApplicationUser;

namespace Utal.Icc.Sgm.Seeders;

public static class StartupSeeder {
	public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager) {
		if (!await roleManager.RoleExistsAsync(nameof(Roles.DirectorTeacher))) {
			_ = await roleManager.CreateAsync(new IdentityRole(nameof(Roles.DirectorTeacher)));
		}
		if (!await roleManager.RoleExistsAsync(nameof(Roles.CommitteeTeacher))) {
			_ = await roleManager.CreateAsync(new IdentityRole(nameof(Roles.CommitteeTeacher)));
		}
		if (!await roleManager.RoleExistsAsync(nameof(Roles.CourseTeacher))) {
			_ = await roleManager.CreateAsync(new IdentityRole(nameof(Roles.CourseTeacher)));
		}
		if (!await roleManager.RoleExistsAsync(nameof(Roles.GuideTeacher))) {
			_ = await roleManager.CreateAsync(new IdentityRole(nameof(Roles.GuideTeacher)));
		}
		if (!await roleManager.RoleExistsAsync(nameof(Roles.AssistantTeacher))) {
			_ = await roleManager.CreateAsync(new IdentityRole(nameof(Roles.AssistantTeacher)));
		}
		if (!await roleManager.RoleExistsAsync(nameof(Roles.Teacher))) {
			_ = await roleManager.CreateAsync(new IdentityRole(nameof(Roles.Teacher)));
		}
		if (!await roleManager.RoleExistsAsync(nameof(Roles.EngineerStudent))) {
			_ = await roleManager.CreateAsync(new IdentityRole(nameof(Roles.EngineerStudent)));
		}
		if (!await roleManager.RoleExistsAsync(nameof(Roles.CompletedStudent))) {
			_ = await roleManager.CreateAsync(new IdentityRole(nameof(Roles.CompletedStudent)));
		}
		if (!await roleManager.RoleExistsAsync(nameof(Roles.ThesisStudent))) {
			_ = await roleManager.CreateAsync(new IdentityRole(nameof(Roles.ThesisStudent)));
		}
		if (!await roleManager.RoleExistsAsync(nameof(Roles.Student))) {
			_ = await roleManager.CreateAsync(new IdentityRole(nameof(Roles.Student)));
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
			_ = await userManager.AddToRolesAsync(directorTeacher, new List<string> { nameof(Roles.Teacher), nameof(Roles.DirectorTeacher) }.AsEnumerable());
		}
	}
}
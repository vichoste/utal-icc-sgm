using Microsoft.AspNetCore.Identity;

using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Seeders;

public static class StartupSeeder {
	public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager) {
		if (!await roleManager.RoleExistsAsync(nameof(Role.DirectorTeacher))) {
			_ = await roleManager.CreateAsync(new IdentityRole(nameof(Role.DirectorTeacher)));
		}
		if (!await roleManager.RoleExistsAsync(nameof(Role.CommitteeTeacher))) {
			_ = await roleManager.CreateAsync(new IdentityRole(nameof(Role.CommitteeTeacher)));
		}
		if (!await roleManager.RoleExistsAsync(nameof(Role.CourseTeacher))) {
			_ = await roleManager.CreateAsync(new IdentityRole(nameof(Role.CourseTeacher)));
		}
		if (!await roleManager.RoleExistsAsync(nameof(Role.GuideTeacher))) {
			_ = await roleManager.CreateAsync(new IdentityRole(nameof(Role.GuideTeacher)));
		}
		if (!await roleManager.RoleExistsAsync(nameof(Role.AssistantTeacher))) {
			_ = await roleManager.CreateAsync(new IdentityRole(nameof(Role.AssistantTeacher)));
		}
		if (!await roleManager.RoleExistsAsync(nameof(Role.Teacher))) {
			_ = await roleManager.CreateAsync(new IdentityRole(nameof(Role.Teacher)));
		}
		if (!await roleManager.RoleExistsAsync(nameof(Role.EngineerStudent))) {
			_ = await roleManager.CreateAsync(new IdentityRole(nameof(Role.EngineerStudent)));
		}
		if (!await roleManager.RoleExistsAsync(nameof(Role.CompletedStudent))) {
			_ = await roleManager.CreateAsync(new IdentityRole(nameof(Role.CompletedStudent)));
		}
		if (!await roleManager.RoleExistsAsync(nameof(Role.ThesisStudent))) {
			_ = await roleManager.CreateAsync(new IdentityRole(nameof(Role.ThesisStudent)));
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
			_ = await userManager.AddToRolesAsync(directorTeacher, new List<string> { nameof(Role.Teacher), nameof(Role.DirectorTeacher) }.AsEnumerable());
		}
	}
}
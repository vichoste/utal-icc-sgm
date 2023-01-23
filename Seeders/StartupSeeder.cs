using Microsoft.AspNetCore.Identity;

using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Seeders;

public static class StartupSeeder {
	public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager) {
		if (!await roleManager.RoleExistsAsync(Roles.Administrator.ToString())) {
			_ = await roleManager.CreateAsync(new IdentityRole(Roles.Administrator.ToString()));
		}
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

	public static async Task SeedAdministratorAsync(string email, string password, string firstName, string lastName, string rut, UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext) {
		var rootUser = new ApplicationUser {
			UserName = email,
			Email = email,
			EmailConfirmed = true,
			FirstName = firstName,
			LastName = lastName,
			Rut = rut
		};
		var studentProfile = new StudentProfile {
			ApplicationUser = rootUser
		};
		var teacherProfile = new TeacherProfile {
			ApplicationUser = rootUser
		};
		if (userManager.Users.All(a => a.Id != rootUser.Id)) {
			var user = await userManager.FindByEmailAsync(email);
			if (user == null) {
				_ = await userManager.CreateAsync(rootUser, password);
				_ = await userManager.AddToRoleAsync(rootUser, Roles.Administrator.ToString());
				_ = dbContext.StudentProfiles.Add(studentProfile);
				_ = dbContext.TeacherProfiles.Add(teacherProfile);
				_ = await dbContext.SaveChangesAsync();
			}
		}
	}
}
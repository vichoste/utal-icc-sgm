﻿using Microsoft.AspNetCore.Identity;

using Utal.Icc.Sgm.Areas.Account.Models;

namespace Utal.Icc.Sgm.Areas.Account.Seeders;

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
		if (!await roleManager.RoleExistsAsync(Roles.MainTeacher.ToString())) {
			_ = await roleManager.CreateAsync(new IdentityRole(Roles.MainTeacher.ToString()));
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
		if (!await roleManager.RoleExistsAsync(Roles.FinishedStudent.ToString())) {
			_ = await roleManager.CreateAsync(new IdentityRole(Roles.FinishedStudent.ToString()));
		}
		if (!await roleManager.RoleExistsAsync(Roles.ThesisStudent.ToString())) {
			_ = await roleManager.CreateAsync(new IdentityRole(Roles.ThesisStudent.ToString()));
		}
		if (!await roleManager.RoleExistsAsync(Roles.Student.ToString())) {
			_ = await roleManager.CreateAsync(new IdentityRole(Roles.Student.ToString()));
		}
	}

	public static async Task SeedAdministratorAsync(string email, string password, string firstName, string lastName, string universityId, string rut, UserManager<ApplicationUser> userManager) {
		var rootUser = new ApplicationUser {
			UserName = email,
			Email = email,
			EmailConfirmed = true,
			FirstName = firstName,
			LastName = lastName,
			UniversityId = universityId,
			Rut = rut
		};
		if (userManager.Users.All(u => u.Id != rootUser.Id)) {
			var user = await userManager.FindByEmailAsync(email);
			if (user == null) {
				_ = await userManager.CreateAsync(rootUser, password);
				_ = await userManager.AddToRoleAsync(rootUser, Roles.Administrator.ToString());
			}
		}
	}
}
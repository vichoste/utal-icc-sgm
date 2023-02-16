using Microsoft.AspNetCore.Identity;

using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Seeders;

public static class TestApplicationUserSeeder {
	public static async Task SeedStudentsAsync(UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore) {
		var random = new Random();
		var emailStore = (IUserEmailStore<ApplicationUser>)userStore;
		for (var i = 0; i < 50; i++) {
			var user = new ApplicationUser {
				FirstName = $"Nombre {i}",
				LastName = $"Apellido {i}",
				Rut = $"Rut {i}",
				CreatedAt = DateTimeOffset.Now,
				UpdatedAt = DateTimeOffset.Now,
				StudentUniversityId = $"N° de Matrícula {i}",
				StudentRemainingCourses = $"Cursos restantes {i}",
				StudentIsDoingThePractice = random.Next(2) == 1,
				StudentIsWorking = random.Next(2) == 1,
			};
			var email = $"estudiante{i}@sgm.utalca.cl";
			var check = await userManager.FindByIdAsync(email);
			if (check is null) {
				await userStore.SetUserNameAsync(user, email, CancellationToken.None);
				await emailStore.SetEmailAsync(user, email, CancellationToken.None);
				_ = await userManager.CreateAsync(user, "Abc_123");
				_ = await userManager.AddToRolesAsync(user, new List<string> { "Student" }.AsEnumerable());
			}
		}
	}

	public static async Task SeedTeachersAsync(UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore) {
		var random = new Random();
		var emailStore = (IUserEmailStore<ApplicationUser>)userStore;
		for (var i = 0; i < 10; i++) {
			var user = new ApplicationUser {
				FirstName = $"Nombre {i}",
				LastName = $"Apellido {i}",
				Rut = $"Rut {i}",
				CreatedAt = DateTimeOffset.Now,
				UpdatedAt = DateTimeOffset.Now,
				TeacherOffice = $"Oficina {i}",
				TeacherSchedule = $"Horario {i}",
				TeacherSpecialization = $"Especialización {i}",
			};
			var email = $"profesor{i}@sgm.utalca.cl";
			var check = await userManager.FindByIdAsync(email);
			if (check is null) {
				await userStore.SetUserNameAsync(user, email, CancellationToken.None);
				await emailStore.SetEmailAsync(user, email, CancellationToken.None);
				_ = await userManager.CreateAsync(user, "Abc_123");
				var roles = random.Next(4) switch {
					1 => new List<string> { "Teacher", "Guide" },
					2 => new List<string> { "Teacher", "Guide", "Assistant" },
					3 => new List<string> { "Teacher", "Assistant" },
					_ => new List<string> { "Teacher" },
				};
				_ = await userManager.AddToRolesAsync(user, roles.AsEnumerable());
			}
		}
	}
}
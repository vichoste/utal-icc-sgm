using System.Globalization;

using CsvHelper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Utal.Icc.Sgm.Models;
using Utal.Icc.Sgm.ViewModels;
using Utal.Icc.Sgm.Areas.University.Helpers;
using Utal.Icc.Sgm.Areas.University.ViewModels.User;
using Microsoft.EntityFrameworkCore;

namespace Utal.Icc.Sgm.Areas.University.Controllers;

[Area("University"), Authorize]
public class UserController : Controller {
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly IUserStore<ApplicationUser> _userStore;
	private readonly IUserEmailStore<ApplicationUser> _emailStore;

	public UserController(UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore) {
		this._userManager = userManager;
		this._userStore = userStore;
		this._emailStore = (IUserEmailStore<ApplicationUser>)this._userStore;
	}

	[Authorize(Roles = "Director")]
	public async Task<IActionResult> List(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		var parameters = new[] { "FirstName", "LastName", "StudentUniversityId", "Rut", "Email" };
		foreach (var parameter in parameters) {
			this.ViewData[$"{parameter}SortParam"] = sortOrder == parameter ? $"{parameter}Desc" : parameter;
		}
		this.ViewData["CurrentSort"] = sortOrder;
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		var users = await this._userManager.Users.ToListAsync();
		var paginator = Paginator<ApplicationViewModel>.Create(users.Select(u => new ApplicationUserViewModel {
			Id = u.Id,
			FirstName = u.FirstName,
			LastName = u.LastName,
			StudentUniversityId = u.StudentUniversityId,
			Rut = u.Rut,
			Email = u.Email,
			IsDeactivated = u.IsDeactivated,
		}).AsQueryable(), pageNumber ?? 1, 10);
		if (!string.IsNullOrEmpty(sortOrder)) {
			paginator.Sort(sortOrder);
		}
		if (!string.IsNullOrEmpty(currentFilter)) {
			paginator.Filter(currentFilter);
		}
		return this.View(paginator);
	}

	[Authorize(Roles = "Director")]
	public IActionResult BatchCreateStudents() => this.View(new CsvFileViewModel());

	[Authorize(Roles = "Director"), HttpPost]
	public async Task<IActionResult> BatchCreateStudents([FromForm] CsvFileViewModel input) {
		try {
			var errorMessages = new List<string>(); ;
			var successMessages = new List<string>();
			using var reader = new StreamReader(input.CsvFile!.OpenReadStream());
			using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
			var records = csv.GetRecords<CsvFileHelper>();
			foreach (var record in records) {
				var user = new ApplicationUser {
					FirstName = record.FirstName,
					LastName = record.LastName,
					StudentUniversityId = record.UniversityId,
					Rut = record.Rut,
					CreatedAt = DateTimeOffset.Now,
					UpdatedAt = DateTimeOffset.Now
				};
				await this._userStore.SetUserNameAsync(user, record.Email, CancellationToken.None);
				await this._emailStore.SetEmailAsync(user, record.Email, CancellationToken.None);
				var result = await this._userManager.CreateAsync(user, record!.Password!);
				if (!result.Succeeded) {
					errorMessages.Add($"Estudiante {record.Email} ya existe");
					continue;
				}
				_ = await this._userManager.AddToRoleAsync(user, "Student");
				successMessages.Add($"Estudiante {record.Email} creado correctamente.");
			}
			if (errorMessages.Any()) {
				this.TempData["ErrorMessages"] = errorMessages.AsEnumerable();
			}
			if (successMessages.Any()) {
				this.TempData["SuccessMessages"] = successMessages.AsEnumerable();
			}
			return this.RedirectToAction("List", "User", new { area = "University" });
		} catch {
			this.TempData["ErrorMessage"] = "Error al importar el archivo CSV.";
			return this.RedirectToAction("List", "User", new { area = "University" });
		}
	}

	[Authorize(Roles = "Director")]
	public IActionResult CreateTeacher() => this.View(new ApplicationUserViewModel());

	[Authorize(Roles = "Director"), HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> CreateTeacher([FromForm] ApplicationUserViewModel input) {
		var roles = new List<string>();
		if (input.IsGuide) {
			roles.Add("Guide");
		}
		if (input.IsAssistant) {
			roles.Add("Assistant");
		}
		if (input.IsCourse) {
			roles.Add("Course");
		}
		if (input.IsCommittee) {
			roles.Add("Committee");
		}
		var user = new ApplicationUser {
			FirstName = input.FirstName,
			LastName = input.LastName,
			Rut = input.Rut,
			CreatedAt = DateTimeOffset.Now,
			UpdatedAt = DateTimeOffset.Now
		};
		await this._userStore.SetUserNameAsync(user, input.Email, CancellationToken.None);
		await this._emailStore.SetEmailAsync(user, input.Email, CancellationToken.None);
		_ = await this._userManager.CreateAsync(user, input.Password!);
		_ = await this._userManager.AddToRoleAsync(user, "Teacher");
		_ = await this._userManager.AddToRolesAsync(user, roles);
		this.TempData["SuccessMessage"] = "Profesor creado exitosamente.";
		return this.RedirectToAction("Users", "User", new { area = "University" });
	}

	[Authorize(Roles = "Director")]
	public async Task<IActionResult> Edit(string id) {
		var user = await this._userManager.FindByIdAsync(id);
		if (user is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al usuario.";
			return this.RedirectToAction("Users", "User", new { area = "University" });
		}
		var output = user switch {
			_ when await this._userManager.IsInRoleAsync(user, "Student") => new ApplicationUserViewModel {
				Id = user.Id,
				FirstName = user.FirstName,
				LastName = user.LastName,
				Rut = user.Rut,
				Email = user.Email,
				CreatedAt = user.CreatedAt,
				UpdatedAt = user.UpdatedAt,
				StudentUniversityId = user.StudentUniversityId
			},
			_ when await this._userManager.IsInRoleAsync(user, "Teacher") => new ApplicationUserViewModel {
				Id = user.Id,
				FirstName = user.FirstName,
				LastName = user.LastName,
				Rut = user.Rut,
				Email = user.Email,
				CreatedAt = user.CreatedAt,
				UpdatedAt = user.UpdatedAt,
			},
			_ => null
		};
		if (output is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al usuario.";
			if (await this._userManager.IsInRoleAsync(user, "Student")) {
				return this.RedirectToAction("Users", "User", new { area = "University" });
			}
			if (await this._userManager.IsInRoleAsync(user, "Teacher")) {
				return this.RedirectToAction("Users", "User", new { area = "University" });
			}
			return this.RedirectToAction("Index", "Home", new { area = string.Empty });
		}
		return this.View(output);
	}

	[Authorize(Roles = "Director"), HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit([FromForm] ApplicationUserViewModel input) {
		var user = await this._userManager.FindByIdAsync(input.Id!);
		if (user is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al usuario.";
			return this.RedirectToAction("Users", "User", new { area = "University" });
		}
		await this._userStore.SetUserNameAsync(user, input.Email, CancellationToken.None);
		await this._emailStore.SetEmailAsync(user, input.Email, CancellationToken.None);
		user.FirstName = input.FirstName;
		user.LastName = input.LastName;
		user.Rut = input.Rut;
		user.UpdatedAt = DateTimeOffset.Now;
		if (await this._userManager.IsInRoleAsync(user, "Student")) {
			user.StudentUniversityId = input.StudentUniversityId;
		}
		_ = await this._userManager.UpdateAsync(user);
		if (await this._userManager.IsInRoleAsync(user, "Teacher")) {
			var roles = (await this._userManager.GetRolesAsync(user)).ToList();
			if (roles.Contains("Teacher")) {
				_ = roles.Remove("Teacher");
			}
			if (roles.Contains("Director")) {
				_ = roles.Remove("Director");
			}
			var rankRoles = new List<string>();
			if (input.IsGuide) {
				rankRoles.Add("Guide");
			}
			if (input.IsAssistant) {
				rankRoles.Add("Assistant");
			}
			if (input.IsCourse) {
				rankRoles.Add("Course");
			}
			if (input.IsCommittee) {
				rankRoles.Add("Committee");
			}
			_ = await this._userManager.AddToRolesAsync(user, rankRoles);
		}
		var output = new ApplicationUserViewModel {
			Id = user.Id,
			FirstName = user.FirstName,
			LastName = user.LastName,
			Rut = user.Rut,
			Email = user.Email,
			CreatedAt = user.CreatedAt,
			UpdatedAt = user.UpdatedAt
		};
		if (await this._userManager.IsInRoleAsync(user, "Teacher")) {
			output.IsAssistant = await this._userManager.IsInRoleAsync(user, "Assistant");
			output.IsCommittee = await this._userManager.IsInRoleAsync(user, "Committee");
			output.IsCourse = await this._userManager.IsInRoleAsync(user, "Course");
			output.IsGuide = await this._userManager.IsInRoleAsync(user, "Guide");
		}
		this.TempData["SuccessMessage"] = "Usuario editado correctamente.";
		return this.View(output);
	}

	[Authorize(Roles = "Director")]
	public async Task<IActionResult> Toggle(string id) {
		var user = await this._userManager.FindByIdAsync(id);
		if (user is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al usuario.";
			return this.RedirectToAction("Users", "User", new { area = "University" });
		}
		if (user.Id == this._userManager.GetUserId(this.User) || (await this._userManager.GetRolesAsync(user)).Contains("Director")) {
			this.TempData["ErrorMessage"] = "Error al cambiar el estado de la activación al usuario.";
			if (await this._userManager.IsInRoleAsync(user, "Student")) {
				return this.RedirectToAction("Users", "User", new { area = "University" });
			}
			return this.RedirectToAction("Users", "User", new { area = "University" });
		}
		var output = new ApplicationUserViewModel {
			Id = user.Id,
			Email = user.Email,
			IsDeactivated = user.IsDeactivated
		};
		return this.View(output);
	}

	[Authorize(Roles = "Director"), HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Toggle([FromForm] ApplicationUserViewModel input) {
		var user = await this._userManager.FindByIdAsync(input.Id!);
		if (user is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al usuario.";
			return this.RedirectToAction("Users", "User", new { area = "University" });
		}
		if (user.Id == this._userManager.GetUserId(this.User) || (await this._userManager.GetRolesAsync(user)).Contains("Director")) {
			this.TempData["ErrorMessage"] = "Error al cambiar el estado de la activación al usuario.";
			return this.RedirectToAction("Index", "Home", new { area = string.Empty });
		}
		user.IsDeactivated = !user.IsDeactivated;
		user.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(user);
		this.TempData["SuccessMessage"] = user.IsDeactivated ? "Usuario desactivado correctamente." : "Usuario activado correctamente.";
		return this.RedirectToAction("Users", "User", new { area = "University" });
	}

	[Authorize(Roles = "Director")]
	public async Task<IActionResult> Transfer(string currentId, string @newId) {
		var current = await this._userManager.FindByIdAsync(currentId);
		var @new = await this._userManager.FindByIdAsync(@newId);
		var check = (current, @new) switch {
			(ApplicationUser, ApplicationUser) => true,
			(ApplicationUser teacher, _) when teacher.IsDeactivated || (await this._userManager.IsInRoleAsync(teacher, "Teacher") && await this._userManager.IsInRoleAsync(teacher, "Director")) => false,
			(_, ApplicationUser teacher) when teacher.IsDeactivated || await this._userManager.IsInRoleAsync(teacher, "Teacher") => false,
			_ => false
		};
		check = check && (current!.Id != @new!.Id);
		if (!check) {
			this.TempData["ErrorMessage"] = "Revisa los profesores fuente y objetivo antes de hacer la transferencia.";
			return this.RedirectToAction("Users", "User", new { area = "University" });
		}
		var transferViewModel = new TransferViewModel {
			CurrentDirectorTeacherId = current!.Id,
			NewDirectorTeacherId = @new!.Id,
			NewDirectorTeacherName = $"{@new.FirstName} {@new.LastName}"
		};
		return this.View(transferViewModel);
	}

	[Authorize(Roles = "Director"), HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Transfer([FromForm] TransferViewModel model) {
		var current = await this._userManager.FindByIdAsync(model.CurrentDirectorTeacherId!);
		var @new = await this._userManager.FindByIdAsync(model.NewDirectorTeacherId!);
		var check = (current, @new) switch {
			(ApplicationUser, ApplicationUser) => true,
			(ApplicationUser teacher, _) when teacher.IsDeactivated || (await this._userManager.IsInRoleAsync(teacher, "Teacher") && await this._userManager.IsInRoleAsync(teacher, "Director")) => false,
			(_, ApplicationUser teacher) when teacher.IsDeactivated || await this._userManager.IsInRoleAsync(teacher, "Teacher") => false,
			_ => false
		};
		check = check && (current!.Id != @new!.Id);
		if (!check) {
			this.TempData["ErrorMessage"] = "Revisa los profesores fuente y objetivo antes de hacer la transferencia.";
			return this.RedirectToAction("Users", "User", new { area = "University" });
		}
		_ = await this._userManager.RemoveFromRoleAsync(current!, "Director");
		_ = await this._userManager.AddToRoleAsync(@new!, "Director");
		current!.UpdatedAt = DateTimeOffset.Now;
		@new!.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(current);
		this.TempData["SuccessMessage"] = "Director de carrera transferido correctamente.";
		return this.RedirectToAction("Users", "User", new { area = "University" });
	}
}
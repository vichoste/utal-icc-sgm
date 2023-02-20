using System.Globalization;

using CsvHelper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Utal.Icc.Sgm.Areas.University.Helpers;
using Utal.Icc.Sgm.Areas.University.ViewModels.User;
using Utal.Icc.Sgm.Models;
using Utal.Icc.Sgm.ViewModels;

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
	public async Task<IActionResult> Students(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		var parameters = new[] { "FirstName", "LastName", "UniversityId", "Rut", "Email" };
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
		var users = (await this._userManager.GetUsersInRoleAsync("Student")).Select(u => new ApplicationUserViewModel {
			Id = u.Id,
			FirstName = u.FirstName,
			LastName = u.LastName,
			UniversityId = u.UniversityId,
			Rut = u.Rut,
			Email = u.Email,
			IsDeactivated = u.IsDeactivated,
		}).AsQueryable();
		var paginator = Paginator<ApplicationUserViewModel>.Create(users, pageNumber ?? 1, 10);
		if (!string.IsNullOrEmpty(sortOrder)) {
			paginator = Paginator<ApplicationUserViewModel>.Sort(paginator.AsQueryable(), sortOrder, pageNumber ?? 1, 6, parameters);
		}
		if (!string.IsNullOrEmpty(searchString)) {
			paginator = Paginator<ApplicationUserViewModel>.Filter(paginator.AsQueryable(), searchString, pageNumber ?? 1, 6, parameters);
		}
		return this.View(paginator);
	}

	[Authorize(Roles = "Director")]
	public async Task<IActionResult> Teachers(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		var parameters = new[] { "FirstName", "LastName", "Rut", "Email" };
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
		var users = (await this._userManager.GetUsersInRoleAsync("Teacher")).Select(u => new ApplicationUserViewModel {
			Id = u.Id,
			FirstName = u.FirstName,
			LastName = u.LastName,
			Rut = u.Rut,
			Email = u.Email,
			IsDeactivated = u.IsDeactivated,
		}).AsQueryable();
		var paginator = Paginator<ApplicationUserViewModel>.Create(users, pageNumber ?? 1, 10);
		if (!string.IsNullOrEmpty(sortOrder)) {
			paginator = Paginator<ApplicationUserViewModel>.Sort(paginator.AsQueryable(), sortOrder, pageNumber ?? 1, 6, parameters);
		}
		if (!string.IsNullOrEmpty(searchString)) {
			paginator = Paginator<ApplicationUserViewModel>.Filter(paginator.AsQueryable(), searchString, pageNumber ?? 1, 6, parameters);
		}
		return this.View(paginator);
	}

	[Authorize(Roles = "Director")]
	public IActionResult BatchCreateStudents() => this.View(new CsvFileViewModel());

	[Authorize(Roles = "Director"), HttpPost]
	public async Task<IActionResult> BatchCreateStudents([FromForm] CsvFileViewModel input) {
		try {
			var warningMessages = new List<string>(); ;
			var successMessages = new List<string>();
			using var reader = new StreamReader(input.CsvFile!.OpenReadStream());
			using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
			var records = csv.GetRecords<CsvFileHelper>();
			foreach (var record in records) {
				var user = new ApplicationUser {
					FirstName = record.FirstName,
					LastName = record.LastName,
					UniversityId = record.UniversityId,
					Rut = record.Rut,
					CreatedAt = DateTimeOffset.Now,
					UpdatedAt = DateTimeOffset.Now
				};
				await this._userStore.SetUserNameAsync(user, record.Email, CancellationToken.None);
				await this._emailStore.SetEmailAsync(user, record.Email, CancellationToken.None);
				var result = await this._userManager.CreateAsync(user, record!.Password!);
				if (!result.Succeeded) {
					warningMessages.Add($"Estudiante con e-mail {record.Email} ya existe");
					continue;
				}
				_ = await this._userManager.AddToRoleAsync(user, "Student");
				successMessages.Add($"Estudiante con e-mail {record.Email} creado correctamente.");
			}
			if (warningMessages.Any()) {
				this.TempData["WarningMessages"] = warningMessages.AsEnumerable();
			}
			if (successMessages.Any()) {
				this.TempData["SuccessMessages"] = successMessages.AsEnumerable();
			}
			return this.RedirectToAction("Students", "User", new { area = "University" });
		} catch {
			this.TempData["ErrorMessage"] = "Error al importar el archivo CSV.";
			return this.RedirectToAction("Students", "User", new { area = "University" });
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
		return this.RedirectToAction("Teachers", "User", new { area = "University" });
	}

	[Authorize(Roles = "Director")]
	public async Task<IActionResult> Edit(string id) {
		var user = await this._userManager.FindByIdAsync(id);
		if (user is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al usuario.";
			return this.RedirectToAction("Students", "User", new { area = "University" });
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
				UniversityId = user.UniversityId
			},
			_ when await this._userManager.IsInRoleAsync(user, "Teacher") => new ApplicationUserViewModel {
				Id = user.Id,
				FirstName = user.FirstName,
				LastName = user.LastName,
				Rut = user.Rut,
				Email = user.Email,
				CreatedAt = user.CreatedAt,
				UpdatedAt = user.UpdatedAt,
				IsAssistant = await this._userManager.IsInRoleAsync(user, "Assistant"),
				IsCommittee = await this._userManager.IsInRoleAsync(user, "Committee"),
				IsCourse = await this._userManager.IsInRoleAsync(user, "Course"),
				IsGuide = await this._userManager.IsInRoleAsync(user, "Guide")
			},
			_ => null
		};
		return this.View(output);
	}

	[Authorize(Roles = "Director"), HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit([FromForm] ApplicationUserViewModel input) {
		var user = await this._userManager.FindByIdAsync(input.Id!);
		if (user is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al usuario.";
			return this.RedirectToAction("Students", "User", new { area = "University" });
		}
		await this._userStore.SetUserNameAsync(user, input.Email, CancellationToken.None);
		await this._emailStore.SetEmailAsync(user, input.Email, CancellationToken.None);
		user.FirstName = input.FirstName;
		user.LastName = input.LastName;
		user.Rut = input.Rut;
		user.UpdatedAt = DateTimeOffset.Now;
		if (await this._userManager.IsInRoleAsync(user, "Student")) {
			user.UniversityId = input.UniversityId;
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
		this.ViewBag.SuccessMessage = "Usuario editado correctamente.";
		return this.View(output);
	}

	[Authorize(Roles = "Director")]
	public async Task<IActionResult> Toggle(string id) {
		var user = await this._userManager.FindByIdAsync(id);
		if (user is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al usuario.";
			return this.RedirectToAction("Students", "User", new { area = "University" });
		}
		if (user.Id == this._userManager.GetUserId(this.User)) {
			this.TempData["ErrorMessage"] = "No te puedes desactivar a tí mismo.";
			if (await this._userManager.IsInRoleAsync((await this._userManager.FindByIdAsync(id))!, "Student")) {
				return this.RedirectToAction("Students", "User", new { area = "University" });
			} else if (await this._userManager.IsInRoleAsync((await this._userManager.FindByIdAsync(id))!, "Teacher")) {
				return this.RedirectToAction("Teachers", "User", new { area = "University" });
			}
			return this.RedirectToAction("Students", "User", new { area = "University" });
		} else if ((await this._userManager.GetRolesAsync(user)).Contains("Director")) {
			this.TempData["ErrorMessage"] = "No puedes desactivar al director de carrera actual.";
			return this.RedirectToAction("Teachers", "User", new { area = "University" });
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
			return this.RedirectToAction("Students", "User", new { area = "University" });
		}
		if (user.Id == this._userManager.GetUserId(this.User)) {
			this.TempData["ErrorMessage"] = "No te puedes desactivar a tí mismo.";
			if (await this._userManager.IsInRoleAsync((await this._userManager.FindByIdAsync(input.Id!))!, "Student")) {
				return this.RedirectToAction("Students", "User", new { area = "University" });
			} else if (await this._userManager.IsInRoleAsync((await this._userManager.FindByIdAsync(input.Id!))!, "Teacher")) {
				return this.RedirectToAction("Teachers", "User", new { area = "University" });
			}
			return this.RedirectToAction("Students", "User", new { area = "University" });
		} else if ((await this._userManager.GetRolesAsync(user)).Contains("Director")) {
			this.TempData["ErrorMessage"] = "No puedes desactivar al director de carrera actual.";
			return this.RedirectToAction("Teachers", "User", new { area = "University" });
		}
		user.IsDeactivated = !user.IsDeactivated;
		user.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(user);
		if (await this._userManager.IsInRoleAsync((await this._userManager.FindByIdAsync(input.Id!))!, "Student")) {
			this.TempData["SuccessMessage"] = user.IsDeactivated ? "Estudiante desactivado correctamente." : "Estudiante activado correctamente.";
			return this.RedirectToAction("Students", "User", new { area = "University" });
		} else if (await this._userManager.IsInRoleAsync((await this._userManager.FindByIdAsync(input.Id!))!, "Teacher")) {
			this.TempData["SuccessMessage"] = user.IsDeactivated ? "Profesor desactivado correctamente." : "Profesor activado correctamente.";
			return this.RedirectToAction("Teachers", "User", new { area = "University" });
		}
		this.TempData["SuccessMessage"] = user.IsDeactivated ? "Estudiante desactivado correctamente." : "Estudiante activado correctamente.";
		return this.RedirectToAction("Students", "User", new { area = "University" });
	}

	[Authorize(Roles = "Director")]
	public async Task<IActionResult> Transfer(string currentId, string @newId) {
		var current = await this._userManager.FindByIdAsync(currentId);
		var @new = await this._userManager.FindByIdAsync(@newId);
		var check = current is not null && @new is not null;
		check = check && (await this._userManager.GetRolesAsync(current!)).Contains("Director");
		check = check && (await this._userManager.GetRolesAsync(@new!)).Contains("Teacher");
		check = check && current!.Id != @new!.Id;
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
		var check = current is not null && @new is not null;
		check = check && (await this._userManager.GetRolesAsync(current!)).Contains("Director");
		check = check && (await this._userManager.GetRolesAsync(@new!)).Contains("Teacher");
		check = check && current!.Id != @new!.Id;
		if (!check) {
			this.TempData["ErrorMessage"] = "Revisa los profesores fuente y objetivo antes de hacer la transferencia.";
			return this.RedirectToAction("Teachers", "User", new { area = "University" });
		}
		_ = await this._userManager.RemoveFromRoleAsync(current!, "Director");
		_ = await this._userManager.AddToRoleAsync(@new!, "Director");
		current!.UpdatedAt = DateTimeOffset.Now;
		@new!.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(current);
		this.TempData["SuccessMessage"] = "Transferencia realizada correctamente.";
		return this.RedirectToAction("Teachers", "User", new { area = "University" });
	}
}
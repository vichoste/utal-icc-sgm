using System.Globalization;

using CsvHelper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Utal.Icc.Sgm.Areas.DirectorTeacher.Helpers;
using Utal.Icc.Sgm.Areas.DirectorTeacher.ViewModels.Student;
using Utal.Icc.Sgm.Controllers;
using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;
using Utal.Icc.Sgm.ViewModels;

using static Utal.Icc.Sgm.Models.ApplicationUser;

namespace Utal.Icc.Sgm.Areas.DirectorTeacher.Controllers;

[Area(nameof(DirectorTeacher)), Authorize(Roles = nameof(Roles.DirectorTeacher))]
public class StudentController : ApplicationUserController {
	public override string[]? Parameters { get; set; }

	public StudentController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore, SignInManager<ApplicationUser> signInManager) : base(dbContext, userManager, userStore, signInManager) { }

	public override void SetSortParameters(string sortOrder, params string[] parameters) {
		foreach (var parameter in parameters) {
			this.ViewData[$"{parameter}SortParam"] = sortOrder == parameter ? $"{parameter}Desc" : parameter;
		}
		this.ViewData["CurrentSort"] = sortOrder;
	}
	
	public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		var parameters = new[] { nameof(ApplicationUserViewModel.FirstName), nameof(ApplicationUserViewModel.LastName), nameof(ApplicationUserViewModel.StudentUniversityId), nameof(ApplicationUserViewModel.Rut), nameof(ApplicationUserViewModel.Email) };
		this.SetSortParameters(sortOrder, parameters);
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		return this.View(await base.GetPaginatedViewModelsAsync<ApplicationUserViewModel>(sortOrder, currentFilter, searchString, pageNumber, parameters,
			async () => (await this._userManager.GetUsersInRoleAsync(nameof(Roles.Student))).Select(
				u => new ApplicationUserViewModel {
					Id = u.Id,
					FirstName = u.FirstName,
					LastName = u.LastName,
					StudentUniversityId = u.StudentUniversityId,
					Rut = u.Rut,
					Email = u.Email,
					IsDeactivated = u.IsDeactivated
				}
				).AsEnumerable()
			)
		);
	}

	public async Task<IActionResult> Create() => await base.CheckSession() is not ApplicationUser user
		? this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty })
		: user.IsDeactivated ? this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty }) : this.View(new CsvFileViewModel());

	[HttpPost]
	public async Task<IActionResult> Create([FromForm] CsvFileViewModel input) {
		if (await base.CheckSession() is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
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
				_ = await this._userManager.AddToRoleAsync(user, nameof(Roles.Student));
				successMessages.Add($"Estudiante {record.Email} creado correctamente.");
			}
			if (errorMessages.Any()) {
				this.TempData["ErrorMessages"] = errorMessages.AsEnumerable();
			}
			if (successMessages.Any()) {
				this.TempData["SuccessMessages"] = successMessages.AsEnumerable();
			}
			return this.RedirectToAction(nameof(StudentController.Index), nameof(StudentController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
		} catch {
			this.TempData["ErrorMessage"] = "Error al importar el archivo CSV.";
			return this.RedirectToAction(nameof(StudentController.Index), nameof(StudentController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
		}
	}

	public async Task<IActionResult> Edit(string id) {
		if (await base.CheckSession() is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var output = await base.EditAsync<ApplicationUserViewModel>(id);
		if (output is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al estudiante.";
			return this.RedirectToAction(nameof(StudentController.Index), nameof(StudentController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
		}
		return this.View(output);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit([FromForm] ApplicationUserViewModel input) {
		if (await base.CheckSession() is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var output = await base.EditAsync<ApplicationUserViewModel>(input);
		if (output is null) {
			this.TempData["ErrorMessage"] = "Error al editar al estudiante.";
			return this.RedirectToAction(nameof(StudentController.Index), nameof(StudentController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
		}
		this.TempData["SuccessMessage"] = "Estudiante editado correctamente.";
		return this.View(output);
	}

	public async Task<IActionResult> ToggleActivation(string id) {
		if (await base.CheckSession() is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var output = await base.ToggleActivationAsync<ApplicationUserViewModel>(id);
		if (output is null) {
			this.TempData["ErrorMessage"] = "Error al cambiar la activación al estudiante.";
			return this.RedirectToAction(nameof(StudentController.Index), nameof(StudentController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
		}
		this.TempData["SuccessMessage"] = "Estudiante cambiado correctamente.";
		return this.RedirectToAction(nameof(StudentController.Index), nameof(StudentController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> ToggleActivation([FromForm] ApplicationUserViewModel input) {
		if (await base.CheckSession() is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var output = await base.ToggleActivationAsync<ApplicationUserViewModel>(input);
		if (output is null) {
			this.TempData["ErrorMessage"] = "Error al cambiar la activación al estudiante.";
			return this.RedirectToAction(nameof(StudentController.Index), nameof(StudentController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
		}
		this.TempData["SuccessMessage"] = "Estudiante cambiado correctamente.";
		return this.RedirectToAction(nameof(StudentController.Index), nameof(StudentController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
	}
}
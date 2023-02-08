using System.Globalization;

using CsvHelper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using Utal.Icc.Sgm.Areas.DirectorTeacher.Helpers;
using Utal.Icc.Sgm.Areas.DirectorTeacher.ViewModels;
using Utal.Icc.Sgm.Controllers;
using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;
using Utal.Icc.Sgm.ViewModels;

using static Utal.Icc.Sgm.Models.ApplicationUser;

namespace Utal.Icc.Sgm.Areas.DirectorTeacher.Controllers;

[Area(nameof(DirectorTeacher)), Authorize(Roles = nameof(Roles.DirectorTeacher))]
public class StudentController : ApplicationController, IApplicationUserViewModelFilterable, IApplicationUserViewModelSortable {
	public StudentController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore, IUserEmailStore<ApplicationUser> emailStore) : base(dbContext, userManager, userStore, emailStore) { }

	public IOrderedEnumerable<T> Sort<T>(string sortOrder, IEnumerable<T> applicationUserViewModels, params string[] parameters) where T : ApplicationUserViewModel {
		foreach (var parameter in parameters) {
			if (parameter == sortOrder) {
				return applicationUserViewModels.OrderBy(s => s.GetType().GetProperty(parameter)!.GetValue(s, null));
			} else if ($"{parameter}Desc" == sortOrder) {
				return applicationUserViewModels.OrderByDescending(s => s.GetType().GetProperty(parameter)!.GetValue(s, null));
			}
		}
		return applicationUserViewModels.OrderBy(s => s.GetType().GetProperty(parameters[0]));
	}

	public IEnumerable<T> Filter<T>(string searchString, IOrderedEnumerable<T> applicationUserViewModels, params string[] parameters) where T : ApplicationUserViewModel {
		var result = new List<T>();
		foreach (var parameter in parameters) {
			var partials = applicationUserViewModels
					.Where(s => (s.GetType().GetProperty(parameter)!.GetValue(s) as string)!.Contains(searchString));
			foreach (var partial in partials) {
				if (!result.Any(ivm => ivm.Id == partial.Id)) {
					result.Add(partial);
				}
			}
		}
		return result.AsEnumerable();
	}

	public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		if (await base.CheckSession() is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var parameters = new[] { nameof(ApplicationUserViewModel.FirstName), nameof(ApplicationUserViewModel.LastName), nameof(ApplicationUserViewModel.StudentUniversityId), nameof(ApplicationUserViewModel.Rut), nameof(ApplicationUserViewModel.Email) };
		this.SetSortParameters(sortOrder, parameters);
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		var users = (await this._userManager.GetUsersInRoleAsync(nameof(Roles.Student))).Select(
			s => new ApplicationUserViewModel {
				FirstName = s.FirstName,
				LastName = s.LastName,
				StudentUniversityId = s.StudentUniversityId,
				Rut = s.Rut,
				Email = s.Email,
				IsDeactivated = s.IsDeactivated
			}
		);
		var ordered = this.Sort(sortOrder, users, parameters);
		var viewModels = !searchString.IsNullOrEmpty() ? this.Filter(searchString, ordered, parameters) : ordered;
		return this.View(PaginatedList<ApplicationUserViewModel>.Create(viewModels.AsQueryable(), pageNumber ?? 1, 6));
	}

	public async Task<IActionResult> Create() => await base.CheckSession() is not ApplicationUser teacher
			? this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty })
			: teacher.IsDeactivated ? this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty }) : this.View(new CsvFileViewModel());

	[HttpPost]
	public async Task<IActionResult> Create([FromForm] CsvFileViewModel model) {
		if (await base.CheckSession() is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		try {
			var errorMessages = new List<string>(); ;
			var successMessages = new List<string>();
			using var reader = new StreamReader(model.CsvFile!.OpenReadStream());
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
		if (await this.CheckApplicationUser(id) is not ApplicationUser user) {
			this.TempData["ErrorMessage"] = "Error al obtener al estudiante.";
			return this.RedirectToAction(nameof(StudentController.Index), nameof(StudentController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
		}
		var viewModel = new ApplicationUserViewModel {
			Id = id,
			FirstName = user.FirstName,
			LastName = user.LastName,
			StudentUniversityId = user.StudentUniversityId,
			Rut = user.Rut,
			Email = user.Email,
			CreatedAt = user.CreatedAt,
			UpdatedAt = user.UpdatedAt
		};
		return this.View(viewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit([FromForm] ApplicationUserViewModel viewModel) {
		if (await base.CheckSession() is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		if (await this.CheckApplicationUser(viewModel.Id!) is not ApplicationUser user) {
			this.TempData["ErrorMessage"] = "Error al obtener al estudiante.";
			return this.RedirectToAction(nameof(StudentController.Index), nameof(StudentController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
		}
		await this._userStore.SetUserNameAsync(user, viewModel.Email, CancellationToken.None);
		await this._emailStore.SetEmailAsync(user, viewModel.Email, CancellationToken.None);
		user.FirstName = model.FirstName;
		user.LastName = model.LastName;
		user.StudentUniversityId = model.UniversityId;
		user.Rut = model.Rut;
		user.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(user);
		var viewModel = new EditViewModel {
			Id = student.Id,
			FirstName = student.FirstName,
			LastName = student.LastName,
			UniversityId = student.StudentUniversityId,
			Rut = student.Rut,
			Email = student.Email,
			CreatedAt = student.CreatedAt,
			UpdatedAt = student.UpdatedAt
		};
		this.ViewBag.SuccessMessage = "Estudiante actualizado correctamente.";
		return this.View(viewModel);
	}

	public async Task<IActionResult> ToggleActivation(string id) {
		if (await this.CheckTeacherSession() is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var student = await this._userManager.FindByIdAsync(id);
		if (student is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al estudiante.";
			return this.RedirectToAction(nameof(StudentController.Index), nameof(StudentController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
		}
		if (student!.Id == this._userManager.GetUserId(this.User)) {
			this.TempData["ErrorMessage"] = !student.IsDeactivated ? "No te puedes desactivar a tí mismo." : "¡No deberías haber llegado a este punto!";
			return this.RedirectToAction(nameof(StudentController.Index), nameof(StudentController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
		}
		var toggleActivationModel = new ToggleActivationViewModel {
			Id = student.Id,
			Email = student.Email,
			IsDeactivated = student.IsDeactivated
		};
		return this.View(toggleActivationModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> ToggleActivation([FromForm] ToggleActivationViewModel model) {
		if (await this.CheckTeacherSession() is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var student = await this._userManager.FindByIdAsync(model.Id!);
		if (student is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al estudiante.";
			return this.RedirectToAction(nameof(StudentController.Index), nameof(StudentController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
		}
		student.IsDeactivated = !model.IsDeactivated;
		student.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(student);
		this.TempData["SuccessMessage"] = !model.IsDeactivated ? "Estudiante desactivado correctamente." : "Estudiante activado correctamente.";
		return this.RedirectToAction(nameof(StudentController.Index), nameof(StudentController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
	}
}
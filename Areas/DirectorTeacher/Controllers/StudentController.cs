using System.Globalization;

using CsvHelper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using NuGet.DependencyResolver;

using Utal.Icc.Sgm.Areas.DirectorTeacher.Views.Student;
using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;

using static Utal.Icc.Sgm.Models.ApplicationUser;

namespace Utal.Icc.Sgm.Areas.DirectorTeacher.Controllers;

[Area("DirectorTeacher"), Authorize(Roles = "DirectorTeacher")]
public class StudentController : Controller {
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly IUserStore<ApplicationUser> _userStore;
	private readonly IUserEmailStore<ApplicationUser> _emailStore;

	public StudentController(UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore) {
		this._userManager = userManager;
		this._userStore = userStore;
		this._emailStore = (IUserEmailStore<ApplicationUser>)this._userStore;
	}

	protected async Task<ApplicationUser> CheckTeacherSession() {
		var teacher = await this._userManager.GetUserAsync(this.User);
		return teacher is null || teacher.IsDeactivated ? null! : teacher;
	}

	protected async Task<ApplicationUser> CheckApplicationUser(string applicationUserId) {
		var applicationUser = await this._userManager.FindByIdAsync(applicationUserId);
		return applicationUser is null || applicationUser.IsDeactivated ? null! : applicationUser;
	}

	protected void SetSortParameters(string sortOrder, params string[] parameters) {
		foreach (var parameter in parameters) {
			this.ViewData[$"{parameter}SortParam"] = sortOrder == parameter ? $"{parameter}Desc" : parameter;
		}
		this.ViewData["CurrentSort"] = sortOrder;
	}

	protected IOrderedEnumerable<ApplicationUser> OrderApplicationUsers(string sortOrder, IEnumerable<ApplicationUser> applicationUsers, params string[] parameters) {
		foreach (var parameter in parameters) {
			if (parameter == sortOrder) {
				return applicationUsers.OrderBy(s => s.GetType().GetProperty(parameter));
			} else if ($"{parameter}Desc" == sortOrder) {
				return applicationUsers.OrderByDescending(s => s.GetType().GetProperty(parameter));
			}
		}
		return applicationUsers.OrderBy(s => s.GetType().GetProperty(parameters[0]));
	}

	protected List<IndexViewModel> FilterApplicationUsers(string searchString, IOrderedEnumerable<ApplicationUser> applicationUsers, params string[] parameters) {
		var result = new List<IndexViewModel>();
		foreach (var parameter in parameters) {
			var partials = applicationUsers
					.Where(s => (s.GetType().GetProperty(parameter)!.GetValue(s, null) as string)!.Contains(searchString))
					.Select(s => new IndexViewModel {
						Id = s.Id,
						FirstName = s.FirstName,
						LastName = s.LastName,
						UniversityId = s.StudentUniversityId,
						Rut = s.Rut,
						Email = s.Email,
						IsDeactivated = s.IsDeactivated
					});
			result.AddRange(partials);
		}
		return result;
	}

	public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		if (await this.CheckTeacherSession() is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var parameters = new[] { "FirstName", "LastName", "UniversityId", "RutSort", "Email" };
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		var students = await this._userManager.GetUsersInRoleAsync(Roles.Student.ToString());
		var orderedStudents = this.OrderApplicationUsers(sortOrder, students, parameters);
		var indexViewModels = this.FilterApplicationUsers(searchString, orderedStudents, parameters);
		return this.View(PaginatedList<IndexViewModel>.Create((await this._userManager.GetUserAsync(this.User))!.Id, indexViewModels.AsQueryable(), pageNumber ?? 1, 6));
	}

	public async Task<IActionResult> Create() => await this.CheckTeacherSession() is not ApplicationUser teacher
			? this.RedirectToAction("Index", "Home", new { area = "" })
			: teacher.IsDeactivated ? this.RedirectToAction("Index", "Home", new { area = "" }) : this.View(new InputViewModel());

	[HttpPost]
	public async Task<IActionResult> Create([FromForm] InputViewModel model) {
		if (await this.CheckTeacherSession() is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		try {
			var errorMessages = new List<string>();
			var warningMessages = new List<string>();
			var successMessages = new List<string>();
			using var reader = new StreamReader(model.CsvFile!.OpenReadStream());
			using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
			var records = csv.GetRecords<CreateViewModel>();
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
				_ = await this._userManager.AddToRoleAsync(user, Roles.Student.ToString());
				successMessages.Add($"Estudiante {record.Email} creado correctamente.");
			}
			if (errorMessages.Any()) {
				this.TempData["ErrorMessages"] = errorMessages;
			}
			if (successMessages.Any()) {
				this.TempData["SuccessMessages"] = successMessages;
			}
			return this.RedirectToAction("Index", "Student", new { area = "DirectorTeacher" });
		} catch {
			this.TempData["ErrorMessage"] = "Error al importar el archivo CSV.";
			return this.RedirectToAction("Index", "Student", new { area = "DirectorTeacher" });
		}
	}

	public async Task<IActionResult> Edit(string id) {
		if (await this.CheckTeacherSession() is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (await this.CheckApplicationUser(id) is not ApplicationUser student) {
			this.TempData["ErrorMessage"] = "Error al obtener al estudiante.";
			return this.RedirectToAction("Index", "Student", new { area = "DirectorTeacher" });
		}
		var editViewModel = new EditViewModel {
			Id = id,
			FirstName = student.FirstName,
			LastName = student.LastName,
			UniversityId = student.StudentUniversityId,
			Rut = student.Rut,
			Email = student.Email,
			CreatedAt = student.CreatedAt,
			UpdatedAt = student.UpdatedAt
		};
		return this.View(editViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit([FromForm] EditViewModel model) {
		if (await this.CheckTeacherSession() is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (await this.CheckApplicationUser(model.Id!) is not ApplicationUser student) {
			this.TempData["ErrorMessage"] = "Error al obtener al estudiante.";
			return this.RedirectToAction("Index", "Student", new { area = "DirectorTeacher" });
		}
		await this._userStore.SetUserNameAsync(student, model.Email, CancellationToken.None);
		await this._emailStore.SetEmailAsync(student, model.Email, CancellationToken.None);
		student.FirstName = model.FirstName;
		student.LastName = model.LastName;
		student.StudentUniversityId = model.UniversityId;
		student.Rut = model.Rut;
		student.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(student);
		var editViewModel = new EditViewModel {
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
		return this.View(editViewModel);
	}

	public async Task<IActionResult> ToggleActivation(string id) {
		if (await this.CheckTeacherSession() is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (await this.CheckApplicationUser(id) is not ApplicationUser student) {
			this.TempData["ErrorMessage"] = "Error al obtener al estudiante.";
			return this.RedirectToAction("Index", "Student", new { area = "DirectorTeacher" });
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
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (await this.CheckApplicationUser(model.Id!) is not ApplicationUser student) {
			this.TempData["ErrorMessage"] = "Error al obtener al estudiante.";
			return this.RedirectToAction("Index", "Student", new { area = "DirectorTeacher" });
		}
		student.IsDeactivated = !model.IsDeactivated;
		student.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(student);
		this.TempData["SuccessMessage"] = !model.IsDeactivated ? "Estudiante desactivado correctamente." : "Estudiante activado correctamente.";
		return this.RedirectToAction("Index", "Student", new { area = "DirectorTeacher" });
	}
}
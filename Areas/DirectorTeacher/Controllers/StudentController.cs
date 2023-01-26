using System.Globalization;

using CsvHelper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using Utal.Icc.Sgm.Areas.DirectorTeacher.Views.Student;
using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Areas.DirectorTeacher.Controllers;

[Area("DirectorTeacher"), Authorize(Roles = "DirectorTeacher")]
public class StudentController : Controller {
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly IUserStore<ApplicationUser> _userStore;
	private readonly IUserEmailStore<ApplicationUser> _emailStore;

	public StudentController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore) {
		this._signInManager = signInManager;
		this._userManager = userManager;
		this._userStore = userStore;
		this._emailStore = (IUserEmailStore<ApplicationUser>)this._userStore;
	}

	public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		this.ViewData["FirstNameSortParam"] = sortOrder == "FirstName" ? "FirstNameDesc" : "FirstName";
		this.ViewData["LastNameSortParam"] = sortOrder == "LastName" ? "LastNameDesc" : "LastName";
		this.ViewData["UniversityIdSortParam"] = sortOrder == "UniversityId" ? "UniversityIdDesc" : "UniversityId";
		this.ViewData["RutSortParam"] = sortOrder == "Rut" ? "RutDesc" : "Rut";
		this.ViewData["EmailSortParam"] = sortOrder == "Email" ? "EmailDesc" : "Email";
		this.ViewData["CurrentSort"] = sortOrder;
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		var students = await this._userManager.GetUsersInRoleAsync(Roles.Student.ToString());
		var orderedStudents = sortOrder switch {
			"FirstName" => students.OrderBy(s => s.FirstName),
			"FirstNameDesc" => students.OrderByDescending(s => s.FirstName),
			"UniversityId" => students.OrderBy(s => s.UniversityId),
			"UniversityIdDesc" => students.OrderByDescending(s => s.UniversityId),
			"Rut" => students.OrderBy(s => s.Rut),
			"RutDesc" => students.OrderByDescending(s => s.Rut),
			"Email" => students.OrderBy(s => s.Email),
			"EmailDesc" => students.OrderByDescending(s => s.Email),
			"LastName" => students.OrderBy(s => s.LastName),
			"LastNameDesc" => students.OrderByDescending(s => s.LastName),
			_ => students.OrderBy(s => s.LastName)
		};
		var filteredAndOrderedStudents = orderedStudents.ToList();
		if (!string.IsNullOrEmpty(searchString)) {
			filteredAndOrderedStudents = orderedStudents.Where(s => s.FirstName!.ToUpper().Contains(searchString.ToUpper()) || s.LastName!.ToUpper().Contains(searchString.ToUpper()) || s.UniversityId!.ToUpper().Contains(searchString) || s.Rut!.ToUpper().Contains(searchString.ToUpper()) || s.Email == searchString).ToList();
		}
		var indexViewModels = filteredAndOrderedStudents.Select(s => new IndexViewModel {
			Id = s.Id,
			FirstName = s.FirstName,
			LastName = s.LastName,
			UniversityId = s.UniversityId,
			Rut = s.Rut,
			Email = s.Email
		});
		var pageSize = 6;
		return this.View(PaginatedList<IndexViewModel>.Create(indexViewModels.AsQueryable(), pageNumber ?? 1, pageSize));
	}

	public IActionResult Create() => this.View();

	[HttpPost]
	public async Task<IActionResult> Create([FromForm] CreateViewModel model) {
		try {
			var errorMessages = new List<string>();
			var warningMessages = new List<string>();
			var successMessages = new List<string>();
			using var reader = new StreamReader(model.CsvFile!.OpenReadStream());
			using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
			var records = csv.GetRecords<CreateModel>();
			foreach (var record in records) {
				var user = new ApplicationUser {
					FirstName = record.FirstName,
					LastName = record.LastName,
					UniversityId = record.UniversityId,
					Rut = record.Rut,
				};
				await this._userStore.SetUserNameAsync(user, record.Email, CancellationToken.None);
				await this._emailStore.SetEmailAsync(user, record.Email, CancellationToken.None);
				var result = await this._userManager.CreateAsync(user, record!.Password!);
				if (!result.Succeeded) {
					errorMessages.Add($"Error al crear al estudiante {record.Email}");
				} else {
					var roleResult = await this._userManager.AddToRoleAsync(user, Roles.Student.ToString());
					if (roleResult.Succeeded) {
						successMessages.Add($"Estudiante {record.Email} creado correctamente.");
					} else {
						warningMessages.Add($"Estudiante {record.Email} creado, pero no se le pudo asignar el rol.");
					}
				}
			}
			if (errorMessages.Any()) {
				this.ViewBag.ErrorMessages = errorMessages;
			}
			if (warningMessages.Any()) {
				this.ViewBag.WarningMessages = warningMessages;
			}
			if (successMessages.Any()) {
				this.ViewBag.SuccessMessages = successMessages;
			}
			return this.View();
		} catch {
			this.ViewBag.ErrorMessage = "Error al importar el archivo CSV.";
			return this.View();
		}
	}

	public async Task<IActionResult> Edit(string id) {
		var student = await this._userManager.FindByIdAsync(id);
		if (student is null) {
			this.ViewBag.ErrorMessage = "Error al obtener al estudiante.";
			return this.View();
		}
		var editViewModel = new EditViewModel {
			FirstName = student.FirstName,
			LastName = student.LastName,
			UniversityId = student.UniversityId,
			Rut = student.Rut,
			Email = student.Email
		};
		return this.View(editViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit(string id, [FromForm] EditViewModel model) {
		var student = await this._userManager.FindByIdAsync(id);
		if (student is null) {
			this.ViewBag.ErrorMessage = "Error al obtener al estudiante.";
			return this.View();
		}
		await this._userStore.SetUserNameAsync(student, model.Email, CancellationToken.None);
		await this._emailStore.SetEmailAsync(student, model.Email, CancellationToken.None);
		student.FirstName = !model.FirstName.IsNullOrEmpty() ? model.FirstName : student.FirstName;
		student.LastName = !model.LastName.IsNullOrEmpty() ? model.LastName : student.LastName;
		student.UniversityId = !model.UniversityId.IsNullOrEmpty() ? model.UniversityId : student.UniversityId;
		student.Rut = !model.Rut.IsNullOrEmpty() ? model.Rut : student.Rut;
		var updateResult = await this._userManager.UpdateAsync(student);
		if (updateResult.Succeeded) {
			this.ViewBag.SuccessMessage = "Estudiante actualizado con éxito.";
			return this.View();
		}
		if (updateResult.Errors.Any()) {
			this.ViewBag.ErrorMessages = updateResult.Errors.Select(e => e.Description).ToList();
		}
		this.ViewBag.ErrorMessage = "Error al actualizar al estudiante.";
		return this.View();
	}

	public async Task<IActionResult> Delete(string id) {
		var student = await this._userManager.FindByIdAsync(id);
		if (student is null) {
			this.ViewBag.ErrorMessage = "Error al obtener al estudiante.";
			return this.View();
		}
		var deleteViewModel = new DeleteViewModel {
			Id = student.Id,
			Email = student.Email
		};
		return this.View(deleteViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete(string id, [FromForm] DeleteViewModel model) {
		var student = await this._userManager.FindByIdAsync(id);
		if (student is null) {
			this.ViewBag.ErrorMessage = "Error al obtener al estudiante.";
			return this.View();
		}
		if (student!.Id == this._userManager.GetUserId(this.User)) {
			this.ViewBag.ErrorMessage = "No te puedes eliminar a tí mismo.";
			return this.View();
		}
		var result = await this._userManager.DeleteAsync(student);
		if (result.Succeeded) {
			this.ViewBag.SuccessMessage = "Estudiante eliminado con éxito.";
			return this.View();
		}
		this.ViewBag.ErrorMessage = "Error al eliminar al estudiante.";
		this.ViewBag.ErrorMessages = result.Errors.Select(e => e.Description).ToList();
		return this.View();
	}
}
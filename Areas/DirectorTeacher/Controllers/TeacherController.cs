using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Utal.Icc.Sgm.Areas.DirectorTeacher.Views.Teacher;
using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;

using static Utal.Icc.Sgm.Models.ApplicationUser;

namespace Utal.Icc.Sgm.Areas.DirectorTeacher.Controllers;

[Area("DirectorTeacher"), Authorize(Roles = "DirectorTeacher")]
public class TeacherController : Controller {
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly IUserStore<ApplicationUser> _userStore;
	private readonly IUserEmailStore<ApplicationUser> _emailStore;

	public TeacherController(UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore) {
		this._userManager = userManager;
		this._userStore = userStore;
		this._emailStore = (IUserEmailStore<ApplicationUser>)this._userStore;
	}

	public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		var teacherSession = await this._userManager.GetUserAsync(this.User);
		if (teacherSession is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (teacherSession.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		this.ViewData["FirstNameSortParam"] = sortOrder == "FirstName" ? "FirstNameDesc" : "FirstName";
		this.ViewData["LastNameSortParam"] = sortOrder == "LastName" ? "LastNameDesc" : "LastName";
		this.ViewData["UniversityIdSortParam"] = sortOrder == "StudentUniversityId" ? "UniversityIdDesc" : "StudentUniversityId";
		this.ViewData["RutSortParam"] = sortOrder == "Rut" ? "RutDesc" : "Rut";
		this.ViewData["EmailSortParam"] = sortOrder == "Email" ? "EmailDesc" : "Email";
		this.ViewData["CurrentSort"] = sortOrder;
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		var teachers = await this._userManager.GetUsersInRoleAsync(Roles.Teacher.ToString());
		var orderedTeachers = sortOrder switch {
			"FirstName" => teachers.OrderBy(t => t.FirstName),
			"FirstNameDesc" => teachers.OrderByDescending(t => t.FirstName),
			"Rut" => teachers.OrderBy(t => t.Rut),
			"RutDesc" => teachers.OrderByDescending(t => t.Rut),
			"Email" => teachers.OrderBy(t => t.Email),
			"EmailDesc" => teachers.OrderByDescending(t => t.Email),
			"LastName" => teachers.OrderBy(t => t.LastName),
			"LastNameDesc" => teachers.OrderByDescending(t => t.LastName),
			_ => teachers.OrderBy(t => t.LastName)
		};
		var filteredAndOrderedTeachers = orderedTeachers.ToList();
		if (!string.IsNullOrEmpty(searchString)) {
			filteredAndOrderedTeachers = orderedTeachers
				.Where(
					t => t.FirstName!.ToUpper().Contains(searchString.ToUpper())
					|| t.LastName!.ToUpper().Contains(searchString.ToUpper())
					|| t.Rut!.ToUpper().Contains(searchString.ToUpper())
					|| t.Email == searchString)
				.ToList();
		}
		var indexViewModels = filteredAndOrderedTeachers.Select(async t => new IndexViewModel {
			Id = t.Id,
			FirstName = t.FirstName,
			LastName = t.LastName,
			Rut = t.Rut,
			Email = t.Email,
			IsDirectorTeacher = await this._userManager.IsInRoleAsync(t, Roles.DirectorTeacher.ToString()),
			IsDeactivated = t.IsDeactivated
		}).Select(t => t.Result);
		var pageSize = 6;
		return this.View(PaginatedList<IndexViewModel>.Create((await this._userManager.GetUserAsync(this.User))!.Id, indexViewModels.AsQueryable(), pageNumber ?? 1, pageSize));
	}

	public async Task<IActionResult> Create() {
		var teacherSession = await this._userManager.GetUserAsync(this.User);
		return teacherSession is null
			? this.RedirectToAction("Index", "Home", new { area = "" })
			: teacherSession.IsDeactivated ? this.RedirectToAction("Index", "Home", new { area = "" }) : this.View(new CreateViewModel());
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Create([FromForm] CreateViewModel model) {
		var teacherSession = await this._userManager.GetUserAsync(this.User);
		if (teacherSession is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (teacherSession.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (!this.ModelState.IsValid) {
			this.ViewBag.WarningMessage = "Revisa que los campos estén correctos.";
			return this.View(model);
		}
		var teacher = new ApplicationUser {
			FirstName = model.FirstName,
			LastName = model.LastName,
			Rut = model.Rut,
			CreatedAt = DateTimeOffset.Now,
			UpdatedAt = DateTimeOffset.Now
		};
		await this._userStore.SetUserNameAsync(teacher, model.Email, CancellationToken.None);
		await this._emailStore.SetEmailAsync(teacher, model.Email, CancellationToken.None);
		_ = await this._userManager.CreateAsync(teacher, model.Password!);
		_ = await this._userManager.AddToRoleAsync(teacher, Roles.Teacher.ToString());
		var rankRoles = new List<string>();
		if (model.IsGuideTeacher) {
			rankRoles.Add(Roles.GuideTeacher.ToString());
		}
		if (model.IsAssistantTeacher) {
			rankRoles.Add(Roles.AssistantTeacher.ToString());
		}
		if (model.IsCourseTeacher) {
			rankRoles.Add(Roles.CourseTeacher.ToString());
		}
		if (model.IsCommitteeTeacher) {
			rankRoles.Add(Roles.CommitteeTeacher.ToString());
		}
		_ = await this._userManager.AddToRolesAsync(teacher, rankRoles);
		this.TempData["SuccessMessage"] = "Profesor creado correctamente.";
		return this.RedirectToAction("Index", "Teacher", new { area = "DirectorTeacher" });
	}

	public async Task<IActionResult> Edit(string id) {
		var teacherSession = await this._userManager.GetUserAsync(this.User);
		if (teacherSession is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (teacherSession.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var teacher = await this._userManager.FindByIdAsync(id);
		if (teacher is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al profesor.";
			return this.RedirectToAction("Index", "Teacher", new { area = "DirectorTeacher" });
		}
		if (teacher.IsDeactivated) {
			this.TempData["ErrorMessage"] = "El profesor está desactivado.";
			return this.RedirectToAction("Index", "Teacher", new { area = "DirectorTeacher" });
		}
		var editViewModel = new EditViewModel {
			Id = id,
			FirstName = teacher.FirstName,
			LastName = teacher.LastName,
			Rut = teacher.Rut,
			Email = teacher.Email,
			CreatedAt = teacher.CreatedAt,
			UpdatedAt = teacher.UpdatedAt
		};
		var roles = (await this._userManager.GetRolesAsync(teacher)).ToList();
		editViewModel.IsGuideTeacher = roles.Contains(Roles.GuideTeacher.ToString());
		editViewModel.IsAssistantTeacher = roles.Contains(Roles.AssistantTeacher.ToString());
		editViewModel.IsCourseTeacher = roles.Contains(Roles.CourseTeacher.ToString());
		editViewModel.IsCommitteeTeacher = roles.Contains(Roles.CommitteeTeacher.ToString());
		return this.View(editViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit([FromForm] EditViewModel model) {
		var teacherSession = await this._userManager.GetUserAsync(this.User);
		if (teacherSession is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (teacherSession.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var teacher = await this._userManager.FindByIdAsync(model.Id!.ToString()!);
		if (teacher is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al profesor.";
			return this.RedirectToAction("Index", "Teacher", new { area = "DirectorTeacher" });
		}
		if (teacher.IsDeactivated) {
			this.TempData["ErrorMessage"] = "El profesor está desactivado.";
			return this.RedirectToAction("Index", "Teacher", new { area = "DirectorTeacher" });
		}
		var roles = (await this._userManager.GetRolesAsync(teacher)).ToList();
		await this._userStore.SetUserNameAsync(teacher, userName: model.Email, CancellationToken.None);
		await this._emailStore.SetEmailAsync(teacher, model.Email, CancellationToken.None);
		teacher.FirstName = model.FirstName;
		teacher.LastName = model.LastName;
		teacher.Rut = model.Rut;
		teacher.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(teacher);
		if (roles.Contains(Roles.Teacher.ToString())) {
			_ = roles.Remove(Roles.Teacher.ToString());
		}
		if (roles.Contains(Roles.DirectorTeacher.ToString())) {
			_ = roles.Remove(Roles.DirectorTeacher.ToString());
		}
		var removeRankRolesResult = await this._userManager.RemoveFromRolesAsync(teacher, roles);
		var rankRoles = new List<string>();
		if (model.IsGuideTeacher) {
			rankRoles.Add(Roles.GuideTeacher.ToString());
		}
		if (model.IsAssistantTeacher) {
			rankRoles.Add(Roles.AssistantTeacher.ToString());
		}
		if (model.IsCourseTeacher) {
			rankRoles.Add(Roles.CourseTeacher.ToString());
		}
		if (model.IsCommitteeTeacher) {
			rankRoles.Add(Roles.CommitteeTeacher.ToString());
		}
		var rankRolesResult = await this._userManager.AddToRolesAsync(teacher, rankRoles);
		if (removeRankRolesResult.Succeeded && rankRolesResult.Succeeded) {
			var editViewModel1 = new EditViewModel {
				FirstName = teacher.FirstName,
				LastName = teacher.LastName,
				Rut = teacher.Rut,
				Email = teacher.Email,
				CreatedAt = teacher.CreatedAt,
				UpdatedAt = teacher.UpdatedAt
			};
			roles = (await this._userManager.GetRolesAsync(teacher)).ToList();
			editViewModel1.IsGuideTeacher = roles.Contains(Roles.GuideTeacher.ToString());
			editViewModel1.IsAssistantTeacher = roles.Contains(Roles.AssistantTeacher.ToString());
			editViewModel1.IsCourseTeacher = roles.Contains(Roles.CourseTeacher.ToString());
			editViewModel1.IsCommitteeTeacher = roles.Contains(Roles.CommitteeTeacher.ToString());
			this.ViewBag.SuccessMessage = "Profesor actualizado correctamente.";
			return this.View(editViewModel1);
		}
		var editViewModel = new EditViewModel {
			Id = teacher.Id,
			FirstName = teacher.FirstName,
			LastName = teacher.LastName,
			Rut = teacher.Rut,
			Email = teacher.Email,
			CreatedAt = teacher.CreatedAt,
			UpdatedAt = teacher.UpdatedAt
		};
		roles = (await this._userManager.GetRolesAsync(teacher)).ToList();
		editViewModel.IsGuideTeacher = roles.Contains(Roles.GuideTeacher.ToString());
		editViewModel.IsAssistantTeacher = roles.Contains(Roles.AssistantTeacher.ToString());
		editViewModel.IsCourseTeacher = roles.Contains(Roles.CourseTeacher.ToString());
		editViewModel.IsCommitteeTeacher = roles.Contains(Roles.CommitteeTeacher.ToString());
		this.TempData["WarningMessage"] = "Profesor actualizado, pero no se le pudo asignar el(los) rol(es).";
		this.TempData["WarningMessages1"] = removeRankRolesResult.Errors.Select(w => w.Description).ToList();
		this.TempData["WarningMessages2"] = rankRolesResult.Errors.Select(w => w.Description).ToList();
		return this.View(editViewModel);
	}

	public async Task<IActionResult> ToggleActivation(string id) {
		var teacherSession = await this._userManager.GetUserAsync(this.User);
		if (teacherSession is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (teacherSession.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var teacher = await this._userManager.FindByIdAsync(id);
		if (teacher is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al profesor.";
			return this.RedirectToAction("Index", "Teacher", new { area = "DirectorTeacher" });
		}
		var toggleActivationModel = new ToggleActivationViewModel {
			Id = teacher.Id,
			Email = teacher.Email,
			IsDeactivated = teacher.IsDeactivated
		};
		return this.View(toggleActivationModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> ToggleActivation([FromForm] ToggleActivationViewModel model) {
		var teacherSession = await this._userManager.GetUserAsync(this.User);
		if (teacherSession is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (teacherSession.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var teacher = await this._userManager.FindByIdAsync(model.Id!.ToString()!);
		if (teacher is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al profesor.";
			return this.RedirectToAction("Index", "Teacher", new { area = "DirectorTeacher" });
		}
		if (teacher!.Id == this._userManager.GetUserId(this.User)) {
			this.TempData["ErrorMessage"] = !model.IsDeactivated ? "No te puedes desactivar a tí mismo." : "¡No deberías haber llegado a este punto!";
			return this.RedirectToAction("Index", "Teacher", new { area = "DirectorTeacher" });
		}
		var roles = (await this._userManager.GetRolesAsync(teacher)).ToList();
		if (roles.Contains(Roles.DirectorTeacher.ToString())) {
			this.TempData["ErrorMessage"] = !model.IsDeactivated ? "No puedes desactivar al director de carrera actual." : "¡No deberías haber llegado a este punto!";
			return this.RedirectToAction("Index", "Teacher", new { area = "DirectorTeacher" });
		}
		teacher.IsDeactivated = !model.IsDeactivated;
		teacher.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(teacher);
		this.TempData["SuccessMessage"] = !model.IsDeactivated ? "Profesor desactivado correctamente." : "Profesor activado correctamente.";
		return this.RedirectToAction("Index", "Teacher", new { area = "DirectorTeacher" });
	}

	public async Task<IActionResult> Transfer(string currentDirectorTeacherId, string newDirectorTeacherId) {
		var teacherSession = await this._userManager.GetUserAsync(this.User);
		if (teacherSession is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (teacherSession.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var currentDirectorTeacher = await this._userManager.FindByIdAsync(currentDirectorTeacherId);
		var newDirectorTeacher = await this._userManager.FindByIdAsync(newDirectorTeacherId);
		if (currentDirectorTeacher is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al profesor fuente.";
			return this.RedirectToAction("Index", "Teacher", new { area = "DirectorTeacher" });
		}
		if (currentDirectorTeacher.IsDeactivated) {
			this.TempData["ErrorMessage"] = "El profesor fuente está desactivado.";
			return this.RedirectToAction("Index", "Teacher", new { area = "DirectorTeacher" });
		}
		if (newDirectorTeacher is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al profesor objetivo.";
			return this.RedirectToAction("Index", "Teacher", new { area = "DirectorTeacher" });
		}
		if (newDirectorTeacher.IsDeactivated) {
			this.TempData["ErrorMessage"] = "El profesor objetivo está desactivado.";
			return this.RedirectToAction("Index", "Teacher", new { area = "DirectorTeacher" });
		}
		var transferViewModel = new TransferViewModel {
			CurrentDirectorTeacherId = currentDirectorTeacher.Id,
			NewDirectorTeacherId = newDirectorTeacher!.Id,
			NewDirectorTeacherName = $"{newDirectorTeacher.FirstName} {newDirectorTeacher.LastName}"
		};
		return this.View(transferViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Transfer([FromForm] TransferViewModel model) {
		var teacherSession = await this._userManager.GetUserAsync(this.User);
		if (teacherSession is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (teacherSession.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var currentDirectorTeacher = await this._userManager.FindByIdAsync(model.CurrentDirectorTeacherId!);
		if (currentDirectorTeacher is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al director de carrera actual.";
			return this.RedirectToAction("Index", "Teacher", new { area = "DirectorTeacher" });
		}
		if (currentDirectorTeacher.IsDeactivated) {
			this.TempData["ErrorMessage"] = "El director de carrera actual está desactivado, lo cual esto no debería haber pasado.";
			return this.RedirectToAction("Index", "Teacher", new { area = "DirectorTeacher" });
		}
		var currentDirectorTeacherRoles = (await this._userManager.GetRolesAsync(currentDirectorTeacher)).ToList();
		if (!currentDirectorTeacherRoles.Contains(Roles.DirectorTeacher.ToString())) {
			this.TempData["ErrorMessage"] = "El profesor fuente no es director de carrera.";
			return this.RedirectToAction("Index", "Teacher", new { area = "DirectorTeacher" });
		}
		var newDirectorTeacher = await this._userManager.FindByIdAsync(model.NewDirectorTeacherId!);
		if (newDirectorTeacher is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al nuevo director de carrera actual.";
			return this.RedirectToAction("Index", "Teacher", new { area = "DirectorTeacher" });
		}
		if (newDirectorTeacher.IsDeactivated) {
			this.TempData["ErrorMessage"] = "El profesor objetivo está desactivado.";
			return this.RedirectToAction("Index", "Teacher", new { area = "DirectorTeacher" });
		}
		if (currentDirectorTeacher == newDirectorTeacher) {
			this.TempData["ErrorMessage"] = "Ambos profesores involucrados en la transferencia son el mismo.";
			return this.RedirectToAction("Index", "Teacher", new { area = "DirectorTeacher" });
		}
		_ = await this._userManager.RemoveFromRoleAsync(currentDirectorTeacher, Roles.DirectorTeacher.ToString());
		_ = await this._userManager.AddToRoleAsync(newDirectorTeacher, Roles.DirectorTeacher.ToString());
		currentDirectorTeacher.UpdatedAt = DateTimeOffset.Now;
		newDirectorTeacher.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(currentDirectorTeacher);
		this.TempData["SuccessMessage"] = "Director de carrera transferido correctamente.";
		return this.RedirectToAction("Index", "Teacher", new { area = "DirectorTeacher" });
	}
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using Utal.Icc.Sgm.Areas.DirectorTeacher.Views.Teacher;
using Utal.Icc.Sgm.Controllers;
using Utal.Icc.Sgm.Models;

using static Utal.Icc.Sgm.Models.ApplicationUser;

namespace Utal.Icc.Sgm.Areas.DirectorTeacher.Controllers;

[Area(nameof(DirectorTeacher)), Authorize(Roles = nameof(Roles.DirectorTeacher))]
public class TeacherControllerOld : Controller {
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly IUserStore<ApplicationUser> _userStore;
	private readonly IUserEmailStore<ApplicationUser> _emailStore;

	public TeacherControllerOld(UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore) {
		this._userManager = userManager;
		this._userStore = userStore;
		this._emailStore = (IUserEmailStore<ApplicationUser>)this._userStore;
	}

	protected async Task<ApplicationUser> CheckTeacherSession() {
		var teacher = await this._userManager.GetUserAsync(this.User);
		return teacher is null || teacher.IsDeactivated ? null! : teacher;
	}

	protected async Task<ApplicationUser?> CheckApplicationUser(string applicationUserId) {
		var applicationUser = await this._userManager.FindByIdAsync(applicationUserId);
		return applicationUser is null || applicationUser.IsDeactivated ? null : applicationUser;
	}

	protected void SetSortParameters(string sortOrder, params string[] parameters) {
		foreach (var parameter in parameters) {
			this.ViewData[$"{parameter}SortParam"] = sortOrder == parameter ? $"{parameter}Desc" : parameter;
		}
		this.ViewData["CurrentSort"] = sortOrder;
	}

	protected IOrderedEnumerable<ApplicationUser> OrderTeachers(string sortOrder, IEnumerable<ApplicationUser> teachers, params string[] parameters) {
		foreach (var parameter in parameters) {
			if (parameter == sortOrder) {
				return teachers.OrderBy(t => t.GetType().GetProperty(parameter)!.GetValue(t, null));
			} else if ($"{parameter}Desc" == sortOrder) {
				return teachers.OrderByDescending(t => t.GetType().GetProperty(parameter)!.GetValue(t, null));
			}
		}
		return teachers.OrderBy(t => t.GetType().GetProperty(parameters[0]));
	}

	protected IEnumerable<IndexViewModel> FilterTeachers(string searchString, IOrderedEnumerable<ApplicationUser> teachers, params string[] parameters) {
		var result = new List<IndexViewModel>();
		foreach (var parameter in parameters) {
			var partials = teachers
					.Where(t => (t.GetType().GetProperty(parameter)!.GetValue(t) as string)!.Contains(searchString))
					.Select(async t => new IndexViewModel {
						Id = t.Id,
						FirstName = t.FirstName,
						LastName = t.LastName,
						Rut = t.Rut,
						Email = t.Email,
						IsDeactivated = t.IsDeactivated,
						IsDirectorTeacher = await this._userManager.IsInRoleAsync(t, nameof(Roles.DirectorTeacher)),
					}).Select(t => t.Result);
			foreach (var partial in partials) {
				if (!result.Any(ivm => ivm.Id == partial.Id)) {
					result.Add(partial);
				}
			}
		}
		return result.AsEnumerable();
	}

	public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		if (await this.CheckTeacherSession() is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var parameters = new[] { nameof(ApplicationUser.FirstName), nameof(ApplicationUser.LastName), nameof(ApplicationUser.Rut), nameof(ApplicationUser.Email) };
		this.SetSortParameters(sortOrder, parameters);
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		var teachers = await this._userManager.GetUsersInRoleAsync(nameof(Roles.Teacher));
		var orderedTeachers = this.OrderTeachers(sortOrder, teachers, parameters);
		var indexViewModels = !searchString.IsNullOrEmpty() ? this.FilterTeachers(searchString, orderedTeachers, parameters) : orderedTeachers.Select(async t => new IndexViewModel {
			Id = t.Id,
			FirstName = t.FirstName,
			LastName = t.LastName,
			Rut = t.Rut,
			Email = t.Email,
			IsDirectorTeacher = await this._userManager.IsInRoleAsync(t, nameof(Roles.DirectorTeacher)),
			IsDeactivated = t.IsDeactivated
		}).Select(t => t.Result).ToList();
		return this.View(PaginatedList<IndexViewModel>.Create(indexViewModels.AsQueryable(), pageNumber ?? 1, 6));
	}

	public async Task<IActionResult> Create() => await this.CheckTeacherSession() is not ApplicationUser teacher
			? this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty })
			: teacher.IsDeactivated ? this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty }) : this.View(new CreateViewModel());

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Create([FromForm] CreateViewModel model) {
		if (!this.ModelState.IsValid) {
			this.ViewBag.WarningMessage = "Revisa que los campos estén correctos.";
			return this.View(model);
		}
		if (await this.CheckTeacherSession() is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
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
		_ = await this._userManager.AddToRoleAsync(teacher, nameof(Roles.Teacher));
		var rankRoles = new List<string>();
		if (model.IsGuideTeacher) {
			rankRoles.Add(nameof(Roles.GuideTeacher));
		}
		if (model.IsAssistantTeacher) {
			rankRoles.Add(nameof(Roles.AssistantTeacher));
		}
		if (model.IsCourseTeacher) {
			rankRoles.Add(nameof(Roles.CourseTeacher));
		}
		if (model.IsCommitteeTeacher) {
			rankRoles.Add(nameof(Roles.CommitteeTeacher));
		}
		_ = await this._userManager.AddToRolesAsync(teacher, rankRoles);
		this.TempData["SuccessMessage"] = "Profesor creado correctamente.";
		return this.RedirectToAction(nameof(TeacherController.Index), nameof(TeacherController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
	}

	public async Task<IActionResult> Edit(string id) {
		if (await this.CheckTeacherSession() is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		if (await this.CheckApplicationUser(id) is not ApplicationUser teacher) {
			this.TempData["ErrorMessage"] = "Error al obtener al profesor.";
			return this.RedirectToAction(nameof(TeacherController.Index), nameof(TeacherController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
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
		editViewModel.IsGuideTeacher = roles.Contains(nameof(Roles.GuideTeacher));
		editViewModel.IsAssistantTeacher = roles.Contains(nameof(Roles.AssistantTeacher));
		editViewModel.IsCourseTeacher = roles.Contains(nameof(Roles.CourseTeacher));
		editViewModel.IsCommitteeTeacher = roles.Contains(nameof(Roles.CommitteeTeacher));
		return this.View(editViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit([FromForm] EditViewModel model) {
		if (await this.CheckTeacherSession() is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		if (await this.CheckApplicationUser(model.Id!) is not ApplicationUser teacher) {
			this.TempData["ErrorMessage"] = "Error al obtener al profesor.";
			return this.RedirectToAction(nameof(TeacherController.Index), nameof(TeacherController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
		}
		var roles = (await this._userManager.GetRolesAsync(teacher)).ToList();
		await this._userStore.SetUserNameAsync(teacher, userName: model.Email, CancellationToken.None);
		await this._emailStore.SetEmailAsync(teacher, model.Email, CancellationToken.None);
		teacher.FirstName = model.FirstName;
		teacher.LastName = model.LastName;
		teacher.Rut = model.Rut;
		teacher.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(teacher);
		if (roles.Contains(nameof(Roles.Teacher))) {
			_ = roles.Remove(nameof(Roles.Teacher));
		}
		if (roles.Contains(nameof(Roles.DirectorTeacher))) {
			_ = roles.Remove(nameof(Roles.DirectorTeacher));
		}
		var removeRankRolesResult = await this._userManager.RemoveFromRolesAsync(teacher, roles);
		var rankRoles = new List<string>();
		if (model.IsGuideTeacher) {
			rankRoles.Add(nameof(Roles.GuideTeacher));
		}
		if (model.IsAssistantTeacher) {
			rankRoles.Add(nameof(Roles.AssistantTeacher));
		}
		if (model.IsCourseTeacher) {
			rankRoles.Add(nameof(Roles.CourseTeacher));
		}
		if (model.IsCommitteeTeacher) {
			rankRoles.Add(nameof(Roles.CommitteeTeacher));
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
			editViewModel1.IsGuideTeacher = roles.Contains(nameof(Roles.GuideTeacher));
			editViewModel1.IsAssistantTeacher = roles.Contains(nameof(Roles.AssistantTeacher));
			editViewModel1.IsCourseTeacher = roles.Contains(nameof(Roles.CourseTeacher));
			editViewModel1.IsCommitteeTeacher = roles.Contains(nameof(Roles.CommitteeTeacher));
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
		editViewModel.IsGuideTeacher = roles.Contains(nameof(Roles.GuideTeacher));
		editViewModel.IsAssistantTeacher = roles.Contains(nameof(Roles.AssistantTeacher));
		editViewModel.IsCourseTeacher = roles.Contains(nameof(Roles.CourseTeacher));
		editViewModel.IsCommitteeTeacher = roles.Contains(nameof(Roles.CommitteeTeacher));
		this.ViewBag.WarningMessage = "Profesor actualizado, pero no se le pudo asignar el(los) rol(es).";
		return this.View(editViewModel);
	}

	public async Task<IActionResult> ToggleActivation(string id) {
		if (await this.CheckTeacherSession() is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var teacher = await this._userManager.FindByIdAsync(id);
		if (teacher is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al profesor.";
			return this.RedirectToAction(nameof(TeacherController.Index), nameof(TeacherController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
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
		if (await this.CheckTeacherSession() is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var teacher = await this._userManager.FindByIdAsync(model.Id!);
		if (teacher is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al profesor.";
			return this.RedirectToAction(nameof(TeacherController.Index), nameof(TeacherController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
		}
		if (teacher!.Id == this._userManager.GetUserId(this.User)) {
			this.TempData["ErrorMessage"] = !model.IsDeactivated ? "No te puedes desactivar a tí mismo." : "¡No deberías haber llegado a este punto!";
			return this.RedirectToAction(nameof(TeacherController.Index), nameof(TeacherController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
		}
		var roles = (await this._userManager.GetRolesAsync(teacher)).ToList();
		if (roles.Contains(nameof(Roles.DirectorTeacher))) {
			this.TempData["ErrorMessage"] = !model.IsDeactivated ? "No puedes desactivar al director de carrera actual." : "¡No deberías haber llegado a este punto!";
			return this.RedirectToAction(nameof(TeacherController.Index), nameof(TeacherController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
		}
		teacher.IsDeactivated = !model.IsDeactivated;
		teacher.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(teacher);
		this.TempData["SuccessMessage"] = !model.IsDeactivated ? "Profesor desactivado correctamente." : "Profesor activado correctamente.";
		return this.RedirectToAction(nameof(TeacherController.Index), nameof(TeacherController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
	}

	public async Task<IActionResult> Transfer(string currentDirectorTeacherId, string newDirectorTeacherId) {
		if (await this.CheckTeacherSession() is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var currentDirectorTeacher = await this._userManager.FindByIdAsync(currentDirectorTeacherId);
		var newDirectorTeacher = await this._userManager.FindByIdAsync(newDirectorTeacherId);
		var check = (currentDirectorTeacher, newDirectorTeacher) switch {
			(ApplicationUser, ApplicationUser) => true,
			(ApplicationUser teacher, _) when teacher.IsDeactivated => false,
			(_, ApplicationUser teacher) when teacher.IsDeactivated => false,
			_ => false
		};
		if (!check) {
			this.TempData["ErrorMessage"] = "Revisa los profesores fuente y objetivo antes de hacer la transferencia.";
			return this.RedirectToAction(nameof(TeacherController.Index), nameof(TeacherController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
		}
		var transferViewModel = new TransferViewModel {
			CurrentDirectorTeacherId = currentDirectorTeacher!.Id,
			NewDirectorTeacherId = newDirectorTeacher!.Id,
			NewDirectorTeacherName = $"{newDirectorTeacher.FirstName} {newDirectorTeacher.LastName}"
		};
		return this.View(transferViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Transfer([FromForm] TransferViewModel model) {
		var teacherSession = await this._userManager.GetUserAsync(this.User);
		if (teacherSession is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		if (teacherSession.IsDeactivated) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		if (await this.CheckApplicationUser(model.CurrentDirectorTeacherId!) is not ApplicationUser currentDirectorTeacher) {
			this.TempData["ErrorMessage"] = "Error al obtener al profesor fuente.";
			return this.RedirectToAction(nameof(TeacherController.Index), nameof(TeacherController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
		}
		var currentDirectorTeacherRoles = (await this._userManager.GetRolesAsync(currentDirectorTeacher)).ToList();
		if (!currentDirectorTeacherRoles.Contains(nameof(Roles.DirectorTeacher))) {
			this.TempData["ErrorMessage"] = "El profesor fuente no es director de carrera.";
			return this.RedirectToAction(nameof(TeacherController.Index), nameof(TeacherController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
		}
		if (await this.CheckApplicationUser(model.NewDirectorTeacherId!) is not ApplicationUser newDirectorTeacher) {
			this.TempData["ErrorMessage"] = "Error al obtener al profesor objetivo.";
			return this.RedirectToAction(nameof(TeacherController.Index), nameof(TeacherController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
		}
		if (currentDirectorTeacher == newDirectorTeacher) {
			this.TempData["ErrorMessage"] = "Ambos profesores involucrados en la transferencia son el mismo.";
			return this.RedirectToAction(nameof(TeacherController.Index), nameof(TeacherController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
		}
		_ = await this._userManager.RemoveFromRoleAsync(currentDirectorTeacher, nameof(Roles.DirectorTeacher));
		_ = await this._userManager.AddToRoleAsync(newDirectorTeacher, nameof(Roles.DirectorTeacher));
		currentDirectorTeacher.UpdatedAt = DateTimeOffset.Now;
		newDirectorTeacher.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(currentDirectorTeacher);
		this.TempData["SuccessMessage"] = "Director de carrera transferido correctamente.";
		return this.RedirectToAction(nameof(TeacherController.Index), nameof(TeacherController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
	}
}
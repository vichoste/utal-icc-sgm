using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using Utal.Icc.Sgm.Areas.DirectorTeacher.ViewModels.Teacher;
using Utal.Icc.Sgm.Controllers;
using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;
using Utal.Icc.Sgm.ViewModels;

using static Utal.Icc.Sgm.Models.ApplicationUser;

namespace Utal.Icc.Sgm.Areas.DirectorTeacher.Controllers;

[Area(nameof(DirectorTeacher)), Authorize(Roles = nameof(Roles.DirectorTeacher))]
public class TeacherController : ApplicationController {
	public TeacherController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore, SignInManager<ApplicationUser> signInManager) : base(dbContext, userManager, userStore, signInManager) { }

	protected IEnumerable<IndexTeacherViewModel> Filter(string searchString, IOrderedEnumerable<IndexTeacherViewModel> viewModels, params string[] parameters) {
		var result = new List<IndexTeacherViewModel>();
		foreach (var parameter in parameters) {
			var partials = viewModels
					.Where(vm => (vm.GetType().GetProperty(parameter)!.GetValue(vm) as string)!.Contains(searchString));
			foreach (var partial in partials) {
				if (!result.Any(vm => vm.Id == partial.Id)) {
					result.Add(partial);
				}
			}
		}
		return result.AsEnumerable();
	}

	protected IOrderedEnumerable<IndexTeacherViewModel> Sort(string sortOrder, IEnumerable<IndexTeacherViewModel> viewModels, params string[] parameters) {
		foreach (var parameter in parameters) {
			if (parameter == sortOrder) {
				return viewModels.OrderBy(vm => vm.GetType().GetProperty(parameter)!.GetValue(vm, null));
			} else if ($"{parameter}Desc" == sortOrder) {
				return viewModels.OrderByDescending(vm => vm.GetType().GetProperty(parameter)!.GetValue(vm, null));
			}
		}
		return viewModels.OrderBy(vm => vm.GetType().GetProperty(parameters[0]));
	}

	public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		if (await base.CheckSession() is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var parameters = new[] { nameof(ApplicationUserViewModel.FirstName), nameof(ApplicationUserViewModel.LastName), nameof(ApplicationUserViewModel.Rut), nameof(ApplicationUserViewModel.Email) };
		this.SetSortParameters(sortOrder, parameters);
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		var users = (await this._userManager.GetUsersInRoleAsync(nameof(Roles.Teacher))).Select(
			async u => new IndexTeacherViewModel {
				Id = u.Id,
				FirstName = u.FirstName,
				LastName = u.LastName,
				Rut = u.Rut,
				Email = u.Email,
				IsDeactivated = u.IsDeactivated,
				IsDirectorTeacher = await this._userManager.IsInRoleAsync(u, nameof(Roles.DirectorTeacher)),
			}
		).Select(u => u.Result).AsEnumerable();
		var ordered = this.Sort(sortOrder, users, parameters);
		var output = !searchString.IsNullOrEmpty() ? this.Filter(searchString, ordered!, parameters) : ordered;
		return this.View(PaginatedList<IndexTeacherViewModel>.Create(output!.AsQueryable(), pageNumber ?? 1, 6));
	}

	public async Task<IActionResult> Create() => await base.CheckSession() is not ApplicationUser user
			? this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty })
			: user.IsDeactivated ? this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty }) : this.View(new CreateTeacherViewModel());

	[HttpPost]
	public async Task<IActionResult> Create([FromForm] CreateTeacherViewModel input) {
		if (await base.CheckSession() is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var teacher = new ApplicationUser {
			FirstName = input.FirstName,
			LastName = input.LastName,
			Rut = input.Rut,
			CreatedAt = DateTimeOffset.Now,
			UpdatedAt = DateTimeOffset.Now
		};
		await this._userStore.SetUserNameAsync(teacher, input.Email, CancellationToken.None);
		await this._emailStore.SetEmailAsync(teacher, input.Email, CancellationToken.None);
		_ = await this._userManager.CreateAsync(teacher, input.Password!);
		_ = await this._userManager.AddToRoleAsync(teacher, nameof(Roles.Teacher));
		var rankRoles = new List<string>();
		if (input.IsGuideTeacher) {
			rankRoles.Add(nameof(Roles.GuideTeacher));
		}
		if (input.IsAssistantTeacher) {
			rankRoles.Add(nameof(Roles.AssistantTeacher));
		}
		if (input.IsCourseTeacher) {
			rankRoles.Add(nameof(Roles.CourseTeacher));
		}
		if (input.IsCommitteeTeacher) {
			rankRoles.Add(nameof(Roles.CommitteeTeacher));
		}
		_ = await this._userManager.AddToRolesAsync(teacher, rankRoles);
		this.TempData["SuccessMessage"] = "Profesor creado correctamente.";
		return this.RedirectToAction(nameof(TeacherController.Index), nameof(TeacherController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
	}

	public async Task<IActionResult> Edit(string id) {
		if (await base.CheckSession() is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		if (await this.CheckApplicationUser(id) is not ApplicationUser user) {
			this.TempData["ErrorMessage"] = "Error al obtener al profesor.";
			return this.RedirectToAction(nameof(TeacherController.Index), nameof(TeacherController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
		}
		var output = new EditTeacherViewModel {
			Id = id,
			FirstName = user.FirstName,
			LastName = user.LastName,
			Rut = user.Rut,
			Email = user.Email,
			CreatedAt = user.CreatedAt,
			UpdatedAt = user.UpdatedAt,
			IsAssistantTeacher = await this._userManager.IsInRoleAsync(user, nameof(Roles.AssistantTeacher)),
			IsCommitteeTeacher = await this._userManager.IsInRoleAsync(user, nameof(Roles.CommitteeTeacher)),
			IsCourseTeacher = await this._userManager.IsInRoleAsync(user, nameof(Roles.CourseTeacher)),
			IsGuideTeacher = await this._userManager.IsInRoleAsync(user, nameof(Roles.GuideTeacher)),
		};
		return this.View(output);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit([FromForm] EditTeacherViewModel input) {
		if (await base.CheckSession() is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		if (await this.CheckApplicationUser(input.Id!) is not ApplicationUser user) {
			this.TempData["ErrorMessage"] = "Error al obtener al profesor.";
			return this.RedirectToAction(nameof(TeacherController.Index), nameof(TeacherController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
		}
		await this._userStore.SetUserNameAsync(user, input.Email, CancellationToken.None);
		await this._emailStore.SetEmailAsync(user, input.Email, CancellationToken.None);
		user.FirstName = input.FirstName;
		user.LastName = input.LastName;
		user.Rut = input.Rut;
		user.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(user);
		var roles = (await this._userManager.GetRolesAsync(user)).ToList();
		if (roles.Contains(nameof(Roles.Teacher))) {
			_ = roles.Remove(nameof(Roles.Teacher));
		}
		if (roles.Contains(nameof(Roles.DirectorTeacher))) {
			_ = roles.Remove(nameof(Roles.DirectorTeacher));
		}
		var removeRankRolesResult = await this._userManager.RemoveFromRolesAsync(user, roles);
		var rankRoles = new List<string>();
		if (input.IsGuideTeacher) {
			rankRoles.Add(nameof(Roles.GuideTeacher));
		}
		if (input.IsAssistantTeacher) {
			rankRoles.Add(nameof(Roles.AssistantTeacher));
		}
		if (input.IsCourseTeacher) {
			rankRoles.Add(nameof(Roles.CourseTeacher));
		}
		if (input.IsCommitteeTeacher) {
			rankRoles.Add(nameof(Roles.CommitteeTeacher));
		}
		_ = await this._userManager.AddToRolesAsync(user, rankRoles);
		var output = new EditTeacherViewModel {
			Id = user.Id,
			FirstName = user.FirstName,
			LastName = user.LastName,
			Rut = user.Rut,
			Email = user.Email,
			CreatedAt = user.CreatedAt,
			UpdatedAt = user.UpdatedAt,
			IsAssistantTeacher = await this._userManager.IsInRoleAsync(user, nameof(Roles.AssistantTeacher)),
			IsCommitteeTeacher = await this._userManager.IsInRoleAsync(user, nameof(Roles.CommitteeTeacher)),
			IsCourseTeacher = await this._userManager.IsInRoleAsync(user, nameof(Roles.CourseTeacher)),
			IsGuideTeacher = await this._userManager.IsInRoleAsync(user, nameof(Roles.GuideTeacher)),
		};
		this.ViewBag.SuccessMessage = "Profesor actualizado correctamente.";
		return this.View(output);
	}

	public async Task<IActionResult> ToggleActivation(string id) {
		if (await base.CheckSession() is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var user = await this._userManager.FindByIdAsync(id);
		if (user is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al profesor.";
			return this.RedirectToAction(nameof(TeacherController.Index), nameof(TeacherController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
		}
		if (user!.Id == this._userManager.GetUserId(this.User)) {
			this.TempData["ErrorMessage"] = "No te puedes desactivar a tí mismo.";
			return this.RedirectToAction(nameof(TeacherController.Index), nameof(TeacherController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
		}
		var roles = (await this._userManager.GetRolesAsync(user)).ToList();
		if (roles.Contains(nameof(Roles.DirectorTeacher))) {
			this.TempData["ErrorMessage"] = "No puedes desactivar al director de carrera actual.";
			return this.RedirectToAction(nameof(TeacherController.Index), nameof(TeacherController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
		}
		var output = new ApplicationUserViewModel {
			Id = user.Id,
			Email = user.Email,
			IsDeactivated = user.IsDeactivated
		};
		return this.View(output);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> ToggleActivation([FromForm] ApplicationUserViewModel input) {
		if (await base.CheckSession() is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var user = await this._userManager.FindByIdAsync(input.Id!);
		if (user is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al profesor.";
			return this.RedirectToAction(nameof(TeacherController.Index), nameof(TeacherController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
		}
		if (user!.Id == this._userManager.GetUserId(this.User)) {
			this.TempData["ErrorMessage"] = "No te puedes desactivar a tí mismo.";
			return this.RedirectToAction(nameof(TeacherController.Index), nameof(TeacherController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
		}
		var roles = (await this._userManager.GetRolesAsync(user)).ToList();
		if (roles.Contains(nameof(Roles.DirectorTeacher))) {
			this.TempData["ErrorMessage"] = "No puedes desactivar al director de carrera actual.";
			return this.RedirectToAction(nameof(TeacherController.Index), nameof(TeacherController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
		}
		user.IsDeactivated = !input.IsDeactivated;
		user.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(user);
		this.TempData["SuccessMessage"] = !input.IsDeactivated ? "Profesor desactivado correctamente." : "Profesor activado correctamente.";
		return this.RedirectToAction(nameof(TeacherController.Index), nameof(TeacherController).Replace("Controller", string.Empty), new { area = nameof(DirectorTeacher) });
	}

	public async Task<IActionResult> Transfer(string currentDirectorTeacherId, string newDirectorTeacherId) {
		if (await base.CheckSession() is null) {
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
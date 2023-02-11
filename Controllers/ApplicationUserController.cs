using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Utal.Icc.Sgm.Areas.DirectorTeacher.ViewModels.Teacher;
using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;
using Utal.Icc.Sgm.ViewModels;

using static Utal.Icc.Sgm.Models.ApplicationUser;

namespace Utal.Icc.Sgm.Controllers;

public abstract class ApplicationUserController : ApplicationController {
	public ApplicationUserController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore, SignInManager<ApplicationUser> signInManager) : base(dbContext, userManager, userStore, signInManager) { }

	protected async Task<IActionResult> Create<T1, T2>() where T1 : ApplicationUser where T2 : ApplicationUserViewModel, new() => await this.CheckSession() is not T1 user
			? this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty })
			: user.IsDeactivated ? this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty }) : this.View(new T2());


	protected async Task<IActionResult> Create<T1, T2>([FromForm] T1 input, IEnumerable<string> roles, string action, string controller, string area) where T1 : ApplicationUserViewModel where T2 : ApplicationUser, new() {
		if (await this.CheckSession() is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var user = new T2 {
			FirstName = input.FirstName,
			LastName = input.LastName,
			Rut = input.Rut,
			CreatedAt = DateTimeOffset.Now,
			UpdatedAt = DateTimeOffset.Now
		};
		await this._userStore.SetUserNameAsync(user, input.Email, CancellationToken.None);
		await this._emailStore.SetEmailAsync(user, input.Email, CancellationToken.None);
		_ = await this._userManager.CreateAsync(user, input.Password!);
		_ = await this._userManager.AddToRoleAsync(user, nameof(Roles.Teacher));
		_ = await this._userManager.AddToRolesAsync(user, roles);
		this.TempData["SuccessMessage"] = "Usuario creado correctamente.";
		return this.RedirectToAction(action, controller, new { area });
	}

	protected async Task<IActionResult> Edit<T>(string id, string action, string controller, string area) where T : ApplicationUserViewModel, new() {
		if (await this.CheckSession() is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		if (await this.CheckApplicationUser(id) is not ApplicationUser user) {
			this.TempData["ErrorMessage"] = "Error al obtener al usuario.";
			return this.RedirectToAction(action, controller, new { area });
		}
		var output = new T {
			Id = id,
			FirstName = user.FirstName,
			LastName = user.LastName,
			Rut = user.Rut,
			Email = user.Email,
			CreatedAt = user.CreatedAt,
			UpdatedAt = user.UpdatedAt
		};
		if (output is EditTeacherViewModel teacher) {
			teacher.IsAssistantTeacher = await this._userManager.IsInRoleAsync(user, nameof(Roles.AssistantTeacher));
			teacher.IsCommitteeTeacher = await this._userManager.IsInRoleAsync(user, nameof(Roles.CommitteeTeacher));
			teacher.IsCourseTeacher = await this._userManager.IsInRoleAsync(user, nameof(Roles.CourseTeacher));
			teacher.IsGuideTeacher = await this._userManager.IsInRoleAsync(user, nameof(Roles.GuideTeacher));
		}
		return this.View(output);
	}

	protected async Task<IActionResult> Edit<T>([FromForm] T input, string action, string controller, string area) where T : ApplicationUserViewModel, new() {
		if (await this.CheckSession() is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		if (await this.CheckApplicationUser(input.Id!) is not ApplicationUser user) {
			this.TempData["ErrorMessage"] = "Error al obtener al usuario.";
			return this.RedirectToAction(action, controller, new { area });
		}
		await this._userStore.SetUserNameAsync(user, input.Email, CancellationToken.None);
		await this._emailStore.SetEmailAsync(user, input.Email, CancellationToken.None);
		user.FirstName = input.FirstName;
		user.LastName = input.LastName;
		user.Rut = input.Rut;
		user.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(user);
		if (input is EditTeacherViewModel teacher) {
			var roles = (await this._userManager.GetRolesAsync(user)).ToList();
			if (roles.Contains(nameof(Roles.Teacher))) {
				_ = roles.Remove(nameof(Roles.Teacher));
			}
			if (roles.Contains(nameof(Roles.DirectorTeacher))) {
				_ = roles.Remove(nameof(Roles.DirectorTeacher));
			}
			var removeRankRolesResult = await this._userManager.RemoveFromRolesAsync(user, roles);
			var rankRoles = new List<string>();
			if (teacher.IsGuideTeacher) {
				rankRoles.Add(nameof(Roles.GuideTeacher));
			}
			if (teacher.IsAssistantTeacher) {
				rankRoles.Add(nameof(Roles.AssistantTeacher));
			}
			if (teacher.IsCourseTeacher) {
				rankRoles.Add(nameof(Roles.CourseTeacher));
			}
			if (teacher.IsCommitteeTeacher) {
				rankRoles.Add(nameof(Roles.CommitteeTeacher));
			}
			_ = await this._userManager.AddToRolesAsync(user, rankRoles);
		}
		var output = new T {
			Id = user.Id,
			FirstName = user.FirstName,
			LastName = user.LastName,
			Rut = user.Rut,
			Email = user.Email,
			CreatedAt = user.CreatedAt,
			UpdatedAt = user.UpdatedAt
		};
		if (output is EditTeacherViewModel teacher1) {
			teacher1.IsAssistantTeacher = await this._userManager.IsInRoleAsync(user, nameof(Roles.AssistantTeacher));
			teacher1.IsCommitteeTeacher = await this._userManager.IsInRoleAsync(user, nameof(Roles.CommitteeTeacher));
			teacher1.IsCourseTeacher = await this._userManager.IsInRoleAsync(user, nameof(Roles.CourseTeacher));
			teacher1.IsGuideTeacher = await this._userManager.IsInRoleAsync(user, nameof(Roles.GuideTeacher));
		}
		this.ViewBag.SuccessMessage = "Usuario actualizado correctamente.";
		return this.View(output);
	}

	protected async Task<IActionResult> ToggleActivation<T>(string id, string action, string controller, string area) where T : ApplicationUserViewModel, new() {
		if (await this.CheckSession() is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var user = await this._userManager.FindByIdAsync(id);
		if (user is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al usuario.";
			return this.RedirectToAction(action, controller, new { area });
		}
		if (user!.Id == this._userManager.GetUserId(this.User)) {
			this.TempData["ErrorMessage"] = "No te puedes desactivar a tí mismo.";
			return this.RedirectToAction(action, controller, new { area });
		}
		var roles = (await this._userManager.GetRolesAsync(user)).ToList();
		if (roles.Contains(nameof(Roles.DirectorTeacher))) {
			this.TempData["ErrorMessage"] = "No puedes desactivar al director de carrera actual.";
			return this.RedirectToAction(action, controller, new { area });
		}
		var output = new ApplicationUserViewModel {
			Id = user.Id,
			Email = user.Email,
			IsDeactivated = user.IsDeactivated
		};
		return this.View(output);
	}

	protected async Task<IActionResult> ToggleActivation<T>([FromForm] ApplicationUserViewModel input, string action, string controller, string area) where T : ApplicationUserViewModel, new() {
		if (await this.CheckSession() is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var user = await this._userManager.FindByIdAsync(input.Id!);
		if (user is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al usuario.";
			return this.RedirectToAction(action, controller, new { area });
		}
		if (user!.Id == this._userManager.GetUserId(this.User)) {
			this.TempData["ErrorMessage"] = "No te puedes desactivar a tí mismo.";
			return this.RedirectToAction(action, controller, new { area });
		}
		var roles = (await this._userManager.GetRolesAsync(user)).ToList();
		if (roles.Contains(nameof(Roles.DirectorTeacher))) {
			this.TempData["ErrorMessage"] = "No puedes desactivar al director de carrera actual.";
			return this.RedirectToAction(action, controller, new { area });
		}
		user.IsDeactivated = !user.IsDeactivated;
		user.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(user);
		this.TempData["SuccessMessage"] = user.IsDeactivated ? "Usuario desactivado correctamente." : "Usuario activado correctamente.";
		return this.RedirectToAction(action, controller, new { area });
	}
}
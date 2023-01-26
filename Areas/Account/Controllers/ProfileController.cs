﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Utal.Icc.Sgm.Areas.Account.Views.Profile;
using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Areas.Account.Controllers;

[Area("Account")]
public class ProfileController : Controller {
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly IUserStore<ApplicationUser> _userStore;
	private readonly IUserEmailStore<ApplicationUser> _emailStore;
	private readonly RoleManager<IdentityRole> _roleManager;

	public ProfileController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore, RoleManager<IdentityRole> roleManager) {
		this._signInManager = signInManager;
		this._userManager = userManager;
		this._userStore = userStore;
		this._emailStore = (IUserEmailStore<ApplicationUser>)this._userStore;
		this._roleManager = roleManager;
	}

	public async Task<IActionResult> Index() {
		if (!this.User.Identity!.IsAuthenticated) {
			return this.RedirectToAction("Index", "SignIn", new { area = "Account" });
		}
		var applicationUser = await this._userManager.GetUserAsync(this.User);
		if (applicationUser is null) {
			this.ViewBag.ErrorMessage = "Error al obtener al usuario.";
			return this.View();
		}
		var indexViewModel = new IndexViewModel {
			FirstName = applicationUser!.FirstName,
			LastName = applicationUser.LastName,
			Rut = applicationUser.Rut,
			Email = await this._emailStore.GetEmailAsync(applicationUser, CancellationToken.None)
		};
		return this.View(indexViewModel);
	}

	public IActionResult ChangePassword() => !this.User.Identity!.IsAuthenticated ? this.RedirectToAction("Index", "SignIn", new { area = "Account" }) : this.View();

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> ChangePassword(string id, [FromForm] ChangePasswordViewModel model) {
		if (!this.User.Identity!.IsAuthenticated) {
			return this.RedirectToAction("Index", "SignIn", new { area = "Account" });
		}
		var applicationUser = await this._userManager.GetUserAsync(this.User);
		var passwordResult = await this._userManager.ChangePasswordAsync(applicationUser!, model.CurrentPassword!, model.NewPassword!);
		if (!passwordResult.Succeeded) {
			this.ViewBag.ErrorMessage = "Error al cambiar la contraseña.";
			this.ViewBag.ErrorMessages = passwordResult.Errors.Select(e => e.Description).ToList();
			return this.View();
		}
		this.ViewBag.SuccessMessage = "Se cambió la contraseña con éxito.";
		this.ModelState.Clear();
		return this.View();
	}

	[Authorize(Roles = "Student")]
	public async Task<IActionResult> Student() {
		if (!this.User.Identity!.IsAuthenticated) {
			return this.RedirectToAction("Index", "SignIn", new { area = "Account" });
		}
		if (await this._userManager.GetUserAsync(this.User) is not ApplicationUser student) {
			this.ViewBag.ErrorMessage = "Error al obtener al estudiante.";
			return this.View();
		}
		var studentViewModel = new StudentViewModel {
			UniversityId = student.UniversityId,
			RemainingCourses = student.RemainingCourses,
			IsDoingThePractice = student.IsDoingThePractice,
			IsWorking = student.IsWorking
		};
		return this.View(studentViewModel);
	}

	[Authorize(Roles = "Student"), HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Student([FromForm] StudentViewModel model) {
		if (!this.User.Identity!.IsAuthenticated) {
			return this.RedirectToAction("Index", "SignIn", new { area = "Account" });
		}
		if (await this._userManager.GetUserAsync(this.User) is not ApplicationUser student) {
			this.ViewBag.ErrorMessage = "Error al obtener al estudiante.";
			return this.View();
		}
		student.UniversityId = model.UniversityId;
		student.RemainingCourses = model.RemainingCourses;
		student.IsDoingThePractice = model.IsDoingThePractice;
		student.IsWorking = model.IsWorking;
		_ = await this._userManager.UpdateAsync(student);
		this.ViewBag.SuccessMessage = "Se actualizó el perfil con éxito.";
		return this.View();
	}

	[Authorize(Roles = "Teacher")]
	public async Task<IActionResult> Teacher() {
		if (!this.User.Identity!.IsAuthenticated) {
			return this.RedirectToAction("Index", "SignIn", new { area = "Account" });
		}
		if (await this._userManager.GetUserAsync(this.User) is not ApplicationUser teacher) {
			this.ViewBag.ErrorMessage = "Error al obtener al estudiante.";
			return this.View();
		}
		var teacherViewModel = new TeacherViewModel {
			Office = teacher.Office,
			Schedule = teacher.Schedule,
			Specialization = teacher.Specialization,
		};
		return this.View(teacherViewModel);
	}

	[Authorize(Roles = "Teacher"), HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Teacher([FromForm] TeacherViewModel model) {
		if (!this.User.Identity!.IsAuthenticated) {
			return this.RedirectToAction("Index", "SignIn", new { area = "Account" });
		}
		if (await this._userManager.GetUserAsync(this.User) is not ApplicationUser teacher) {
			this.ViewBag.ErrorMessage = "Error al obtener al estudiante.";
			return this.View();
		}
		teacher.Office = model.Office;
		teacher.Schedule = model.Schedule;
		teacher.Specialization = model.Specialization;
		_ = await this._userManager.UpdateAsync(teacher);
		this.ViewBag.SuccessMessage = "Se actualizó el perfil con éxito.";
		return this.View();
	}
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Utal.Icc.Sgm.Areas.Account.ViewModels.Profile;
using Utal.Icc.Sgm.Controllers;
using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;
using Utal.Icc.Sgm.ViewModels;

namespace Utal.Icc.Sgm.Areas.Account.Controllers;

[Area(nameof(Account))]
public class ProfileController : ApplicationController {
	public ProfileController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore, SignInManager<ApplicationUser> signInManager) : base(dbContext, userManager, userStore, signInManager) { }

	public async Task<IActionResult> Index() {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var output = new ApplicationUserViewModel {
			FirstName = user!.FirstName,
			LastName = user.LastName,
			Rut = user.Rut,
			Email = await this._emailStore.GetEmailAsync(user, CancellationToken.None),
			CreatedAt = user.CreatedAt,
			UpdatedAt = user.UpdatedAt
		};
		return this.View(output);
	}

	public IActionResult ChangePassword() => !this.User.Identity!.IsAuthenticated ? this.RedirectToAction(nameof(ProfileController.Index), "SignIn", new { area = nameof(Utal.Icc.Sgm.Areas.Account) }) : this.View();

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> ChangePassword([FromForm] ChangePasswordViewModel input) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var result = await this._userManager.ChangePasswordAsync(user!, input.CurrentPassword!, input.NewPassword!);
		if (!result.Succeeded) {
			this.ViewBag.ErrorMessage = "Contraseña incorrecta.";
			this.ViewBag.ErrorMessages = result.Errors.Select(e => e.Description).ToList();
			return this.View(input);
		}
		user.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(user);
		this.TempData["SuccessMessage"] = "Has cambiado tu contraseña correctamente.";
		return this.RedirectToAction(nameof(ProfileController.Index), nameof(ProfileController).Replace("Controller", string.Empty), new { area = nameof(Utal.Icc.Sgm.Areas.Account) });
	}

	[Authorize(Roles = nameof(Role.Student))]
	public async Task<IActionResult> Student() {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var output = new ApplicationUserViewModel {
			StudentUniversityId = user.StudentUniversityId,
			StudentRemainingCourses = user.StudentRemainingCourses,
			StudentIsDoingThePractice = user.StudentIsDoingThePractice,
			StudentIsWorking = user.StudentIsWorking
		};
		return this.View(output);
	}

	[Authorize(Roles = nameof(Role.Student)), HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Student([FromForm] ApplicationUserViewModel input) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		user.StudentRemainingCourses = input.StudentRemainingCourses;
		user.StudentIsDoingThePractice = input.StudentIsDoingThePractice;
		user.StudentIsWorking = input.StudentIsWorking;
		user.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(user);
		var output = new ApplicationUserViewModel {
			StudentUniversityId = user.StudentUniversityId,
			StudentRemainingCourses = user.StudentRemainingCourses,
			StudentIsDoingThePractice = user.StudentIsDoingThePractice,
			StudentIsWorking = user.StudentIsWorking
		};
		this.ViewBag.SuccessMessage = "Has actualizado tu perfil correctamente.";
		return this.View(output);
	}

	[Authorize(Roles = nameof(Role.Teacher))]
	public async Task<IActionResult> Teacher() {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var output = new ApplicationUserViewModel {
			TeacherOffice = user.TeacherOffice,
			TeacherSchedule = user.TeacherSchedule,
			TeacherSpecialization = user.TeacherSpecialization
		};
		return this.View(output);
	}

	[Authorize(Roles = nameof(Role.Teacher)), HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Teacher([FromForm] ApplicationUserViewModel input) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		user.TeacherOffice = input.TeacherOffice;
		user.TeacherSchedule = input.TeacherSchedule;
		user.TeacherSpecialization = input.TeacherSpecialization;
		user.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(user);
		var output = new ApplicationUserViewModel {
			TeacherOffice = input.TeacherOffice,
			TeacherSchedule = input.TeacherSchedule,
			TeacherSpecialization = input.TeacherSpecialization
		};
		this.ViewBag.SuccessMessage = "Has actualizado tu perfil correctamente.";
		return this.View(output);
	}
}
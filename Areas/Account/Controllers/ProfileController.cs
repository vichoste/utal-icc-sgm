using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Utal.Icc.Sgm.Areas.Account.ViewModels.Profile;
using Utal.Icc.Sgm.Models;
using Utal.Icc.Sgm.ViewModels;

namespace Utal.Icc.Sgm.Areas.Account.Controllers;

[Area("Account")]
public class ProfileController : Controller {
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly IUserStore<ApplicationUser> _userStore;
	private readonly IUserEmailStore<ApplicationUser> _emailStore;

	public ProfileController(UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore) {
		this._userManager = userManager;
		this._userStore = userStore;
		this._emailStore = (IUserEmailStore<ApplicationUser>)this._userStore;
	}

	public async Task<IActionResult> Index() {
		if (await this._userManager.GetUserAsync(this.User) is not ApplicationUser user) {
			return this.RedirectToAction("Index", "Home", new { area = string.Empty });
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

	[Authorize]
	public IActionResult ChangePassword() => this.View();

	[Authorize, HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> ChangePassword([FromForm] ChangePasswordViewModel input) {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = string.Empty });
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
		return this.RedirectToAction("Index", "Profile", new { area = "Account" });
	}

	[Authorize(Roles = "Student")]
	public async Task<IActionResult> Student() {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = string.Empty });
		}
		var output = new ApplicationUserViewModel {
			StudentUniversityId = user.StudentUniversityId,
			StudentRemainingCourses = user.StudentRemainingCourses,
			StudentIsDoingThePractice = user.StudentIsDoingThePractice,
			StudentIsWorking = user.StudentIsWorking
		};
		return this.View(output);
	}

	[Authorize(Roles = "Student"), HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Student([FromForm] ApplicationUserViewModel input) {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = string.Empty });
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

	[Authorize(Roles = "Teacher")]
	public async Task<IActionResult> Teacher() {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = string.Empty });
		}
		var output = new ApplicationUserViewModel {
			TeacherOffice = user.TeacherOffice,
			TeacherSchedule = user.TeacherSchedule,
			TeacherSpecialization = user.TeacherSpecialization
		};
		return this.View(output);
	}

	[Authorize(Roles = "Teacher"), HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Teacher([FromForm] ApplicationUserViewModel input) {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = string.Empty });
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
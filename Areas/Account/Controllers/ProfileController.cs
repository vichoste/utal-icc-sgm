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
			UniversityId = user.UniversityId,
			RemainingCourses = user.RemainingCourses,
			IsDoingThePractice = user.IsDoingThePractice,
			IsWorking = user.IsWorking
		};
		return this.View(output);
	}

	[Authorize(Roles = "Student"), HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Student([FromForm] ApplicationUserViewModel input) {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = string.Empty });
		}
		user.RemainingCourses = input.RemainingCourses;
		user.IsDoingThePractice = input.IsDoingThePractice;
		user.IsWorking = input.IsWorking;
		user.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(user);
		var output = new ApplicationUserViewModel {
			UniversityId = user.UniversityId,
			RemainingCourses = user.RemainingCourses,
			IsDoingThePractice = user.IsDoingThePractice,
			IsWorking = user.IsWorking
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
			Office = user.Office,
			Schedule = user.Schedule,
			Specialization = user.Specialization
		};
		return this.View(output);
	}

	[Authorize(Roles = "Teacher"), HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Teacher([FromForm] ApplicationUserViewModel input) {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = string.Empty });
		}
		user.Office = input.Office;
		user.Schedule = input.Schedule;
		user.Specialization = input.Specialization;
		user.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(user);
		var output = new ApplicationUserViewModel {
			Office = input.Office,
			Schedule = input.Schedule,
			Specialization = input.Specialization
		};
		this.ViewBag.SuccessMessage = "Has actualizado tu perfil correctamente.";
		return this.View(output);
	}
}
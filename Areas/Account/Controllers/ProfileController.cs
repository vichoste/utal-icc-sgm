using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Utal.Icc.Sgm.Areas.Account.Views.Profile;
using Utal.Icc.Sgm.Models;

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
		if (!this.User.Identity!.IsAuthenticated) {
			return this.RedirectToAction("Index", "SignIn", new { area = "Account" });
		}
		var applicationUser = await this._userManager.GetUserAsync(this.User);
		if (applicationUser is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al usuario.";
			return this.View();
		}
		var indexViewModel = new IndexViewModel {
			FirstName = applicationUser!.FirstName,
			LastName = applicationUser.LastName,
			Rut = applicationUser.Rut,
			Email = await this._emailStore.GetEmailAsync(applicationUser, CancellationToken.None),
			CreatedAt = applicationUser.CreatedAt,
			UpdatedAt = applicationUser.UpdatedAt
		};
		return this.View(indexViewModel);
	}

	public IActionResult ChangePassword() => !this.User.Identity!.IsAuthenticated ? this.RedirectToAction("Index", "SignIn", new { area = "Account" }) : this.View();

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> ChangePassword([FromForm] ChangePasswordViewModel model) {
		if (!this.User.Identity!.IsAuthenticated) {
			return this.RedirectToAction("Index", "SignIn", new { area = "Account" });
		}
		var applicationUser = await this._userManager.GetUserAsync(this.User);
		if (applicationUser is null) {
			return this.RedirectToAction("Index", "SignIn", new { area = "Account" });
		}
		var passwordResult = await this._userManager.ChangePasswordAsync(applicationUser!, model.CurrentPassword!, model.NewPassword!);
		if (!passwordResult.Succeeded) {
			this.ViewBag.ErrorMessage = "Tu contraseña no se pudo cambiar.";
			this.ViewBag.ErrorMessages = passwordResult.Errors.Select(e => e.Description).ToList();
			return this.View(model);
		}
		applicationUser.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(applicationUser);
		this.TempData["SuccessMessage"] = "Has cambiado tu contraseña con éxito.";
		return this.RedirectToAction("Index", "Profile", new { area = "Account" });
	}

	[Authorize(Roles = "Student")]
	public async Task<IActionResult> Student() {
		if (!this.User.Identity!.IsAuthenticated) {
			return this.RedirectToAction("Index", "SignIn", new { area = "Account" });
		}
		if (await this._userManager.GetUserAsync(this.User) is not ApplicationUser student) {
			return this.RedirectToAction("Index", "SignIn", new { area = "Account" });
		}
		var studentViewModel = new StudentViewModel {
			UniversityId = student.StudentUniversityId,
			RemainingCourses = student.StudentRemainingCourses,
			IsDoingThePractice = student.StudentIsDoingThePractice,
			IsWorking = student.StudentIsWorking
		};
		return this.View(studentViewModel);
	}

	[Authorize(Roles = "Student"), HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Student([FromForm] StudentViewModel model) {
		if (!this.User.Identity!.IsAuthenticated) {
			return this.RedirectToAction("Index", "SignIn", new { area = "Account" });
		}
		if (await this._userManager.GetUserAsync(this.User) is not ApplicationUser student) {
			return this.RedirectToAction("Index", "SignIn", new { area = "Account" });
		}
		student.StudentUniversityId = model.UniversityId;
		student.StudentRemainingCourses = model.RemainingCourses;
		student.StudentIsDoingThePractice = model.IsDoingThePractice;
		student.StudentIsWorking = model.IsWorking;
		student.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(student);
		this.ViewBag.SuccessMessage = "Has actualizado tu perfil con éxito.";
		return this.View(model);
	}

	[Authorize(Roles = "Teacher")]
	public async Task<IActionResult> Teacher() {
		if (!this.User.Identity!.IsAuthenticated) {
			return this.RedirectToAction("Index", "SignIn", new { area = "Account" });
		}
		if (await this._userManager.GetUserAsync(this.User) is not ApplicationUser teacher) {
			return this.RedirectToAction("Index", "SignIn", new { area = "Account" });
		}
		var teacherViewModel = new TeacherViewModel {
			Office = teacher.TeacherOffice,
			Schedule = teacher.TeacherSchedule,
			Specialization = teacher.TeacherSpecialization,
		};
		return this.View(teacherViewModel);
	}

	[Authorize(Roles = "Teacher"), HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Teacher([FromForm] TeacherViewModel model) {
		if (!this.User.Identity!.IsAuthenticated) {
			return this.RedirectToAction("Index", "SignIn", new { area = "Account" });
		}
		if (await this._userManager.GetUserAsync(this.User) is not ApplicationUser teacher) {
			return this.RedirectToAction("Index", "SignIn", new { area = "Account" });
		}
		teacher.TeacherOffice = model.Office;
		teacher.TeacherSchedule = model.Schedule;
		teacher.TeacherSpecialization = model.Specialization;
		teacher.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(teacher);
		this.ViewBag.SuccessMessage = "Has actualizado tu perfil con éxito.";
		return this.View(model);
	}
}
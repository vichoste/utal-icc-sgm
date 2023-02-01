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
		var applicationUser = await this._userManager.GetUserAsync(this.User);
		if (applicationUser is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (applicationUser.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
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
		var applicationUser = await this._userManager.GetUserAsync(this.User);
		if (applicationUser is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (applicationUser.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var passwordResult = await this._userManager.ChangePasswordAsync(applicationUser!, model.CurrentPassword!, model.NewPassword!);
		if (!passwordResult.Succeeded) {
			this.ViewBag.ErrorMessage = "Contraseña incorrecta.";
			this.ViewBag.ErrorMessages = passwordResult.Errors.Select(e => e.Description).ToList();
			return this.View(model);
		}
		applicationUser.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(applicationUser);
		this.TempData["SuccessMessage"] = "Has cambiado tu contraseña correctamente.";
		return this.RedirectToAction("Index", "Profile", new { area = "Account" });
	}

	[Authorize(Roles = "Student")]
	public async Task<IActionResult> Student() {
		var student = await this._userManager.GetUserAsync(this.User);
		if (student is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (student.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
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
		var student = await this._userManager.GetUserAsync(this.User);
		if (student is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (student.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		student.StudentRemainingCourses = model.RemainingCourses;
		student.StudentIsDoingThePractice = model.IsDoingThePractice;
		student.StudentIsWorking = model.IsWorking;
		student.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(student);
		var editViewModel = new StudentViewModel {
			UniversityId = model.UniversityId,
			RemainingCourses = model.RemainingCourses,
			IsDoingThePractice = model.IsDoingThePractice,
			IsWorking = model.IsWorking
		};
		this.ViewBag.SuccessMessage = "Has actualizado tu perfil correctamente.";
		return this.View(editViewModel);
	}

	[Authorize(Roles = "Teacher")]
	public async Task<IActionResult> Teacher() {
		var teacher = await this._userManager.GetUserAsync(this.User);
		if (teacher is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (teacher.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
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
		var teacher = await this._userManager.GetUserAsync(this.User);
		if (teacher is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (teacher.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		teacher.TeacherOffice = model.Office;
		teacher.TeacherSchedule = model.Schedule;
		teacher.TeacherSpecialization = model.Specialization;
		teacher.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(teacher);
		var teacherViewModel = new TeacherViewModel {
			Office = model.Office,
			Schedule = model.Schedule,
			Specialization = model.Specialization,
		};
		this.ViewBag.SuccessMessage = "Has actualizado tu perfil correctamente.";
		return this.View(teacherViewModel);
	}
}
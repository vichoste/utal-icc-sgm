using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Utal.Icc.Sgm.Areas.Account.Views.Profile;
using Utal.Icc.Sgm.Models;

using static Utal.Icc.Sgm.Models.ApplicationUser;

namespace Utal.Icc.Sgm.Areas.Account.Controllers;

[Area(nameof(Utal.Icc.Sgm.Areas.Account))]
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
			return this.RedirectToAction("Index", "Home", new { area = string.Empty });
		}
		if (applicationUser.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = string.Empty });
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

	public IActionResult ChangePassword() => !this.User.Identity!.IsAuthenticated ? this.RedirectToAction("Index", "SignIn", new { area = nameof(Utal.Icc.Sgm.Areas.Account) }) : this.View();

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> ChangePassword([FromForm] ChangePasswordViewModel model) {
		var applicationUser = await this._userManager.GetUserAsync(this.User);
		if (applicationUser is null) {
			return this.RedirectToAction("Index", "Home", new { area = string.Empty });
		}
		if (applicationUser.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = string.Empty });
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
		return this.RedirectToAction("Index", "Profile", new { area = nameof(Utal.Icc.Sgm.Areas.Account) });
	}

	[Authorize(Roles = nameof(Roles.Student))]
	public async Task<IActionResult> Student() {
		var student = await this._userManager.GetUserAsync(this.User);
		if (student is null) {
			return this.RedirectToAction("Index", "Home", new { area = string.Empty });
		}
		if (student.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = string.Empty });
		}
		var studentViewModel = new StudentViewModel {
			StudentUniversityId = student.StudentUniversityId,
			StudentRemainingCourses = student.StudentRemainingCourses,
			StudentIsDoingThePractice = student.StudentIsDoingThePractice,
			StudentIsWorking = student.StudentIsWorking
		};
		return this.View(studentViewModel);
	}

	[Authorize(Roles = nameof(Roles.Student)), HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Student([FromForm] StudentViewModel model) {
		var student = await this._userManager.GetUserAsync(this.User);
		if (student is null) {
			return this.RedirectToAction("Index", "Home", new { area = string.Empty });
		}
		if (student.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = string.Empty });
		}
		student.StudentRemainingCourses = model.StudentRemainingCourses;
		student.StudentIsDoingThePractice = model.StudentIsDoingThePractice;
		student.StudentIsWorking = model.StudentIsWorking;
		student.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(student);
		var editViewModel = new StudentViewModel {
			StudentUniversityId = student.StudentUniversityId,
			StudentRemainingCourses = student.StudentRemainingCourses,
			StudentIsDoingThePractice = student.StudentIsDoingThePractice,
			StudentIsWorking = student.StudentIsWorking
		};
		this.ViewBag.SuccessMessage = "Has actualizado tu perfil correctamente.";
		return this.View(editViewModel);
	}

	[Authorize(Roles = nameof(Roles.Teacher))]
	public async Task<IActionResult> Teacher() {
		var teacher = await this._userManager.GetUserAsync(this.User);
		if (teacher is null) {
			return this.RedirectToAction("Index", "Home", new { area = string.Empty });
		}
		if (teacher.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = string.Empty });
		}
		var teacherViewModel = new TeacherViewModel {
			TeacherOffice = teacher.TeacherOffice,
			TeacherSchedule = teacher.TeacherSchedule,
			TeacherSpecialization = teacher.TeacherSpecialization
		};
		return this.View(teacherViewModel);
	}

	[Authorize(Roles = nameof(Roles.Teacher)), HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Teacher([FromForm] TeacherViewModel model) {
		var teacher = await this._userManager.GetUserAsync(this.User);
		if (teacher is null) {
			return this.RedirectToAction("Index", "Home", new { area = string.Empty });
		}
		if (teacher.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = string.Empty });
		}
		teacher.TeacherOffice = model.TeacherOffice;
		teacher.TeacherSchedule = model.TeacherSchedule;
		teacher.TeacherSpecialization = model.TeacherSpecialization;
		teacher.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(teacher);
		var teacherViewModel = new TeacherViewModel {
			TeacherOffice = model.TeacherOffice,
			TeacherSchedule = model.TeacherSchedule,
			TeacherSpecialization = model.TeacherSpecialization
		};
		this.ViewBag.SuccessMessage = "Has actualizado tu perfil correctamente.";
		return this.View(teacherViewModel);
	}
}
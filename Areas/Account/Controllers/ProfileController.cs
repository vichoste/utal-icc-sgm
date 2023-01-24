using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
	private readonly ApplicationDbContext _dbContext;

	public ProfileController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore, RoleManager<IdentityRole> roleManager, ApplicationDbContext dbContext) {
		this._signInManager = signInManager;
		this._userManager = userManager;
		this._userStore = userStore;
		this._emailStore = (IUserEmailStore<ApplicationUser>)this._userStore;
		this._roleManager = roleManager;
		this._dbContext = dbContext;
	}

	public async Task<IActionResult> Index() {
		if (!this.User.Identity!.IsAuthenticated) {
			return this.RedirectToAction("Index", "SignIn", new { area = "Account" });
		}
		var applicationUser = await this._userManager.GetUserAsync(this.User);
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
		var applicationUsers = this._userManager.Users.Include(a => a.StudentProfile).ToList();
		ApplicationUser? student = null;
		foreach (var applicationUser in applicationUsers) {
			if (applicationUser == await this._userManager.GetUserAsync(this.User)) {
				student = applicationUser;
				break;
			}
		}
		var studentViewModel = new StudentViewModel {
			UniversityId = student!.StudentProfile!.UniversityId,
			RemainingCourses = student!.StudentProfile.RemainingCourses,
			IsDoingThePractice = student!.StudentProfile.IsDoingThePractice,
			IsWorking = student!.StudentProfile.IsWorking
		};
		return this.View(studentViewModel);
	}

	[Authorize(Roles = "Student"), HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Student([FromForm] StudentViewModel model) {
		if (!this.User.Identity!.IsAuthenticated) {
			return this.RedirectToAction("Index", "SignIn", new { area = "Account" });
		}
		var applicationUsers = this._userManager.Users.Include(a => a.StudentProfile).ToList();
		ApplicationUser? student = null;
		foreach (var applicationUser in applicationUsers) {
			if (applicationUser == await this._userManager.GetUserAsync(this.User)) {
				student = applicationUser;
				break;
			}
		}
		student!.StudentProfile!.UniversityId = model.UniversityId;
		student.StudentProfile.RemainingCourses = model.RemainingCourses;
		student.StudentProfile.IsDoingThePractice = model.IsDoingThePractice;
		student.StudentProfile.IsWorking = model.IsWorking;
		_ = await this._dbContext.SaveChangesAsync();
		this.ViewBag.SuccessMessage = "Se actualizó el perfil con éxito.";
		return this.View();
	}

	[Authorize(Roles = "Teacher")]
	public async Task<IActionResult> Teacher() {
		if (!this.User.Identity!.IsAuthenticated) {
			return this.RedirectToAction("Index", "SignIn", new { area = "Account" });
		}
		var applicationUsers = this._userManager.Users.Include(a => a.TeacherProfile).ToList();
		ApplicationUser? teacher = null;
		foreach (var applicationUser in applicationUsers) {
			if (applicationUser == await this._userManager.GetUserAsync(this.User)) {
				teacher = applicationUser;
				break;
			}
		}
		var teacherViewModel = new TeacherViewModel {
			Office = teacher!.TeacherProfile!.Office,
			Schedule = teacher!.TeacherProfile.Schedule,
			Specialization = teacher!.TeacherProfile.Specialization,
		};
		return this.View(teacherViewModel);
	}

	[Authorize(Roles = "Teacher"), HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Teacher([FromForm] TeacherViewModel model) {
		if (!this.User.Identity!.IsAuthenticated) {
			return this.RedirectToAction("Index", "SignIn", new { area = "Account" });
		}
		var applicationUsers = this._userManager.Users.Include(a => a.TeacherProfile).ToList();
		ApplicationUser? teacher = null;
		foreach (var applicationUser in applicationUsers) {
			if (applicationUser == await this._userManager.GetUserAsync(this.User)) {
				teacher = applicationUser;
				break;
			}
		}
		teacher!.TeacherProfile!.Office = model.Office;
		teacher.TeacherProfile.Schedule = model.Schedule;
		teacher.TeacherProfile.Specialization = model.Specialization;
		_ = await this._dbContext.SaveChangesAsync();
		this.ViewBag.SuccessMessage = "Se actualizó el perfil con éxito.";
		return this.View();
	}
}
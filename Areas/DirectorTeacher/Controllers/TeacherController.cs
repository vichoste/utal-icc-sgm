using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Utal.Icc.Sgm.Areas.DirectorTeacher.ViewModels.Teacher;
using Utal.Icc.Sgm.Controllers;
using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;
using Utal.Icc.Sgm.ViewModels;

using static Utal.Icc.Sgm.Models.ApplicationUser;

namespace Utal.Icc.Sgm.Areas.DirectorTeacher.Controllers;

[Area(nameof(DirectorTeacher)), Authorize(Roles = nameof(Roles.DirectorTeacher))]
public class TeacherController : ApplicationUserController {
	public TeacherController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore, SignInManager<ApplicationUser> signInManager) : base(dbContext, userManager, userStore, signInManager) { }

	public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
		=> await base.Index<ApplicationUserViewModel>(sortOrder, currentFilter, searchString, pageNumber, new[] { nameof(ApplicationUserViewModel.FirstName), nameof(ApplicationUserViewModel.LastName), nameof(ApplicationUserViewModel.StudentUniversityId), nameof(ApplicationUserViewModel.Rut), nameof(ApplicationUserViewModel.Email) },
			async () => (await this._userManager.GetUsersInRoleAsync(nameof(Roles.Teacher))).Select(
				async u => new IndexTeacherViewModel {
					Id = u.Id,
					FirstName = u.FirstName,
					LastName = u.LastName,
					Rut = u.Rut,
					Email = u.Email,
					IsDeactivated = u.IsDeactivated,
					IsDirectorTeacher = await this._userManager.IsInRoleAsync(u, nameof(Roles.DirectorTeacher)),
				}
			).Select(u => u.Result)
			.AsEnumerable()
		);

	public async Task<IActionResult> Create() => await base.Create<ApplicationUser, CreateTeacherViewModel>();

	[HttpPost]
	public async Task<IActionResult> Create([FromForm] CreateTeacherViewModel input) {
		var roles = new List<string>();
		if (input.IsGuideTeacher) {
			roles.Add(nameof(Roles.GuideTeacher));
		}
		if (input.IsAssistantTeacher) {
			roles.Add(nameof(Roles.AssistantTeacher));
		}
		if (input.IsCourseTeacher) {
			roles.Add(nameof(Roles.CourseTeacher));
		}
		if (input.IsCommitteeTeacher) {
			roles.Add(nameof(Roles.CommitteeTeacher));
		}
		return await base.Create<CreateTeacherViewModel, ApplicationUser>(input, roles, nameof(TeacherController.Index), nameof(TeacherController).Replace("Controller", string.Empty), nameof(DirectorTeacher));
	}

	public async Task<IActionResult> Edit(string id) => await base.Edit<EditTeacherViewModel>(id, nameof(TeacherController.Index), nameof(TeacherController).Replace("Controller", string.Empty), nameof(DirectorTeacher));

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit([FromForm] EditTeacherViewModel input) => await base.Edit<EditTeacherViewModel>(input, nameof(TeacherController.Index), nameof(TeacherController).Replace("Controller", string.Empty), nameof(DirectorTeacher));

	public async Task<IActionResult> ToggleActivation(string id) => await base.ToggleActivation<ApplicationUserViewModel>(id, nameof(TeacherController.Index), nameof(TeacherController).Replace("Controller", string.Empty), nameof(DirectorTeacher));

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> ToggleActivation([FromForm] ApplicationUserViewModel input) => await base.ToggleActivation<ApplicationUserViewModel>(input, nameof(TeacherController.Index), nameof(TeacherController).Replace("Controller", string.Empty), nameof(DirectorTeacher));

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
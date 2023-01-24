using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Utal.Icc.Sgm.Areas.DirectorTeacher.Views.Teacher;
using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Areas.DirectorTeacher.Controllers;

[Area("DirectorTeacher"), Authorize(Roles = "DirectorTeacher")]
public class TeacherController : Controller {
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly IUserStore<ApplicationUser> _userStore;
	private readonly IUserEmailStore<ApplicationUser> _emailStore;
	private readonly RoleManager<IdentityRole> _roleManager;
	private readonly ApplicationDbContext _dbContext;

	public TeacherController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore, RoleManager<IdentityRole> roleManager, ApplicationDbContext dbContext) {
		this._signInManager = signInManager;
		this._userManager = userManager;
		this._userStore = userStore;
		this._emailStore = (IUserEmailStore<ApplicationUser>)this._userStore;
		this._roleManager = roleManager;
		this._dbContext = dbContext;
	}

	public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		this.ViewData["FirstNameSortParam"] = sortOrder == "FirstName" ? "FirstNameDesc" : "FirstName";
		this.ViewData["LastNameSortParam"] = sortOrder == "LastName" ? "LastNameDesc" : "LastName";
		this.ViewData["RutSortParam"] = sortOrder == "Rut" ? "RutDesc" : "Rut";
		this.ViewData["EmailSortParam"] = sortOrder == "Email" ? "EmailDesc" : "Email";
		this.ViewData["CurrentSort"] = sortOrder;
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		var teachers = new List<ApplicationUser>();
		foreach (var applicationUser in this._userManager.Users.Include(a => a.TeacherProfile).ToList()) {
			if (await this._userManager.IsInRoleAsync(applicationUser, Roles.Teacher.ToString())) {
				teachers.Add(applicationUser);
			}
		}
		teachers = sortOrder switch {
			"FirstName" => teachers.OrderBy(a => a.FirstName).ToList(),
			"FirstNameDesc" => teachers.OrderByDescending(a => a.FirstName).ToList(),
			"Rut" => teachers.OrderBy(a => a.Rut).ToList(),
			"RutDesc" => teachers.OrderByDescending(a => a.Rut).ToList(),
			"Email" => teachers.OrderBy(a => a.Email).ToList(),
			"EmailDesc" => teachers.OrderByDescending(a => a.Email).ToList(),
			"LastName" => teachers.OrderBy(a => a.LastName).ToList(),
			"LastNameDesc" => teachers.OrderByDescending(a => a.LastName).ToList(),
			_ => teachers.OrderBy(a => a.LastName).ToList()
		};
		if (!string.IsNullOrEmpty(searchString)) {
			teachers = teachers.Where(s => s.FirstName!.ToUpper().Contains(searchString.ToUpper()) || s.LastName!.ToUpper().Contains(searchString.ToUpper()) || s.Rut!.ToUpper().Contains(searchString.ToUpper()) || s.Email == searchString).ToList();
		}
		var indexViewModels = teachers.Select(async a => new IndexViewModel {
			Id = a.Id,
			FirstName = a.FirstName,
			LastName = a.LastName,
			Rut = a.Rut,
			Email = a.Email,
			IsDirectorTeacher = await this._userManager.IsInRoleAsync(a, Roles.DirectorTeacher.ToString()),
		}).Select(t => t.Result);
		var pageSize = 6;
		return this.View(PaginatedList<IndexViewModel>.Create(indexViewModels.AsQueryable(), pageNumber ?? 1, pageSize));
	}

	public IActionResult Create() => this.View();

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Create([FromForm] Create model) {
		if (!this.ModelState.IsValid) {
			this.ViewBag.ErrorMessage = "Revisa que los campos estén correctos.";
			return this.View();
		}
		var applicationUser = new ApplicationUser {
			FirstName = model.FirstName,
			LastName = model.LastName,
			Rut = model.Rut,
		};
		await this._userStore.SetUserNameAsync(applicationUser, model.Email, CancellationToken.None);
		await this._emailStore.SetEmailAsync(applicationUser, model.Email, CancellationToken.None);
		var createResult = await this._userManager.CreateAsync(applicationUser, model.Password!);
		if (createResult.Succeeded) {
			var rolesResult = await this._userManager.AddToRoleAsync(applicationUser, "Teacher");
			var rankRoles = new List<string>();
			if (model.IsGuideTeacher) {
				rankRoles.Add(Roles.GuideTeacher.ToString());
			}
			if (model.IsAssistantTeacher) {
				rankRoles.Add(Roles.AssistantTeacher.ToString());
			}
			if (model.IsCourseTeacher) {
				rankRoles.Add(Roles.CourseTeacher.ToString());
			}
			if (model.IsCommitteeTeacher) {
				rankRoles.Add(Roles.CommitteeTeacher.ToString());
			}
			var rankRolesResult = await this._userManager.AddToRolesAsync(applicationUser, rankRoles);
			if (rolesResult.Succeeded && rankRolesResult.Succeeded) {
				var teacherProfile = new TeacherProfile {
					ApplicationUser = applicationUser
				};
				_ = this._dbContext.TeacherProfiles.Add(teacherProfile);
				_ = await this._dbContext.SaveChangesAsync();
				this.ViewBag.SuccessMessage = "Profesor creado con éxito.";
				this.ModelState.Clear();
				return this.View();
			}
			this.ViewBag.WarningMessage = "Profesor creado, pero no se le pudo asignar el(los) rol(es).";
			this.ViewBag.WarningMessages = rolesResult.Errors.Select(w => w.Description).ToList();
			this.ModelState.Clear();
			return this.View();
		}
		if (createResult.Errors.Any()) {
			this.ViewBag.ErrorMessages = createResult.Errors.Select(e => e.Description).ToList();
		}
		this.ViewBag.ErrorMessage = "Error al crear el profesor.";
		return this.View();
	}

	public async Task<IActionResult> Edit(string id) {
		var applicationUser = this._userManager.Users.Include(a => a.TeacherProfile).FirstOrDefault(a => a.Id == id);
		if (applicationUser is null) {
			this.ViewBag.ErrorMessage = "Error al obtener al profesor.";
			return this.View();
		}
		var editViewModel = new EditViewModel {
			FirstName = applicationUser.FirstName,
			LastName = applicationUser.LastName,
			Rut = applicationUser.Rut,
			Email = applicationUser.Email
		};
		var roles = (await this._userManager.GetRolesAsync(applicationUser)).ToList();
		editViewModel.IsGuideTeacher = roles.Contains(Roles.GuideTeacher.ToString());
		editViewModel.IsAssistantTeacher = roles.Contains(Roles.AssistantTeacher.ToString());
		editViewModel.IsCourseTeacher = roles.Contains(Roles.CourseTeacher.ToString());
		editViewModel.IsCommitteeTeacher = roles.Contains(Roles.CommitteeTeacher.ToString());
		return this.View(editViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit(string id, [FromForm] EditViewModel model) {
		var applicationUser = this._userManager.Users.Include(a => a.TeacherProfile).FirstOrDefault(a => a.Id == id);
		if (applicationUser is null) {
			this.ViewBag.ErrorMessage = "Error al obtener al profesor.";
			return this.View();
		}
		await this._userStore.SetUserNameAsync(applicationUser, userName: model.Email, CancellationToken.None);
		await this._emailStore.SetEmailAsync(applicationUser, model.Email, CancellationToken.None);
		applicationUser.FirstName = !model.FirstName.IsNullOrEmpty() ? model.FirstName : applicationUser.FirstName;
		applicationUser.LastName = !model.LastName.IsNullOrEmpty() ? model.LastName : applicationUser.LastName;
		applicationUser.Rut = !model.Rut.IsNullOrEmpty() ? model.Rut : applicationUser.Rut;
		var updateResult = await this._userManager.UpdateAsync(applicationUser);
		if (updateResult.Succeeded) {
			var roles = (await this._userManager.GetRolesAsync(applicationUser)).ToList();
			if (roles.Contains(Roles.Teacher.ToString())) {
				_ = roles.Remove(Roles.Teacher.ToString());
			}
			if (roles.Contains(Roles.DirectorTeacher.ToString())) {
				_ = roles.Remove(Roles.DirectorTeacher.ToString());
			}
			var removeRankRolesResult = await this._userManager.RemoveFromRolesAsync(applicationUser, roles);
			var rankRoles = new List<string>();
			if (model.IsGuideTeacher) {
				rankRoles.Add(Roles.GuideTeacher.ToString());
			}
			if (model.IsAssistantTeacher) {
				rankRoles.Add(Roles.AssistantTeacher.ToString());
			}
			if (model.IsCourseTeacher) {
				rankRoles.Add(Roles.CourseTeacher.ToString());
			}
			if (model.IsCommitteeTeacher) {
				rankRoles.Add(Roles.CommitteeTeacher.ToString());
			}
			var rankRolesResult = await this._userManager.AddToRolesAsync(applicationUser, rankRoles);
			if (removeRankRolesResult.Succeeded && rankRolesResult.Succeeded) {
				this.ViewBag.SuccessMessage = "Profesor actualizado con éxito.";
				this.ModelState.Clear();
				return this.View();
			}
			this.ViewBag.WarningMessage = "Profesor actualizado, pero no se le pudo asignar el(los) rol(es).";
			this.ViewBag.WarningMessages = removeRankRolesResult.Errors.Select(w => w.Description).ToList();
			this.ViewBag.WarningMessages2 = rankRolesResult.Errors.Select(w => w.Description).ToList();
			this.ModelState.Clear();
			return this.View();
		}
		if (updateResult.Errors.Any()) {
			this.ViewBag.ErrorMessages = updateResult.Errors.Select(e => e.Description).ToList();
		}
		this.ViewBag.ErrorMessage = "Error al actualizar al profesor.";
		return this.View();
	}

	public IActionResult Delete(string id) {
		var applicationUser = this._userManager.Users.FirstOrDefault(a => a.Id == id);
		if (applicationUser is null) {
			this.ViewBag.ErrorMessage = "Error al obtener al profesor.";
			return this.View();
		}
		var deleteViewModel = new DeleteViewModel {
			Id = applicationUser.Id,
			Email = applicationUser.Email
		};
		return this.View(deleteViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete(string id, [FromForm] DeleteViewModel model) {
		var applicationUser = this._userManager.Users.Include(a => a.TeacherProfile).FirstOrDefault(a => a.Id == id);
		if (applicationUser is null) {
			this.ViewBag.ErrorMessage = "Error al obtener al profesor.";
			return this.View();
		}
		if (applicationUser!.Id == this._userManager.GetUserId(this.User)) {
			this.ViewBag.ErrorMessage = "No te puedes eliminar a tí mismo.";
			return this.View();
		}
		var roles = (await this._userManager.GetRolesAsync(applicationUser)).ToList();
		if (roles.Contains(Roles.DirectorTeacher.ToString())) {
			this.ViewBag.ErrorMessage = "No puedes eliminar al director de carrera actual.";
			return this.View();
		}
		_ = this._dbContext.TeacherProfiles.Remove(applicationUser.TeacherProfile!);
		_ = await this._dbContext.SaveChangesAsync();
		var result = await this._userManager.DeleteAsync(applicationUser);
		if (result.Succeeded) {
			this.ViewBag.SuccessMessage = "Profesor eliminado con éxito.";
			return this.View();
		}
		this.ViewBag.ErrorMessage = "Error al eliminar al profesor.";
		this.ViewBag.ErrorMessages = result.Errors.Select(e => e.Description).ToList();
		return this.View();
	}

	public IActionResult Transfer(string id) {
		var applicationUser = this._userManager.Users.FirstOrDefault(a => a.Id == id);
		if (applicationUser is null) {
			this.ViewBag.ErrorMessage = "Error al obtener al profesor.";
			return this.View();
		}
		var transferViewModel = new TransferViewModel {
			Id = applicationUser.Id,
			Email = applicationUser.Email
		};
		return this.View(transferViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Transfer(string id, [FromForm] TransferViewModel model) {
		var applicationUser = this._userManager.Users.FirstOrDefault(a => a.Id == id);
		if (applicationUser is null) {
			this.ViewBag.ErrorMessage = "Error al obtener al profesor.";
			return this.View();
		}
		var directorTeacher = this._userManager.Users.FirstOrDefault(a => a.Id == this._userManager.GetUserId(this.User));
		if (applicationUser == directorTeacher) {
			this.ViewBag.ErrorMessage = "No puedes transferirte a tí mismo.";
			return this.View();
		}
		if (directorTeacher is null) {
			this.ViewBag.ErrorMessage = "Error al obtener al director de carrera actual.";
			return this.View();
		}
		var directorTeacherRoles = (await this._userManager.GetRolesAsync(directorTeacher)).ToList();
		if (!directorTeacherRoles.Contains(Roles.DirectorTeacher.ToString())) {
			this.ViewBag.ErrorMessage = "El director de carrera actual no es director de carrera.";
			return this.View();
		}
		var removeDirectorTeacherRoleResult = await this._userManager.RemoveFromRoleAsync(directorTeacher, Roles.DirectorTeacher.ToString());
		var addDirectorTeacherRoleResult = await this._userManager.AddToRoleAsync(applicationUser, Roles.DirectorTeacher.ToString());
		if (removeDirectorTeacherRoleResult.Succeeded && addDirectorTeacherRoleResult.Succeeded) {
			this.ViewBag.SuccessMessage = "Director de carrera transferido con éxito.";
			this.ModelState.Clear();
			return this.View();
		}
		this.ViewBag.ErrorMessage = "Error al transferir el director de carrera.";
		this.ViewBag.ErrorMessages = removeDirectorTeacherRoleResult.Errors.Select(e => e.Description).ToList();
		this.ViewBag.ErrorMessages2 = addDirectorTeacherRoleResult.Errors.Select(e => e.Description).ToList();
		return this.View();
	}
}
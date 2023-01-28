﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using Utal.Icc.Sgm.Areas.DirectorTeacher.Views.Teacher;
using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Areas.DirectorTeacher.Controllers;

[Area("DirectorTeacher"), Authorize(Roles = "DirectorTeacher")]
public class TeacherController : Controller {
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly IUserStore<ApplicationUser> _userStore;
	private readonly IUserEmailStore<ApplicationUser> _emailStore;

	public TeacherController(UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore) {
		this._userManager = userManager;
		this._userStore = userStore;
		this._emailStore = (IUserEmailStore<ApplicationUser>)this._userStore;
	}

	public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		this.ViewData["FirstNameSortParam"] = sortOrder == "FirstName" ? "FirstNameDesc" : "FirstName";
		this.ViewData["LastNameSortParam"] = sortOrder == "LastName" ? "LastNameDesc" : "LastName";
		this.ViewData["UniversityIdSortParam"] = sortOrder == "StudentUniversityId" ? "UniversityIdDesc" : "StudentUniversityId";
		this.ViewData["RutSortParam"] = sortOrder == "Rut" ? "RutDesc" : "Rut";
		this.ViewData["EmailSortParam"] = sortOrder == "Email" ? "EmailDesc" : "Email";
		this.ViewData["CurrentSort"] = sortOrder;
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		var teachers = await this._userManager.GetUsersInRoleAsync(Roles.Teacher.ToString());
		var orderedTeachers = sortOrder switch {
			"FirstName" => teachers.OrderBy(t => t.FirstName),
			"FirstNameDesc" => teachers.OrderByDescending(t => t.FirstName),
			"Rut" => teachers.OrderBy(t => t.Rut),
			"RutDesc" => teachers.OrderByDescending(t => t.Rut),
			"Email" => teachers.OrderBy(t => t.Email),
			"EmailDesc" => teachers.OrderByDescending(t => t.Email),
			"LastName" => teachers.OrderBy(t => t.LastName),
			"LastNameDesc" => teachers.OrderByDescending(t => t.LastName),
			_ => teachers.OrderBy(t => t.LastName)
		};
		var filteredAndOrderedTeachers = orderedTeachers.ToList();
		if (!string.IsNullOrEmpty(searchString)) {
			filteredAndOrderedTeachers = orderedTeachers.Where(t => t.FirstName!.ToUpper().Contains(searchString.ToUpper()) || t.LastName!.ToUpper().Contains(searchString.ToUpper()) || t.Rut!.ToUpper().Contains(searchString.ToUpper()) || t.Email == searchString).ToList();
		}
		var indexViewModels = filteredAndOrderedTeachers.Select(async t => new IndexViewModel {
			Id = t.Id,
			FirstName = t.FirstName,
			LastName = t.LastName,
			Rut = t.Rut,
			Email = t.Email,
			IsDirectorTeacher = await this._userManager.IsInRoleAsync(t, Roles.DirectorTeacher.ToString()),
		}).Select(t => t.Result);
		var pageSize = 6;
		return this.View(PaginatedList<IndexViewModel>.Create(indexViewModels.AsQueryable(), pageNumber ?? 1, pageSize));
	}

	public IActionResult Create() => this.View();

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Create([FromForm] CreateViewModel model) {
		if (!this.ModelState.IsValid) {
			this.ViewBag.ErrorMessage = "Revisa que los campos estén correctos.";
			return this.View(model);
		}
		var teacher = new ApplicationUser {
			FirstName = model.FirstName,
			LastName = model.LastName,
			Rut = model.Rut,
		};
		await this._userStore.SetUserNameAsync(teacher, model.Email, CancellationToken.None);
		await this._emailStore.SetEmailAsync(teacher, model.Email, CancellationToken.None);
		var createResult = await this._userManager.CreateAsync(teacher, model.Password!);
		if (createResult.Succeeded) {
			var rolesResult = await this._userManager.AddToRoleAsync(teacher, Roles.Teacher.ToString());
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
			var rankRolesResult = await this._userManager.AddToRolesAsync(teacher, rankRoles);
			if (rolesResult.Succeeded && rankRolesResult.Succeeded) {
				this.ViewBag.SuccessMessage = "Profesor creado con éxito.";
				this.ModelState.Clear();
				return this.View(model);
			}
			this.ViewBag.WarningMessage = "Profesor creado, pero no se le pudo asignar el rol.";
			this.ViewBag.WarningMessages = rolesResult.Errors.Select(w => w.Description).ToList();
			this.ModelState.Clear();
			return this.View(model);
		}
		if (createResult.Errors.Any()) {
			this.ViewBag.ErrorMessages = createResult.Errors.Select(e => e.Description).ToList();
		}
		this.ViewBag.ErrorMessage = "Error al crear el profesor.";
		return this.View(model);
	}

	public async Task<IActionResult> Edit(string id) {
		var teacher = await this._userManager.FindByIdAsync(id);
		if (teacher is null) {
			this.ViewBag.ErrorMessage = "Error al obtener al profesor.";
			return this.View();
		}
		var editViewModel = new EditViewModel {
			FirstName = teacher.FirstName,
			LastName = teacher.LastName,
			Rut = teacher.Rut,
			Email = teacher.Email
		};
		var roles = (await this._userManager.GetRolesAsync(teacher)).ToList();
		editViewModel.IsGuideTeacher = roles.Contains(Roles.GuideTeacher.ToString());
		editViewModel.IsAssistantTeacher = roles.Contains(Roles.AssistantTeacher.ToString());
		editViewModel.IsCourseTeacher = roles.Contains(Roles.CourseTeacher.ToString());
		editViewModel.IsCommitteeTeacher = roles.Contains(Roles.CommitteeTeacher.ToString());
		return this.View(editViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit(string id, [FromForm] EditViewModel model) {
		var teacher = await this._userManager.FindByIdAsync(id);
		if (teacher is null) {
			this.ViewBag.ErrorMessage = "Error al obtener al profesor.";
			return this.View(model);
		}
		await this._userStore.SetUserNameAsync(teacher, userName: model.Email, CancellationToken.None);
		await this._emailStore.SetEmailAsync(teacher, model.Email, CancellationToken.None);
		teacher.FirstName = !model.FirstName.IsNullOrEmpty() ? model.FirstName : teacher.FirstName;
		teacher.LastName = !model.LastName.IsNullOrEmpty() ? model.LastName : teacher.LastName;
		teacher.Rut = !model.Rut.IsNullOrEmpty() ? model.Rut : teacher.Rut;
		var updateResult = await this._userManager.UpdateAsync(teacher);
		if (updateResult.Succeeded) {
			var roles = (await this._userManager.GetRolesAsync(teacher)).ToList();
			if (roles.Contains(Roles.Teacher.ToString())) {
				_ = roles.Remove(Roles.Teacher.ToString());
			}
			if (roles.Contains(Roles.DirectorTeacher.ToString())) {
				_ = roles.Remove(Roles.DirectorTeacher.ToString());
			}
			var removeRankRolesResult = await this._userManager.RemoveFromRolesAsync(teacher, roles);
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
			var rankRolesResult = await this._userManager.AddToRolesAsync(teacher, rankRoles);
			if (removeRankRolesResult.Succeeded && rankRolesResult.Succeeded) {
				this.ViewBag.SuccessMessage = "Profesor actualizado con éxito.";
				return this.View(model);
			}
			this.ViewBag.WarningMessage = "Profesor actualizado, pero no se le pudo asignar el(los) rol(es).";
			this.ViewBag.WarningMessages = removeRankRolesResult.Errors.Select(w => w.Description).ToList();
			this.ViewBag.WarningMessages2 = rankRolesResult.Errors.Select(w => w.Description).ToList();
			return this.View(model);
		}
		if (updateResult.Errors.Any()) {
			this.ViewBag.ErrorMessages = updateResult.Errors.Select(e => e.Description).ToList();
		}
		this.ViewBag.ErrorMessage = "Error al actualizar al profesor.";
		return this.View(model);
	}

	public async Task<IActionResult> Delete(string id) {
		var teacher = await this._userManager.FindByIdAsync(id);
		if (teacher is null) {
			this.ViewBag.ErrorMessage = "Error al obtener al profesor.";
			return this.View();
		}
		var deleteViewModel = new DeleteViewModel {
			Id = teacher.Id,
			Email = teacher.Email
		};
		return this.View(deleteViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete(string id, [FromForm] DeleteViewModel model) {
		var teacher = await this._userManager.FindByIdAsync(id);
		if (teacher is null) {
			this.ViewBag.ErrorMessage = "Error al obtener al profesor.";
			return this.View(model);
		}
		if (teacher!.Id == this._userManager.GetUserId(this.User)) {
			this.ViewBag.ErrorMessage = "No te puedes eliminar a tí mismo.";
			return this.View(model);
		}
		var roles = (await this._userManager.GetRolesAsync(teacher)).ToList();
		if (roles.Contains(Roles.DirectorTeacher.ToString())) {
			this.ViewBag.ErrorMessage = "No puedes eliminar al director de carrera actual.";
			return this.View(model);
		}
		var result = await this._userManager.DeleteAsync(teacher);
		if (result.Succeeded) {
			this.ViewBag.SuccessMessage = "Profesor eliminado con éxito.";
			this.ModelState.Clear();
			return this.View(model);
		}
		this.ViewBag.ErrorMessage = "Error al eliminar al profesor.";
		this.ViewBag.ErrorMessages = result.Errors.Select(e => e.Description).ToList();
		return this.View(model);
	}

	public async Task<IActionResult> Transfer(string id) {
		var teacher = await this._userManager.FindByIdAsync(id);
		if (teacher is null) {
			this.ViewBag.ErrorMessage = "Error al obtener al profesor.";
			return this.View();
		}
		var transferViewModel = new TransferViewModel {
			Id = teacher.Id,
			Email = teacher.Email
		};
		return this.View(transferViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Transfer(string id, [FromForm] TransferViewModel model) {
		var currentDirectorTeacher = await this._userManager.FindByIdAsync(id);
		if (currentDirectorTeacher is null) {
			this.ViewBag.ErrorMessage = "Error al obtener al director de carrera actual.";
			return this.View(model);
		}
		var newDirectorTeacher = await this._userManager.FindByIdAsync(model.Id!);
		if (newDirectorTeacher is null) {
			this.ViewBag.ErrorMessage = "Error al obtener al nuevo director de carrera actual.";
			return this.View(model);
		}
		if (currentDirectorTeacher == newDirectorTeacher) {
			this.ViewBag.ErrorMessage = "No puedes transferirte a tí mismo.";
			return this.View(model);
		}
		var currentDirectorTeacherRoles = (await this._userManager.GetRolesAsync(currentDirectorTeacher)).ToList();
		if (!currentDirectorTeacherRoles.Contains(Roles.DirectorTeacher.ToString())) {
			this.ViewBag.ErrorMessage = "El profesor quien es fuente de esta petición no es director de carrera.";
			return this.View(model);
		}
		var removeCurrentDirectorTeacherResult = await this._userManager.RemoveFromRoleAsync(currentDirectorTeacher, Roles.DirectorTeacher.ToString());
		var transferCurrentDirectorTeacherResult = await this._userManager.AddToRoleAsync(newDirectorTeacher, Roles.DirectorTeacher.ToString());
		if (removeCurrentDirectorTeacherResult.Succeeded && transferCurrentDirectorTeacherResult.Succeeded) {
			this.ViewBag.SuccessMessage = "Director de carrera transferido con éxito.";
			this.ModelState.Clear();
			return this.View(model);
		}
		this.ViewBag.ErrorMessage = "Error al transferir el director de carrera.";
		this.ViewBag.ErrorMessages = removeCurrentDirectorTeacherResult.Errors.Select(e => e.Description).ToList();
		this.ViewBag.ErrorMessages2 = transferCurrentDirectorTeacherResult.Errors.Select(e => e.Description).ToList();
		return this.View(model);
	}
}
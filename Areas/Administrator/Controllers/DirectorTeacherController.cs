using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Utal.Icc.Sgm.Areas.Administrator.Views.DirectorTeacher;
using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Areas.Administrator.Controllers;

[Area("Administrator"), Authorize(Roles = "Administrator")]
public class DirectorTeacherController : Controller {
	private readonly UserManager<ApplicationUser> _userManager;

	public DirectorTeacherController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) => this._userManager = userManager;

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
		var teachers = sortOrder switch {
			"FirstName" => (await this._userManager.GetUsersInRoleAsync("Teacher")).OrderBy(a => a.FirstName).ToList(),
			"FirstNameDesc" => (await this._userManager.GetUsersInRoleAsync("Teacher")).OrderByDescending(a => a.FirstName).ToList(),
			"Rut" => (await this._userManager.GetUsersInRoleAsync("Teacher")).OrderBy(a => a.Rut).ToList(),
			"RutDesc" => (await this._userManager.GetUsersInRoleAsync("Teacher")).OrderByDescending(a => a.Rut).ToList(),
			"Email" => (await this._userManager.GetUsersInRoleAsync("Teacher")).OrderBy(a => a.Email).ToList(),
			"EmailDesc" => (await this._userManager.GetUsersInRoleAsync("Teacher")).OrderByDescending(a => a.Email).ToList(),
			"LastName" => (await this._userManager.GetUsersInRoleAsync("Teacher")).OrderBy(a => a.LastName).ToList(),
			"LastNameDesc" => (await this._userManager.GetUsersInRoleAsync("Teacher")).OrderByDescending(a => a.LastName).ToList(),
			_ => (await this._userManager.GetUsersInRoleAsync("Teacher")).OrderBy(a => a.LastName).ToList()
		};
		if (!string.IsNullOrEmpty(searchString)) {
			teachers = teachers.Where(s => s.FirstName!.ToUpper().Contains(searchString.ToUpper()) || s.LastName!.ToUpper().Contains(searchString.ToUpper()) || s.Rut!.ToUpper().Contains(searchString.ToUpper()) || s.Email == searchString).ToList();
		}
		var indexViewModels = teachers.Select(async t => new IndexViewModel {
			Id = t.Id,
			FirstName = t.FirstName,
			LastName = t.LastName,
			Rut = t.Rut,
			Email = t.Email,
			IsDirectorTeacher = await this._userManager.IsInRoleAsync(t, "DirectorTeacher")
		}).Select(t => t.Result);
		var pageSize = 6;
		return this.View(PaginatedList<IndexViewModel>.Create(indexViewModels.AsQueryable(), pageNumber ?? 1, pageSize));
	}

	public async Task<IActionResult> Toggle(string id) {
		var user = this._userManager.Users.FirstOrDefault(a => a.Id == id);
		if (user is null) {
			this.ViewBag.ErrorMessage = "Error al obtener al usuario.";
			return this.View();
		}
		if (!await this._userManager.IsInRoleAsync(user, "Teacher")) {
			this.ViewBag.ErrorMessage = "Error al obtener al usuario.";
			return this.View();
		}
		var model = new ToggleViewModel {
			Id = user.Id,
			Email = user.Email,
			IsDirectorTeacher = await this._userManager.IsInRoleAsync(user, "DirectorTeacher")
		};
		return this.View(model);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Toggle(string id, [FromForm] ToggleViewModel model) {
		var user = this._userManager.Users.FirstOrDefault(a => a.Id == id);
		if (user is null) {
			this.ViewBag.ErrorMessage = "Error al obtener al usuario.";
			return this.View();
		}
		if (!await this._userManager.IsInRoleAsync(user, "Teacher")) {
			this.ViewBag.ErrorMessage = "Error al obtener al usuario.";
			return this.View();
		}
		if (await this._userManager.IsInRoleAsync(user, "DirectorTeacher")) {
			_ = await this._userManager.RemoveFromRoleAsync(user, "DirectorTeacher");
			this.ViewBag.SuccessMessage = "Ya no es director de carrera.";
			return this.View();
		} else {
			_ = await this._userManager.AddToRoleAsync(user, "DirectorTeacher");
			this.ViewBag.SuccessMessage = "Ahora es director de carrera.";
		}
		return this.View();
	}
}
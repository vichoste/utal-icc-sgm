using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Utal.Icc.Sgm.Areas.Administrator.Views.DirectorTeacher;
using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Areas.Administrator.Controllers;

[Area("Administrator"), Authorize(Roles = "Administrator")]
public class DirectorTeacherController : Controller {
	private readonly UserManager<ApplicationUser> _userManager;

	public DirectorTeacherController(UserManager<ApplicationUser> userManager) => this._userManager = userManager;

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
		var users = sortOrder switch {
			"FirstName" => this._userManager.Users.OrderBy(u => u.FirstName).ToList(),
			"FirstNameDesc" => this._userManager.Users.OrderByDescending(u => u.FirstName).ToList(),
			"Rut" => this._userManager.Users.OrderBy(u => u.Rut).ToList(),
			"RutDesc" => this._userManager.Users.OrderByDescending(u => u.Rut).ToList(),
			"Email" => this._userManager.Users.OrderBy(u => u.Email).ToList(),
			"EmailDesc" => this._userManager.Users.OrderByDescending(u => u.Email).ToList(),
			"LastName" => this._userManager.Users.OrderBy(u => u.LastName).ToList(),
			"LastNameDesc" => this._userManager.Users.OrderByDescending(u => u.LastName).ToList(),
			_ => this._userManager.Users.OrderBy(u => u.LastName).ToList()
		};
		if (!string.IsNullOrEmpty(searchString)) {
			users = users.Where(s => s.FirstName!.ToUpper().Contains(searchString.ToUpper()) || s.LastName!.ToUpper().Contains(searchString.ToUpper()) || s.Rut!.ToUpper().Contains(searchString.ToUpper()) || s.Email == searchString).ToList();
		}
		var indexViewModels = new List<IndexViewModel>();
		foreach (var user in users) {
			if (await this._userManager.IsInRoleAsync(user, "Teacher")) {
				indexViewModels.Add(new IndexViewModel {
					Id = user.Id,
					FirstName = user.FirstName,
					LastName = user.LastName,
					Rut = user.Rut,
					Email = user.Email,
					IsDirectorTeacher = await this._userManager.IsInRoleAsync(user, "DirectorTeacher")
				});
			}
		}
		var pageSize = 6;
		return this.View(PaginatedList<IndexViewModel>.Create(indexViewModels.AsQueryable(), pageNumber ?? 1, pageSize));
	}

	public async Task<IActionResult> Toggle(string id) {
		var user = this._userManager.Users.FirstOrDefault(u => u.Id == id);
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
		var user = this._userManager.Users.FirstOrDefault(u => u.Id == id);
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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Utal.Icc.Sgm.Areas.Account.Models;
using Utal.Icc.Sgm.Areas.Account.Views.Administration;

namespace Utal.Icc.Sgm.Areas.Account.Controllers;

[Area("Account"), Authorize(Roles = "Administrator")]
public class AdministrationController : Controller {
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly IUserStore<ApplicationUser> _userStore;
	private readonly IUserEmailStore<ApplicationUser> _emailStore;
	private readonly RoleManager<IdentityRole> _roleManager;

	public AdministrationController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore, RoleManager<IdentityRole> roleManager) {
		this._signInManager = signInManager;
		this._userManager = userManager;
		this._userStore = userStore;
		this._emailStore = (IUserEmailStore<ApplicationUser>)this._userStore;
		this._roleManager = roleManager;
	}

	public IActionResult CreateUser() => this.View();

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> CreateUser([FromForm] CreateUserViewModel model) {
		if (!this.ModelState.IsValid) {
			this.ViewBag.ErrorMessage = "Revisa que los campos estén correctos.";
			return this.View();
		}
		var user = new ApplicationUser {
			FirstName = model.FirstName,
			LastName = model.LastName,
			UniversityId = model.UniversityId,
			Rut = model.Rut,
		};
		var roles = new List<string>();
		var assistantTeacherRole = model.IsAdministrator ? Roles.Administrator.ToString() : null;
		if (assistantTeacherRole is not null) {
			roles.Add(assistantTeacherRole);
		}
		var teacherRole = model.IsTeacher ? Roles.Teacher.ToString() : null;
		if (teacherRole is not null) {
			roles.Add(teacherRole);
		}
		var studentRole = model.IsStudent ? Roles.Student.ToString() : null;
		if (studentRole is not null) {
			roles.Add(studentRole);
		}
		await this._userStore.SetUserNameAsync(user, userName: model.Email, CancellationToken.None);
		await this._emailStore.SetEmailAsync(user, model.Email, CancellationToken.None);
		var createResult = await this._userManager.CreateAsync(user, model.Password!);
		if (createResult.Succeeded) {
			var rolesResult = await this._userManager.AddToRolesAsync(user, roles);
			if (rolesResult.Succeeded) {
				this.ViewBag.SuccessMessage = "Usuario creado con éxito.";
				this.ModelState.Clear();
				return this.View();
			}
			this.ViewBag.WarningMessage = "Usuario creado, pero no se le pudo asignar el(los) rol(es).";
			this.ViewBag.WarningMessages = rolesResult.Errors.Select(w => w.Description);
			this.ModelState.Clear();
			return this.View();
		}
		if (createResult.Errors.Any()) {
			this.ViewBag.ErrorMessages = createResult.Errors.Select(e => e.Description);
		}
		this.ViewBag.ErrorMessage = "Error al crear el usuario.";
		return this.View();
	}

	public IActionResult ManageUsers(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		this.ViewData["FirstNameSortParam"] = sortOrder == "FirstName" ? "FirstNameDesc" : "FirstName";
		this.ViewData["LastNameSortParam"] = sortOrder == "LastName" ? "LastNameDesc" : "LastName";
		this.ViewData["UniversityIdSortParam"] = sortOrder == "UniversityId" ? "UniversityIdDesc" : "UniversityId";
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
			"UniversityId" => this._userManager.Users.OrderBy(u => u.UniversityId).ToList(),
			"UniversityIdDesc" => this._userManager.Users.OrderByDescending(u => u.UniversityId).ToList(),
			"Rut" => this._userManager.Users.OrderBy(u => u.Rut).ToList(),
			"RutDesc" => this._userManager.Users.OrderByDescending(u => u.Rut).ToList(),
			"Email" => this._userManager.Users.OrderBy(u => u.Email).ToList(),
			"EmailDesc" => this._userManager.Users.OrderByDescending(u => u.Email).ToList(),
			"LastName" => this._userManager.Users.OrderBy(u => u.LastName).ToList(),
			"LastNameDesc" => this._userManager.Users.OrderByDescending(u => u.LastName).ToList(),
			_ => this._userManager.Users.OrderBy(u => u.LastName).ToList()
		};
		if (!string.IsNullOrEmpty(searchString)) {
			users = users.Where(s => s.FirstName!.ToUpper().Contains(searchString.ToUpper()) || s.LastName!.ToUpper().Contains(searchString.ToUpper()) || s.UniversityId!.ToUpper().Contains(searchString.ToUpper()) || s.Rut!.ToUpper().Contains(searchString.ToUpper()) || s.Email == searchString).ToList();
		}
		var usersViewModel = users.Select(u => new ManageUsersViewModel {
			Id = u.Id,
			FirstName = u.FirstName,
			LastName = u.LastName,
			UniversityId = u.UniversityId,
			Rut = u.Rut,
			Email = u.Email
		});
		var pageSize = 10;
		return this.View(PaginatedList<ManageUsersViewModel>.Create(usersViewModel.AsQueryable(), pageNumber ?? 1, pageSize));
	}
}
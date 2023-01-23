using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

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

	public IActionResult Index(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
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
		var usersViewModel = users.Select(u => new IndexViewModel {
			Id = u.Id,
			FirstName = u.FirstName,
			LastName = u.LastName,
			UniversityId = u.UniversityId,
			Rut = u.Rut,
			Email = u.Email
		});
		var pageSize = 6;
		return this.View(PaginatedList<IndexViewModel>.Create(usersViewModel.AsQueryable(), pageNumber ?? 1, pageSize));
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
			this.ViewBag.WarningMessages = rolesResult.Errors.Select(w => w.Description).ToList();
			this.ModelState.Clear();
			return this.View();
		}
		if (createResult.Errors.Any()) {
			this.ViewBag.ErrorMessages = createResult.Errors.Select(e => e.Description).ToList();
		}
		this.ViewBag.ErrorMessage = "Error al crear el usuario.";
		return this.View();
	}

	public async Task<IActionResult> EditUser(string id) {
		var user = this._userManager.Users.FirstOrDefault(u => u.Id == id);
		if (user is null) {
			this.ViewBag.ErrorMessage = "Error al obtener al usuario.";
			return this.View();
		}
		var editUserViewModel = new EditUserViewModel {
			FirstName = user.FirstName,
			LastName = user.LastName,
			UniversityId = user.UniversityId,
			Rut = user.Rut,
			Email = user.Email
		};
		var userRoles = (await this._userManager.GetRolesAsync(user)).ToList();
		editUserViewModel.IsAdministrator = userRoles.Contains(Roles.Administrator.ToString());
		editUserViewModel.IsTeacher = userRoles.Contains(Roles.Teacher.ToString());
		editUserViewModel.IsStudent = userRoles.Contains(Roles.Student.ToString());
		return this.View(editUserViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> EditUser(string id, [FromForm] EditUserViewModel model) {
		var user = this._userManager.Users.FirstOrDefault(u => u.Id == id);
		if (user is null) {
			this.ViewBag.ErrorMessage = "Error al obtener el usuario.";
			return this.View();
		}
		if (!model.CurrentPassword.IsNullOrEmpty() && !model.NewPassword.IsNullOrEmpty() && !model.ConfirmNewPassword.IsNullOrEmpty()) {
			var passwordResult = await this._userManager.ChangePasswordAsync(user, model.CurrentPassword!, model.NewPassword!);
			if (!passwordResult.Succeeded) {
				this.ViewBag.ErrorMessage = "Error al cambiar la contraseña del usuario.";
				this.ViewBag.ErrorMessages = passwordResult.Errors.Select(e => e.Description).ToList();
				return this.View();
			}
		}
		var userRoles = (await this._userManager.GetRolesAsync(user)).ToList();
		var result = await this._userManager.RemoveFromRolesAsync(user, userRoles);
		if (!result.Succeeded) {
			this.ViewBag.ErrorMessage = "Error al eliminar los roles del usuario.";
			this.ViewBag.ErrorMessages = result.Errors.Select(e => e.Description).ToList();
			return this.View();
		}
		var roles = new List<string>();
		if (model.IsAdministrator) {
			roles.Add(Roles.Administrator.ToString());
		}
		if (model.IsTeacher) {
			roles.Add(Roles.Teacher.ToString());
		}
		if (model.IsStudent) {
			roles.Add(Roles.Student.ToString());
		}
		await this._userStore.SetUserNameAsync(user, userName: model.Email, CancellationToken.None);
		await this._emailStore.SetEmailAsync(user, model.Email, CancellationToken.None);
		user.FirstName = !model.FirstName.IsNullOrEmpty() ? model.FirstName : user.FirstName;
		user.LastName = !model.LastName.IsNullOrEmpty() ? model.LastName : user.LastName;
		user.UniversityId = !model.UniversityId.IsNullOrEmpty() ? model.UniversityId : user.UniversityId;
		user.Rut = !model.Rut.IsNullOrEmpty() ? model.Rut : user.Rut;
		var updateResult = await this._userManager.UpdateAsync(user);
		if (updateResult.Succeeded) {
			var rolesResult = await this._userManager.AddToRolesAsync(user, roles);
			if (rolesResult.Succeeded) {
				this.ViewBag.SuccessMessage = "Usuario actualizado con éxito.";
				return this.View();
			}
			this.ViewBag.WarningMessage = "Usuario actualizado, pero no se le pudo asignar el(los) rol(es).";
			this.ViewBag.WarningMessages = rolesResult.Errors.Select(w => w.Description).ToList();
			return this.View();
		}
		if (updateResult.Errors.Any()) {
			this.ViewBag.ErrorMessages = updateResult.Errors.Select(e => e.Description).ToList();
		}
		this.ViewBag.ErrorMessage = "Error al actualizar el usuario.";
		return this.View();
	}

	public IActionResult DeleteUser(string id) {
		var user = this._userManager.Users.FirstOrDefault(u => u.Id == id);
		if (user is null) {
			this.ViewBag.ErrorMessage = "Error al obtener el usuario.";
			return this.View();
		}
		var deleteUserViewModel = new DeleteUserViewModel {
			Id = user.Id,
			Email = user.Email
		};
		return this.View(deleteUserViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> DeleteUser(string id, [FromForm] DeleteUserViewModel model) {
		var user = this._userManager.Users.FirstOrDefault(u => u.Id == id);
		if (user is null) {
			this.ViewBag.ErrorMessage = "Error al obtener el usuario.";
			return this.View();
		}
		if (user!.Id == this._userManager.GetUserId(this.User)) {
			this.ViewBag.ErrorMessage = "No te puedes eliminar a tí mismo.";
			return this.View();
		}
		if (user is null) {
			this.ViewBag.ErrorMessage = "Error al obtener el usuario.";
			return this.View();
		}
		var result = await this._userManager.DeleteAsync(user);
		if (result.Succeeded) {
			this.ViewBag.SuccessMessage = "Usuario eliminado con éxito.";
			return this.View();
		}
		this.ViewBag.ErrorMessage = "Error al eliminar el usuario.";
		this.ViewBag.ErrorMessages = result.Errors.Select(e => e.Description).ToList();
		return this.View();
	}
}
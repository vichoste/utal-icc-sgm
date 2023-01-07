using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Utal.Icc.Sgm.Areas.Account.Models;
using Utal.Icc.Sgm.Areas.Account.Views.Administrator;

namespace Utal.Icc.Sgm.Areas.Account.Controllers;

[Area("Account"), Authorize(Roles = "Administrator")]
public class AdministratorController : Controller {
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly IUserStore<ApplicationUser> _userStore;
	private readonly IUserEmailStore<ApplicationUser> _emailStore;
	private readonly RoleManager<IdentityRole> _roleManager;

	public AdministratorController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore, RoleManager<IdentityRole> roleManager) {
		this._signInManager = signInManager;
		this._userManager = userManager;
		this._userStore = userStore;
		this._emailStore = (IUserEmailStore<ApplicationUser>)this._userStore;
		this._roleManager = roleManager;
	}

	public IActionResult CreateUser() {
		var roleViewModels = new List<CreateUserViewModel.RoleViewModel>();
		foreach (var role in this._roleManager.Roles.ToList()) {
			var roleViewModel = new CreateUserViewModel.RoleViewModel {
				Name = SpanishRoles.TranslateRoleStringToSpanish(role!.Name),
				IsSelected = false
			};
			roleViewModels.Add(roleViewModel);
		}
		this.ViewBag.RoleViewModels = roleViewModels;
		return this.View();
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> CreateUser([FromForm] CreateUserViewModel model) {
		if (!this.ModelState.IsValid) {
			this.ViewBag.ErrorMessage = "Revisa que los campos estén correctos.";
			return this.View();
		}
		var user = new ApplicationUser {
			FirstName = model.FirstName,
			LastName = model.LastName
		};
		await this._userStore.SetUserNameAsync(user, userName: model.Email, CancellationToken.None);
		await this._emailStore.SetEmailAsync(user, model.Email, CancellationToken.None);
		var createResult = await this._userManager.CreateAsync(user, model.Password!);
		if (createResult.Succeeded) {
			var rolesResult = await this._userManager.AddToRolesAsync(user, model.Roles!);
			if (rolesResult.Succeeded) {
				this.ViewBag.SuccessMessage = "Usuario creado con éxito.";
				return this.View();
			}
			this.ViewBag.WarningMessage = "Usuario creado, pero no se le pudo asignar el(los) rol(es).";
			this.ViewBag.WarningMessages = rolesResult.Errors.Select(w => w.Description);
			return this.View();
		}
		if (createResult.Errors.Any()) {
			this.ViewBag.ErrorMessages = createResult.Errors.Select(e => e.Description);
		}
		this.ViewBag.ErrorMessage = "Error al crear el usuario.";
		return this.View();
	}
}
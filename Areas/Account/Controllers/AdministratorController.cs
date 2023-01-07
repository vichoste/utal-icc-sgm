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
		this._emailStore = this.GetEmailStore();
		this._roleManager = roleManager;
	}

	public IActionResult CreateUser() => this.View();

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> CreateUser([FromForm] CreateUserModel model) {
		if (!this.ModelState.IsValid) {
			this.ViewBag.ErrorMessage = "Revisa que los campos estén correctos.";
			return this.View();
		}
		var user = CreateUserInstance();
		user.FirstName = model.FirstName;
		user.LastName = model.LastName;
		await this._userStore.SetUserNameAsync(user, userName: model.Email, CancellationToken.None);
		await this._emailStore.SetEmailAsync(user, model.Email, CancellationToken.None);
		var result = await this._userManager.CreateAsync(user, model.Password!);
		if (result.Succeeded) {
			this.ViewBag.SuccessMessage = "Usuario creado con éxito.";
			return this.View();
		}
		if (result.Errors.Any()) {
			this.ViewBag.ErrorMessages = result.Errors.ToList();
		}
		this.ViewBag.ErrorMessage = "Error al crear el usuario.";
		return this.View();
	}

	private static ApplicationUser CreateUserInstance() {
		try {
			return Activator.CreateInstance<ApplicationUser>();
		} catch {
			throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
				$"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
				$"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
		}
	}

	private IUserEmailStore<ApplicationUser> GetEmailStore() {
		return !this._userManager.SupportsUserEmail
			? throw new NotSupportedException("The default UI requires a user store with email support.")
			: (IUserEmailStore<ApplicationUser>)this._userStore;
	}
}
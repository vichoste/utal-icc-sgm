using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Utal.Icc.Sgm.Areas.Account.Views.Profile;
using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Areas.Account.Controllers;

[Area("Account")]
public class ProfileController : Controller {
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly IUserStore<ApplicationUser> _userStore;
	private readonly IUserEmailStore<ApplicationUser> _emailStore;
	private readonly RoleManager<IdentityRole> _roleManager;

	public ProfileController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore, RoleManager<IdentityRole> roleManager) {
		this._signInManager = signInManager;
		this._userManager = userManager;
		this._userStore = userStore;
		this._emailStore = (IUserEmailStore<ApplicationUser>)this._userStore;
		this._roleManager = roleManager;
	}

	public async Task<IActionResult> Index() {
		if (!this.User.Identity!.IsAuthenticated) {
			return this.RedirectToAction("Index", "SignIn", new { area = "Account" });
		}
		var user = await this._userManager.GetUserAsync(this.User);
		var indexViewModel = new IndexViewModel {
			FirstName = user!.FirstName,
			LastName = user.LastName,
			Rut = user.Rut,
			Email = await this._emailStore.GetEmailAsync(user, CancellationToken.None)
		};
		return this.View(indexViewModel);
	}

	public IActionResult ChangePassword() => !this.User.Identity!.IsAuthenticated ? this.RedirectToAction("Index", "SignIn", new { area = "Account" }) : this.View();

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> ChangePassword(string id, [FromForm] ChangePasswordViewModel model) {
		if (!this.User.Identity!.IsAuthenticated) {
			return this.RedirectToAction("Index", "SignIn", new { area = "Account" });
		}
		var user = await this._userManager.GetUserAsync(this.User);
		var passwordResult = await this._userManager.ChangePasswordAsync(user!, model.CurrentPassword!, model.NewPassword!);
		if (!passwordResult.Succeeded) {
			this.ViewBag.ErrorMessage = "Error al cambiar la contraseña.";
			this.ViewBag.ErrorMessages = passwordResult.Errors.Select(e => e.Description).ToList();
			return this.View();
		}
		this.ViewBag.SuccessMessage = "Se cambió la contraseña con éxito.";
		this.ModelState.Clear();
		return this.View();
	}
}
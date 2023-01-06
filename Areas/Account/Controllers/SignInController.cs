using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Utal.Icc.Sgm.Areas.Account.Models;
using Utal.Icc.Sgm.Areas.Account.Views.SignIn;

namespace Utal.Icc.Sgm.Areas.Account.Controllers;

[Area("Account")]
public class SignInController : Controller {
	private readonly SignInManager<ApplicationUser> _signInManager;

	public SignInController(SignInManager<ApplicationUser> signInManager) => this._signInManager = signInManager;

	public IActionResult Index() => this.User.Identity!.IsAuthenticated ? this.RedirectToAction("Index", "Home", new { area = "" }) : this.View();

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Index([FromForm] IndexModel model) {
		if (this.User.Identity!.IsAuthenticated) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (!this.ModelState.IsValid) {
			this.ViewBag.ErrorMessage = "Revisa que los campos estén correctos.";
			return this.View();
		}
		var result = await this._signInManager.PasswordSignInAsync(model.Email!, model.Password!, model.RememberMe, false);
		if (!result.Succeeded) {
			this.ViewBag.ErrorMessage = "Error al iniciar sesión.";
			return this.View();
		}
		return this.RedirectToAction("Index", "Home", new { area = "" });
	}
}
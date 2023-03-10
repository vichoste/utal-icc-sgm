using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Utal.Icc.Sgm.Areas.Account.ViewModels.SignIn;
using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Areas.Account.Controllers;

[Area("Account")]
public class SignInController : Controller {
	private readonly SignInManager<ApplicationUser> _signInManager;

	public SignInController(SignInManager<ApplicationUser> signInManager) => this._signInManager = signInManager;

	public IActionResult Index() => this.User.Identity!.IsAuthenticated ? this.RedirectToAction("Index", "Home", new { area = string.Empty }) : this.View();

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Index([FromForm] IndexViewModel model) {
		if (this.User.Identity!.IsAuthenticated) {
			return this.RedirectToAction("Index", "Home", new { area = string.Empty });
		}
		var result = await this._signInManager.PasswordSignInAsync(model.Email!, model.Password!, model.RememberMe, false);
		if (!result.Succeeded) {
			this.ViewBag.ErrorMessage = "Credenciales incorrectas.";
			return this.View(new IndexViewModel());
		}
		return this.RedirectToAction("Index", "Home", new { area = string.Empty });
	}
}
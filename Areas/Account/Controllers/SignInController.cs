using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Utal.Icc.Sgm.Areas.Account.Models;
using Utal.Icc.Sgm.Areas.Account.Views.SignIn;

namespace Utal.Icc.Sgm.Areas.Account.Controllers;

public class SignInController : Controller {
	private readonly SignInManager<ApplicationUser> _signInManager;
	public SignInController(SignInManager<ApplicationUser> signInManager) => this._signInManager = signInManager;
	public IActionResult Index() => this.View();

	public IActionResult Error() => this.View();

	[ValidateAntiForgeryToken]
	public async Task<IActionResult> OnPost([FromForm] IndexModel model) {
		if (!this.ModelState.IsValid) {
			return this.RedirectToAction("Error", "SignIn", new { area = "Account" });
		}
		var result = await this._signInManager.PasswordSignInAsync(model.Email!, model.Password!, model.RememberMe, false);
		return !result.Succeeded
			? this.RedirectToAction("Error", "SignIn", new { area = "Account" })
			: (IActionResult)this.RedirectToAction("Index", "Home", new { area = "" });
	}
}
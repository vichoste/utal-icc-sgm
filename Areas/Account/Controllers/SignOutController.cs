using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Utal.Icc.Sgm.Areas.Account.Models;

namespace Utal.Icc.Sgm.Areas.Account.Controllers;

[Area("Account")]
public class SignOutController : Controller {
	private readonly SignInManager<ApplicationUser> _signInManager;

	public SignOutController(SignInManager<ApplicationUser> signInManager) => this._signInManager = signInManager;

	public async Task<IActionResult> Index() {
		if (this.User.Identity!.IsAuthenticated) {
			await this._signInManager.SignOutAsync();
		}
		return this.RedirectToAction("Index", "Home", new { area = "" });
	}
}
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Utal.Icc.Sgm.Areas.Account.ViewModels.SignIn;
using Utal.Icc.Sgm.Controllers;
using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Areas.Account.Controllers;

[Area(nameof(Account))]
public class SignInController : ApplicationController {
	public SignInController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore, IUserEmailStore<ApplicationUser> emailStore, SignInManager<ApplicationUser> signInManager) : base(dbContext, userManager, userStore, emailStore, signInManager) { }

	public IActionResult Index() => this.User.Identity!.IsAuthenticated ? this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty }) : this.View();

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Index([FromForm] IndexViewModel model) {
		if (this.User.Identity!.IsAuthenticated) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var result = await this._signInManager.PasswordSignInAsync(model.Email!, model.Password!, model.RememberMe, false);
		if (!result.Succeeded) {
			this.ViewBag.ErrorMessage = "Credenciales incorrectas.";
			return this.View(new IndexViewModel());
		}
		return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
	}
}
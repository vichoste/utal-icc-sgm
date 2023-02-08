using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Utal.Icc.Sgm.Controllers;
using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Areas.Account.Controllers;

[Area(nameof(Account))]
public class SignOutController : ApplicationController {
	public SignOutController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore, IUserEmailStore<ApplicationUser> emailStore, SignInManager<ApplicationUser> signInManager) : base(dbContext, userManager, userStore, emailStore, signInManager) { }

	public async Task<IActionResult> Index() {
		if (this.User.Identity!.IsAuthenticated) {
			await this._signInManager.SignOutAsync();
			this.TempData["SuccessMessage"] = "Se ha cerrado tu sesión correctamente.";
		}
		return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
	}
}
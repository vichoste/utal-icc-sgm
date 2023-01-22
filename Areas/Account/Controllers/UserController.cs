using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Utal.Icc.Sgm.Areas.Account.Models;
using Utal.Icc.Sgm.Areas.Account.Views.User;

namespace Utal.Icc.Sgm.Areas.Account.Controllers;

[Area("Account")]
public class UserController : Controller {
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly IUserStore<ApplicationUser> _userStore;
	private readonly IUserEmailStore<ApplicationUser> _emailStore;
	private readonly RoleManager<IdentityRole> _roleManager;

	public UserController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore, RoleManager<IdentityRole> roleManager) {
		this._signInManager = signInManager;
		this._userManager = userManager;
		this._userStore = userStore;
		this._emailStore = (IUserEmailStore<ApplicationUser>)this._userStore;
		this._roleManager = roleManager;
	}
	
	public async Task<IActionResult> Index() {
		var user = await this._userManager.GetUserAsync(this.User);
		if (!this.User.Identity!.IsAuthenticated) {
			return this.RedirectToAction("Index", "SignIn", new { area = "Account"});
		}
		var indexViewModel = new IndexViewModel {
			FirstName = user!.FirstName,
			LastName = user.LastName,
			Rut = user.Rut,
			UniversityId = user.UniversityId,
			Email = await this._emailStore.GetEmailAsync(user, CancellationToken.None),
			IsAdministrator = await this._userManager.IsInRoleAsync(user, "Administrator"),
			IsTeacher = await this._userManager.IsInRoleAsync(user, "Teacher"),
			IsStudent = await this._userManager.IsInRoleAsync(user, "Student")
		};
		return this.View(indexViewModel);
	}
}
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Utal.Icc.Sgm.Areas.Account.Models;
using Utal.Icc.Sgm.Areas.Account.Views.SignUp;

namespace Utal.Icc.Sgm.Areas.Account.Controllers;
public class SignUpController : Controller {
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly IUserStore<ApplicationUser> _userStore;
	private readonly IUserEmailStore<ApplicationUser> _emailStore;

	public SignUpController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore) {
		this._signInManager = signInManager;
		this._userManager = userManager;
		this._userStore = userStore;
		this._emailStore = this.GetEmailStore();
	}

	public IActionResult Index() => this.View();

	public IActionResult Error() => this.View();

	[ValidateAntiForgeryToken]
	public async Task<IActionResult> OnPost([FromForm] IndexModel model) {
		if (!this.ModelState.IsValid) {
			return this.RedirectToAction("Error", "SignUp", new { area = "Account" });
		}
		var user = CreateUser();
		user.FirstName = model.FirstName;
		user.LastName = model.LastName;
		await this._userStore.SetUserNameAsync(user, userName: model.Email, CancellationToken.None);
		await this._emailStore.SetEmailAsync(user, model.Email, CancellationToken.None);
		var result = await this._userManager.CreateAsync(user, model.Password!);
		if (result.Succeeded) {
			await this._signInManager.SignInAsync(user, isPersistent: false);
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		// TODO: Reveal password errors while signing up
		return this.RedirectToAction("Error", "SignUp", new { area = "Account" });
	}

	private static ApplicationUser CreateUser() {
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
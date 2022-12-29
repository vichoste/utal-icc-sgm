// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Areas.Identity.Pages.Account.Manage;

public class TwoFactorAuthenticationModel : PageModel {
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly SignInManager<ApplicationUser> _signInManager;

	public TwoFactorAuthenticationModel(
		UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager) {
		this._userManager = userManager;
		this._signInManager = signInManager;
	}

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	public bool HasAuthenticator { get; set; }

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	public int RecoveryCodesLeft { get; set; }

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	[BindProperty]
	public bool Is2faEnabled { get; set; }

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	public bool IsMachineRemembered { get; set; }

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	[TempData]
	public string StatusMessage { get; set; }

	public async Task<IActionResult> OnGetAsync() {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user == null) {
			return this.NotFound($"No se pudo cargar el usuario con el ID '{this._userManager.GetUserId(this.User)}'.");
		}

		this.HasAuthenticator = await this._userManager.GetAuthenticatorKeyAsync(user) != null;
		this.Is2faEnabled = await this._userManager.GetTwoFactorEnabledAsync(user);
		this.IsMachineRemembered = await this._signInManager.IsTwoFactorClientRememberedAsync(user);
		this.RecoveryCodesLeft = await this._userManager.CountRecoveryCodesAsync(user);

		return this.Page();
	}

	public async Task<IActionResult> OnPostAsync() {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user == null) {
			return this.NotFound($"No se pudo cargar el usuario con el ID '{this._userManager.GetUserId(this.User)}'.");
		}

		await this._signInManager.ForgetTwoFactorClientAsync();
		this.StatusMessage = "El explorador Web ha sido olvidao. Durante el próximo inicio de sesión, será requerido el código de autenticación en dos pasos (2FA).";
		return this.RedirectToPage();
	}
}
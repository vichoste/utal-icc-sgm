// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Utal.Icc.Sgm.Areas.Identity.Pages.Account.Manage;

public class TwoFactorAuthenticationModel : PageModel {
	private readonly UserManager<IdentityUser> _userManager;
	private readonly SignInManager<IdentityUser> _signInManager;
	private readonly ILogger<TwoFactorAuthenticationModel> _logger;

	public TwoFactorAuthenticationModel(
		UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ILogger<TwoFactorAuthenticationModel> logger) {
		_userManager = userManager;
		_signInManager = signInManager;
		_logger = logger;
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
		var user = await _userManager.GetUserAsync(User);
		if (user == null) {
			return NotFound($"No se pudo cargar el usuario con el ID '{_userManager.GetUserId(User)}'.");
		}

		HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null;
		Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
		IsMachineRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user);
		RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user);

		return Page();
	}

	public async Task<IActionResult> OnPostAsync() {
		var user = await _userManager.GetUserAsync(User);
		if (user == null) {
			return NotFound($"No se pudo cargar el usuario con el ID '{_userManager.GetUserId(User)}'.");
		}

		await _signInManager.ForgetTwoFactorClientAsync();
		StatusMessage = "El explorador Web ha sido olvidao. Durante el próximo inicio de sesión, será requerido el código de autenticación en dos pasos (2FA).";
		return RedirectToPage();
	}
}
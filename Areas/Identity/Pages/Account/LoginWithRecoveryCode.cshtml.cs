// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Areas.Identity.Pages.Account;

public class LoginWithRecoveryCodeModel : PageModel {
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly ILogger<LoginWithRecoveryCodeModel> _logger;

	public LoginWithRecoveryCodeModel(
		SignInManager<ApplicationUser> signInManager,
		UserManager<ApplicationUser> userManager,
		ILogger<LoginWithRecoveryCodeModel> logger) {
		this._signInManager = signInManager;
		this._userManager = userManager;
		this._logger = logger;
	}

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	[BindProperty]
	public InputModel Input { get; set; }

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	public string ReturnUrl { get; set; }

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	public class InputModel {
		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[BindProperty]
		[Required]
		[DataType(DataType.Text)]
		[Display(Name = "Código de recuperación")]
		public string RecoveryCode { get; set; }
	}

	public async Task<IActionResult> OnGetAsync(string returnUrl = null) {
		// Ensure the user has gone through the username & password screen first
		var user = await this._signInManager.GetTwoFactorAuthenticationUserAsync();
		if (user == null) {
			throw new InvalidOperationException($"No se pudo cargar al usuario de autenticación en dos pasos (2FA).");
		}

		this.ReturnUrl = returnUrl;

		return this.Page();
	}

	public async Task<IActionResult> OnPostAsync(string returnUrl = null) {
		if (!this.ModelState.IsValid) {
			return this.Page();
		}

		var user = await this._signInManager.GetTwoFactorAuthenticationUserAsync();
		if (user == null) {
			throw new InvalidOperationException($"No se pudo cargar al usuario de autenticación en dos pasos (2FA).");
		}

		var recoveryCode = this.Input.RecoveryCode.Replace(" ", string.Empty);

		var result = await this._signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

		_ = await this._userManager.GetUserIdAsync(user);

		if (result.Succeeded) {
			this._logger.LogInformation("Usuario con ID '{UserId}' ha iniciado sesión con un código de recuperación.", user.Id);
			return this.LocalRedirect(returnUrl ?? this.Url.Content("~/"));
		}
		if (result.IsLockedOut) {
			this._logger.LogWarning("La cuenta está bloqueada.");
			return this.RedirectToPage("./Lockout");
		} else {
			this._logger.LogWarning("Código inválido para el usuario con ID '{UserId}' ", user.Id);
			this.ModelState.AddModelError(string.Empty, "Código de recuperación inválido.");
			return this.Page();
		}
	}
}
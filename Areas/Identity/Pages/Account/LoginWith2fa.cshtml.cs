// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Areas.Identity.Pages.Account;

public class LoginWith2faModel : PageModel {
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly ILogger<LoginWith2faModel> _logger;

	public LoginWith2faModel(
		SignInManager<ApplicationUser> signInManager,
		UserManager<ApplicationUser> userManager,
		ILogger<LoginWith2faModel> logger) {
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
	public bool RememberMe { get; set; }

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
		[Required]
		[StringLength(7, ErrorMessage = "El {0} debe tener al menos {2} y como máximo {1} carácteres de largo.", MinimumLength = 6)]
		[DataType(DataType.Text)]
		[Display(Name = "Código del autenticador")]
		public string TwoFactorCode { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[Display(Name = "Recordar este explorador Web")]
		public bool RememberMachine { get; set; }
	}

	public async Task<IActionResult> OnGetAsync(bool rememberMe, string returnUrl = null) {
		// Ensure the user has gone through the username & password screen first
		var user = await this._signInManager.GetTwoFactorAuthenticationUserAsync();

		if (user == null) {
			throw new InvalidOperationException($"No se pudo cargar el usuario con autenticación en dos pasos (2FA).");
		}

		this.ReturnUrl = returnUrl;
		this.RememberMe = rememberMe;

		return this.Page();
	}

	public async Task<IActionResult> OnPostAsync(bool rememberMe, string returnUrl = null) {
		if (!this.ModelState.IsValid) {
			return this.Page();
		}

		returnUrl ??= this.Url.Content("~/");

		var user = await this._signInManager.GetTwoFactorAuthenticationUserAsync();
		if (user == null) {
			throw new InvalidOperationException($"No se pudo cargar el usuario con autenticación en dos pasos (2FA).");
		}

		var authenticatorCode = this.Input.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

		var result = await this._signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, this.Input.RememberMachine);

		_ = await this._userManager.GetUserIdAsync(user);

		if (result.Succeeded) {
			this._logger.LogInformation("Usuario con ID '{UserId}' ha iniciado sesión mediante autenticación en dos pasos (2FA).", user.Id);
			return this.LocalRedirect(returnUrl);
		} else if (result.IsLockedOut) {
			this._logger.LogWarning("El usuario con ID '{UserId}' tiene la cuenta bloqueada.", user.Id);
			return this.RedirectToPage("./Lockout");
		} else {
			this._logger.LogWarning("Código del autenticador inválido para el usuario con ID '{UserId}'.", user.Id);
			this.ModelState.AddModelError(string.Empty, "Código del autenticador inválido.");
			return this.Page();
		}
	}
}
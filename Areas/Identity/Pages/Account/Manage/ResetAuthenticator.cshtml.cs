// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Areas.Identity.Pages.Account.Manage;

public class ResetAuthenticatorModel : PageModel {
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly ILogger<ResetAuthenticatorModel> _logger;

	public ResetAuthenticatorModel(
		UserManager<ApplicationUser> userManager,
		SignInManager<ApplicationUser> signInManager,
		ILogger<ResetAuthenticatorModel> logger) {
		this._userManager = userManager;
		this._signInManager = signInManager;
		this._logger = logger;
	}

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	[TempData]
	public string StatusMessage { get; set; }

	public async Task<IActionResult> OnGet() {
		var user = await this._userManager.GetUserAsync(this.User);
		return user == null ? this.NotFound($"No se pudo cargar el usuario con el ID '{this._userManager.GetUserId(this.User)}'.") : this.Page();
	}

	public async Task<IActionResult> OnPostAsync() {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user == null) {
			return this.NotFound($"No se pudo cargar el usuario con el ID '{this._userManager.GetUserId(this.User)}'.");
		}

		_ = await this._userManager.SetTwoFactorEnabledAsync(user, false);
		_ = await this._userManager.ResetAuthenticatorKeyAsync(user);
		_ = await this._userManager.GetUserIdAsync(user);
		this._logger.LogInformation("El usuario con el ID '{UserId}' ha reestablecido su llave de autenticación de dos pasos.", user.Id);

		await this._signInManager.RefreshSignInAsync(user);
		this.StatusMessage = "Tu llave de autenticación ha sido reestablecida, deberás configurar tu aplicación utilizando la nueva llave.";

		return this.RedirectToPage("./EnableAuthenticator");
	}
}
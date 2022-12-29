// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Areas.Identity.Pages.Account.Manage;

public class GenerateRecoveryCodesModel : PageModel {
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly ILogger<GenerateRecoveryCodesModel> _logger;

	public GenerateRecoveryCodesModel(
		UserManager<ApplicationUser> userManager,
		ILogger<GenerateRecoveryCodesModel> logger) {
		this._userManager = userManager;
		this._logger = logger;
	}

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	[TempData]
	public string[] RecoveryCodes { get; set; }

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

		var isTwoFactorEnabled = await this._userManager.GetTwoFactorEnabledAsync(user);
		return !isTwoFactorEnabled
			? throw new InvalidOperationException($"No se pueden generar códigos de recuperación porque el usuario no tiene la autenticación en dos pasos (2FA) habilitada.")
			: (IActionResult)this.Page();
	}

	public async Task<IActionResult> OnPostAsync() {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user == null) {
			return this.NotFound($"No se pudo cargar el usuario con el ID '{this._userManager.GetUserId(this.User)}'.");
		}

		var isTwoFactorEnabled = await this._userManager.GetTwoFactorEnabledAsync(user);
		var userId = await this._userManager.GetUserIdAsync(user);
		if (!isTwoFactorEnabled) {
			throw new InvalidOperationException($"No se pueden generar códigos de recuperación porque el usuario no tiene la autenticación en dos pasos (2FA) habilitada.");
		}

		var recoveryCodes = await this._userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
		this.RecoveryCodes = recoveryCodes.ToArray();

		this._logger.LogInformation("El usuario con el ID '{UserId}' ha generado nuevos códigos de recuperación.", userId);
		this.StatusMessage = "Has generado nuevos códigos de recuperación.";
		return this.RedirectToPage("./ShowRecoveryCodes");
	}
}
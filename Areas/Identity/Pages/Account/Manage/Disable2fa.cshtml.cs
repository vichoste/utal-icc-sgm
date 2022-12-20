// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Utal.Icc.Sgm.Areas.Identity.Pages.Account.Manage;

public class Disable2faModel : PageModel {
	private readonly UserManager<IdentityUser> _userManager;
	private readonly ILogger<Disable2faModel> _logger;

	public Disable2faModel(
		UserManager<IdentityUser> userManager,
		ILogger<Disable2faModel> logger) {
		_userManager = userManager;
		_logger = logger;
	}

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	[TempData]
	public string StatusMessage { get; set; }

	public async Task<IActionResult> OnGet() {
		var user = await _userManager.GetUserAsync(User);
		return user == null
			? NotFound($"No se pudo cargar el usuario con el ID '{_userManager.GetUserId(User)}'.")
			: !await _userManager.GetTwoFactorEnabledAsync(user)
			? throw new InvalidOperationException($"No se puede deshabilitar la autenticación en dos pasos (2FA), ya que el usuario no la tiene activada.")
			: (IActionResult)Page();
	}

	public async Task<IActionResult> OnPostAsync() {
		var user = await _userManager.GetUserAsync(User);
		if (user == null) {
			return NotFound($"No se pudo cargar el usuario con el ID '{_userManager.GetUserId(User)}'.");
		}
		var disable2faResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
		if (!disable2faResult.Succeeded) {
			throw new InvalidOperationException($"Ocurrió un error inesperado al deshabilitar la autenticación en dos pasos (2FA).");
		}

		_logger.LogInformation("El usuario con ID '{UserId}' ha deshabilitado la autenticación en dos pasos (2FA).", _userManager.GetUserId(User));
		StatusMessage = "La autenticación en dos pasos (2FA) ha sido deshabilitada. Si deseas habilitarla nuevamente, usa tu aplicación autenticadora.";
		return RedirectToPage("./TwoFactorAuthentication");
	}
}
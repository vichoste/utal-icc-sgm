// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Utal.Icc.Sgm.Areas.Identity.Pages.Account.Manage;

public class ChangePasswordModel : PageModel {
	private readonly UserManager<IdentityUser> _userManager;
	private readonly SignInManager<IdentityUser> _signInManager;
	private readonly ILogger<ChangePasswordModel> _logger;

	public ChangePasswordModel(
		UserManager<IdentityUser> userManager,
		SignInManager<IdentityUser> signInManager,
		ILogger<ChangePasswordModel> logger) {
		_userManager = userManager;
		_signInManager = signInManager;
		_logger = logger;
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
	[TempData]
	public string StatusMessage { get; set; }

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
		[DataType(DataType.Password)]
		[Display(Name = "Contraseña actual")]
		public string OldPassword { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[Required]
		[StringLength(100, ErrorMessage = "La {0} debe ser de al menos {2} y como máximo {1} carácteres de largo.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "Nueva contraseña")]
		public string NewPassword { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[DataType(DataType.Password)]
		[Display(Name = "Confirmar nueva contraseña")]
		[Compare("NewPassword", ErrorMessage = "Las contraseñas proporcionadas no coinciden.")]
		public string ConfirmPassword { get; set; }
	}

	public async Task<IActionResult> OnGetAsync() {
		var user = await _userManager.GetUserAsync(User);
		if (user == null) {
			return NotFound($"No se pudo cargar el usuario con el ID '{_userManager.GetUserId(User)}'.");
		}

		var hasPassword = await _userManager.HasPasswordAsync(user);
		return !hasPassword ? RedirectToPage("./SetPassword") : Page();
	}

	public async Task<IActionResult> OnPostAsync() {
		if (!ModelState.IsValid) {
			return Page();
		}

		var user = await _userManager.GetUserAsync(User);
		if (user == null) {
			return NotFound($"No se pudo cargar el usuario con el ID '{_userManager.GetUserId(User)}'.");
		}

		var changePasswordResult = await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
		if (!changePasswordResult.Succeeded) {
			foreach (var error in changePasswordResult.Errors) {
				ModelState.AddModelError(string.Empty, error.Description);
			}
			return Page();
		}

		await _signInManager.RefreshSignInAsync(user);
		_logger.LogInformation("Usuario ha cambiado su contraseña exitosamente.");
		StatusMessage = "Tu contraseña ha sido cambiada.";

		return RedirectToPage();
	}
}
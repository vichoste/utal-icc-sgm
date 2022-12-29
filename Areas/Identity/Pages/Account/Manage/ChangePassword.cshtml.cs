// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Areas.Identity.Pages.Account.Manage;

public class ChangePasswordModel : PageModel {
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly ILogger<ChangePasswordModel> _logger;

	public ChangePasswordModel(
		UserManager<ApplicationUser> userManager,
		SignInManager<ApplicationUser> signInManager,
		ILogger<ChangePasswordModel> logger) {
		this._userManager = userManager;
		this._signInManager = signInManager;
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
		var user = await this._userManager.GetUserAsync(this.User);
		if (user == null) {
			return this.NotFound($"No se pudo cargar el usuario con el ID '{this._userManager.GetUserId(this.User)}'.");
		}

		var hasPassword = await this._userManager.HasPasswordAsync(user);
		return !hasPassword ? this.RedirectToPage("./SetPassword") : this.Page();
	}

	public async Task<IActionResult> OnPostAsync() {
		if (!this.ModelState.IsValid) {
			return this.Page();
		}

		var user = await this._userManager.GetUserAsync(this.User);
		if (user == null) {
			return this.NotFound($"No se pudo cargar el usuario con el ID '{this._userManager.GetUserId(this.User)}'.");
		}

		var changePasswordResult = await this._userManager.ChangePasswordAsync(user, this.Input.OldPassword, this.Input.NewPassword);
		if (!changePasswordResult.Succeeded) {
			foreach (var error in changePasswordResult.Errors) {
				this.ModelState.AddModelError(string.Empty, error.Description);
			}
			return this.Page();
		}

		await this._signInManager.RefreshSignInAsync(user);
		this._logger.LogInformation("Usuario ha cambiado su contraseña exitosamente.");
		this.StatusMessage = "Tu contraseña ha sido cambiada.";

		return this.RedirectToPage();
	}
}
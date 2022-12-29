// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Areas.Identity.Pages.Account;

public class ResetPasswordModel : PageModel {
	private readonly UserManager<ApplicationUser> _userManager;

	public ResetPasswordModel(UserManager<ApplicationUser> userManager) => this._userManager = userManager;

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
	public class InputModel {
		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[Required]
		[EmailAddress]
		public string Email { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[Required]
		[StringLength(100, ErrorMessage = "La {0} debe ser de al menos {2} y como máximo {1} carácteres de largo.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[DataType(DataType.Password)]
		[Display(Name = "Confirmar contraseña")]
		[Compare("Password", ErrorMessage = "Las contraseñas proporcionadas no coinciden.")]
		public string ConfirmPassword { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[Required]
		public string Code { get; set; }
	}

	public IActionResult OnGet(string code = null) {
		if (code == null) {
			return this.BadRequest("Se debe proporcionar un código para reestablecer la contraseña.");
		} else {
			this.Input = new InputModel {
				Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code))
			};
			return this.Page();
		}
	}

	public async Task<IActionResult> OnPostAsync() {
		if (!this.ModelState.IsValid) {
			return this.Page();
		}

		var user = await this._userManager.FindByEmailAsync(this.Input.Email);
		if (user == null) {
			// Don't reveal that the user does not exist
			return this.RedirectToPage("./ResetPasswordConfirmation");
		}

		var result = await this._userManager.ResetPasswordAsync(user, this.Input.Code, this.Input.Password);
		if (result.Succeeded) {
			return this.RedirectToPage("./ResetPasswordConfirmation");
		}

		foreach (var error in result.Errors) {
			this.ModelState.AddModelError(string.Empty, error.Description);
		}
		return this.Page();
	}
}
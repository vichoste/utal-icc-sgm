// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel {
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly IUserStore<ApplicationUser> _userStore;
	private readonly IUserEmailStore<ApplicationUser> _emailStore;
	private readonly ILogger<RegisterModel> _logger;
	private readonly IEmailSender _emailSender;

	public RegisterModel(
		UserManager<ApplicationUser> userManager,
		IUserStore<ApplicationUser> userStore,
		SignInManager<ApplicationUser> signInManager,
		ILogger<RegisterModel> logger,
		IEmailSender emailSender) {
		this._userManager = userManager;
		this._userStore = userStore;
		this._emailStore = this.GetEmailStore();
		this._signInManager = signInManager;
		this._logger = logger;
		this._emailSender = emailSender;
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
	public IList<AuthenticationScheme> ExternalLogins { get; set; }

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
		[Display(Name = "Email")]
		public string Email { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[Required]
		[StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y como máximo {1} carácteres de largo.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "Contraseña")]
		public string Password { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[DataType(DataType.Password)]
		[Display(Name = "Confirmar contraseña")]
		[Compare("Password", ErrorMessage = "La contraseñas proporcionadas no coinciden.")]
		public string ConfirmPassword { get; set; }

		[Required]
		[Display(Name = "Nombre")]
		public string FirstName { get; set; }
		[Required]
		[Display(Name = "Apellido")]
		public string LastName { get; set; }
	}

	public async Task OnGetAsync(string returnUrl = null) {
		this.ReturnUrl = returnUrl;
		this.ExternalLogins = (await this._signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
	}

	public async Task<IActionResult> OnPostAsync(string returnUrl = null) {
		returnUrl ??= this.Url.Content("~/");
		this.ExternalLogins = (await this._signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
		if (this.ModelState.IsValid) {
			var user = this.CreateUser();

			user.FirstName = this.Input.FirstName;
			user.LastName = this.Input.LastName;

			await this._userStore.SetUserNameAsync(user, this.Input.Email, CancellationToken.None);
			await this._emailStore.SetEmailAsync(user, this.Input.Email, CancellationToken.None);
			var result = await this._userManager.CreateAsync(user, this.Input.Password);

			if (result.Succeeded) {
				this._logger.LogInformation("Un usuario ha creado una cuenta con contraseña.");

				var userId = await this._userManager.GetUserIdAsync(user);
				var code = await this._userManager.GenerateEmailConfirmationTokenAsync(user);
				code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
				var callbackUrl = this.Url.Page(
					"/Account/ConfirmEmail",
					pageHandler: null,
					values: new { area = "Identity", userId, code, returnUrl },
					protocol: this.Request.Scheme);

				await this._emailSender.SendEmailAsync(this.Input.Email, "Confirma tu correo electrónico",
					$"Por favor, confirma tu cuenta haciendo <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>click aquí</a>.");

				if (this._userManager.Options.SignIn.RequireConfirmedAccount) {
					return this.RedirectToPage("RegisterConfirmation", new { email = this.Input.Email, returnUrl });
				} else {
					await this._signInManager.SignInAsync(user, isPersistent: false);
					return this.LocalRedirect(returnUrl);
				}
			}
			foreach (var error in result.Errors) {
				this.ModelState.AddModelError(string.Empty, error.Description);
			}
		}

		// If we got this far, something failed, redisplay form
		return this.Page();
	}

	private ApplicationUser CreateUser() {
		try {
			return Activator.CreateInstance<ApplicationUser>();
		} catch {
			throw new InvalidOperationException($"No se puede crear una instancia de '{nameof(ApplicationUser)}'. " +
				$"Asegúrate de que '{nameof(ApplicationUser)}' no es una clase abstracta y que tiene un constructor sin parámetros, o bien sobrescribe la página de inicio de sesión externa en /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
		}
	}

	private IUserEmailStore<ApplicationUser> GetEmailStore() {
		return !this._userManager.SupportsUserEmail
			? throw new NotSupportedException("La UI por defecto requiere un guardado de usuario con correo electrónico.")
			: (IUserEmailStore<ApplicationUser>)this._userStore;
	}
}
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
		_userManager = userManager;
		_userStore = userStore;
		_emailStore = GetEmailStore();
		_signInManager = signInManager;
		_logger = logger;
		_emailSender = emailSender;
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
	}


	public async Task OnGetAsync(string returnUrl = null) {
		ReturnUrl = returnUrl;
		ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
	}

	public async Task<IActionResult> OnPostAsync(string returnUrl = null) {
		returnUrl ??= Url.Content("~/");
		ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
		if (ModelState.IsValid) {
			var user = CreateUser();

			await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
			await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
			var result = await _userManager.CreateAsync(user, Input.Password);

			if (result.Succeeded) {
				_logger.LogInformation("Un usuario ha creado una cuenta con contraseña.");

				var userId = await _userManager.GetUserIdAsync(user);
				var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
				code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
				var callbackUrl = Url.Page(
					"/Account/ConfirmEmail",
					pageHandler: null,
					values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
					protocol: Request.Scheme);

				await _emailSender.SendEmailAsync(Input.Email, "Confirma tu correo electrónico",
					$"Por favor, confirma tu cuenta haciendo <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>click aquí</a>.");

				if (_userManager.Options.SignIn.RequireConfirmedAccount) {
					return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
				} else {
					await _signInManager.SignInAsync(user, isPersistent: false);
					return LocalRedirect(returnUrl);
				}
			}
			foreach (var error in result.Errors) {
				ModelState.AddModelError(string.Empty, error.Description);
			}
		}

		// If we got this far, something failed, redisplay form
		return Page();
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
		return !_userManager.SupportsUserEmail
			? throw new NotSupportedException("La UI por defecto requiere un guardado de usuario con correo electrónico.")
			: (IUserEmailStore<ApplicationUser>)_userStore;
	}
}
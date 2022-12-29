﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Areas.Identity.Pages.Account.Manage;

public class EmailModel : PageModel {
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly IEmailSender _emailSender;

	public EmailModel(
		UserManager<ApplicationUser> userManager, IEmailSender emailSender) {
		this._userManager = userManager;
		this._emailSender = emailSender;
	}

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	public string Email { get; set; }

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	public bool IsEmailConfirmed { get; set; }

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
		[Display(Name = "Nuevo correo electrónico")]
		public string NewEmail { get; set; }
	}

	private async Task LoadAsync(ApplicationUser user) {
		var email = await this._userManager.GetEmailAsync(user);
		this.Email = email;

		this.Input = new InputModel {
			NewEmail = email,
		};

		this.IsEmailConfirmed = await this._userManager.IsEmailConfirmedAsync(user);
	}

	public async Task<IActionResult> OnGetAsync() {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user == null) {
			return this.NotFound($"No se pudo cargar el usuario con el ID '{this._userManager.GetUserId(this.User)}'.");
		}

		await this.LoadAsync(user);
		return this.Page();
	}

	public async Task<IActionResult> OnPostChangeEmailAsync() {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user == null) {
			return this.NotFound($"No se pudo cargar el usuario con el ID '{this._userManager.GetUserId(this.User)}'.");
		}

		if (!this.ModelState.IsValid) {
			await this.LoadAsync(user);
			return this.Page();
		}

		var email = await this._userManager.GetEmailAsync(user);
		if (this.Input.NewEmail != email) {
			var userId = await this._userManager.GetUserIdAsync(user);
			var code = await this._userManager.GenerateChangeEmailTokenAsync(user, this.Input.NewEmail);
			code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
			var callbackUrl = this.Url.Page(
				"/Account/ConfirmEmailChange",
				pageHandler: null,
				values: new { area = "Identity", userId, email = this.Input.NewEmail, code },
				protocol: this.Request.Scheme);
			await this._emailSender.SendEmailAsync(
				this.Input.NewEmail,
				"Confirma tu correo electrónico",
				$"Por favor confirma tu correo electrónico <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>haciendo click aquí</a>.");

			this.StatusMessage = "El enlace para la confirmación de cambio de correo electrónico ha sido enviado. Por favor revisa tu bandeja de entrada.";
			return this.RedirectToPage();
		}

		this.StatusMessage = "Tu correo sigue sin cambios.";
		return this.RedirectToPage();
	}

	public async Task<IActionResult> OnPostSendVerificationEmailAsync() {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user == null) {
			return this.NotFound($"No se pudo cargar el usuario con el ID  '{this._userManager.GetUserId(this.User)}'.");
		}

		if (!this.ModelState.IsValid) {
			await this.LoadAsync(user);
			return this.Page();
		}

		var userId = await this._userManager.GetUserIdAsync(user);
		var email = await this._userManager.GetEmailAsync(user);
		var code = await this._userManager.GenerateEmailConfirmationTokenAsync(user);
		code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
		var callbackUrl = this.Url.Page(
			"/Account/ConfirmEmail",
			pageHandler: null,
			values: new { area = "Identity", userId, code },
			protocol: this.Request.Scheme);
		await this._emailSender.SendEmailAsync(
			email,
			"Confirma tu correo electrónico",
			$"Por favor confirma tu correo electrónico <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>haciendo click aquí</a>.");

		this.StatusMessage = "Se ha enviado el correo electrónico de verificación. Por favor revisa tu bandeja de entrada.";
		return this.RedirectToPage();
	}
}
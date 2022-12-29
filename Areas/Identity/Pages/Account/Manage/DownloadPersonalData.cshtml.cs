// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.Text.Json;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Areas.Identity.Pages.Account.Manage;

public class DownloadPersonalDataModel : PageModel {
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly ILogger<DownloadPersonalDataModel> _logger;

	public DownloadPersonalDataModel(
		UserManager<ApplicationUser> userManager,
		ILogger<DownloadPersonalDataModel> logger) {
		this._userManager = userManager;
		this._logger = logger;
	}

	public IActionResult OnGet() => this.NotFound();

	public async Task<IActionResult> OnPostAsync() {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user == null) {
			return this.NotFound($"No se pudo cargar el usuario con el ID '{this._userManager.GetUserId(this.User)}'.");
		}

		this._logger.LogInformation("El usuario con el ID '{UserId}' pidió sus datos personales.", this._userManager.GetUserId(this.User));

		// Only include personal data for download
		var personalData = new Dictionary<string, string>();
		var personalDataProps = typeof(ApplicationUser).GetProperties().Where(
						prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
		foreach (var p in personalDataProps) {
			personalData.Add(p.Name, p.GetValue(user)?.ToString() ?? "null");
		}

		var logins = await this._userManager.GetLoginsAsync(user);
		foreach (var l in logins) {
			personalData.Add($"{l.LoginProvider} llave de proveedor de inicio de sesión externo", l.ProviderKey);
		}

		personalData.Add($"Llave de autenticador de dos pasos", await this._userManager.GetAuthenticatorKeyAsync(user));

		this.Response.Headers.Add("Content-Disposition", "attachment; filename=PersonalData.json");
		return new FileContentResult(JsonSerializer.SerializeToUtf8Bytes(personalData), "application/json");
	}
}
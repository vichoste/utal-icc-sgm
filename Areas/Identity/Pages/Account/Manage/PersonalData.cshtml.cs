// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Areas.Identity.Pages.Account.Manage;

public class PersonalDataModel : PageModel {
	private readonly UserManager<ApplicationUser> _userManager;

	public PersonalDataModel(UserManager<ApplicationUser> userManager) => this._userManager = userManager;

	public async Task<IActionResult> OnGet() {
		var user = await this._userManager.GetUserAsync(this.User);
		return user == null ? this.NotFound($"No se pudo cargar el usuario con el ID '{this._userManager.GetUserId(this.User)}'.") : this.Page();
	}
}
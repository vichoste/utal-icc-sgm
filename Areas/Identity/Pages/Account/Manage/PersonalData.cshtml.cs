// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Utal.Icc.Sgm.Areas.Identity.Pages.Account.Manage;

public class PersonalDataModel : PageModel {
	private readonly UserManager<IdentityUser> _userManager;
	private readonly ILogger<PersonalDataModel> _logger;

	public PersonalDataModel(
		UserManager<IdentityUser> userManager,
		ILogger<PersonalDataModel> logger) {
		_userManager = userManager;
		_logger = logger;
	}

	public async Task<IActionResult> OnGet() {
		var user = await _userManager.GetUserAsync(User);
		return user == null ? NotFound($"No se pudo cargar el usuario con el ID '{_userManager.GetUserId(User)}'.") : Page();
	}
}
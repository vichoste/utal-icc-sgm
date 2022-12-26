using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Controllers;

public class UserRolesController : Controller {
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly RoleManager<IdentityRole> _roleManager;

	public UserRolesController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) {
		_roleManager = roleManager;
		_userManager = userManager;
	}

	public async Task<IActionResult> Index() {
		var users = await _userManager.Users.ToListAsync();
		var userRolesViewModel = new List<UserRolesViewModel>();
		foreach (var user in users) {
			var thisViewModel = new UserRolesViewModel {
				UserId = user.Id,
				Email = user.Email!,
				FirstName = user.FirstName!,
				LastName = user.LastName!,
				Roles = await GetUserRoles(user)
			};
			userRolesViewModel.Add(thisViewModel);
		}
		return View(userRolesViewModel);
	}
	private async Task<List<string>> GetUserRoles(ApplicationUser user) => new List<string>(await _userManager.GetRolesAsync(user));
}

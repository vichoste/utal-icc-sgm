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

	public async Task<IActionResult> Manage(string userId) {
		ViewBag.userId = userId;
		var user = await _userManager.FindByIdAsync(userId);
		if (user == null) {
			ViewBag.ErrorMessage = $"No se encuentra el usuario con el id {userId}.";
			return View("NotFound");
		}
		ViewBag.UserName = user.UserName;
		var model = new List<ManageUserRolesViewModel>();
		foreach (var role in _roleManager.Roles.ToList()) {
			var userRolesViewModel = new ManageUserRolesViewModel {
				RoleId = role.Id,
				RoleName = role.Name,
				Selected = await _userManager.IsInRoleAsync(user, role.Name!)
			};
			model.Add(userRolesViewModel);
		}
		return View(model);
	}

	[HttpPost]
	public async Task<IActionResult> Manage(List<ManageUserRolesViewModel> model, string userId) {
		var user = await _userManager.FindByIdAsync(userId);
		if (user == null) {
			return View();
		}
		var roles = await _userManager.GetRolesAsync(user);
		var result = await _userManager.RemoveFromRolesAsync(user, roles);
		if (!result.Succeeded) {
			ModelState.AddModelError("", "No se puede(n) remover el(los) rol(es).");
			return View(model);
		}
		result = await _userManager.AddToRolesAsync(user, model.Where(x => x.Selected).Select(y => y.RoleName).ToList()!);
		if (!result.Succeeded) {
			ModelState.AddModelError("", "No se puede(n) añadir el(los) rol(es) al usuario.");
			return View(model);
		}
		return RedirectToAction("Index");
	}

	private async Task<List<string>> GetUserRoles(ApplicationUser user) => new List<string>(await _userManager.GetRolesAsync(user));
}
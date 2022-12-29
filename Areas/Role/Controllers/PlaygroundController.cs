using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Utal.Icc.Sgm.Areas.Role.ViewModels;
using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Areas.Role.Controllers;

[Area("Role")]
public class PlaygroundController : Controller {
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly RoleManager<IdentityRole> _roleManager;

	public PlaygroundController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) {
		this._roleManager = roleManager;
		this._userManager = userManager;
	}

	public async Task<IActionResult> Index() {
		var users = await this._userManager.Users.ToListAsync();
		var roles = new List<RolesPlaygroundViewModel>();
		foreach (var user in users) {
			var thisViewModel = new RolesPlaygroundViewModel {
				UserId = user.Id,
				Email = user.Email!,
				FirstName = user.FirstName!,
				LastName = user.LastName!,
				Roles = await this.GetUserRoles(user)
			};
			roles.Add(thisViewModel);
		}
		return this.View(roles);
	}

	public async Task<IActionResult> Manage(string userId) {
		this.ViewBag.userId = userId;
		var user = await this._userManager.FindByIdAsync(userId);
		if (user == null) {
			this.ViewBag.ErrorMessage = $"No se encuentra el usuario con el id {userId}.";
			return this.View("NotFound");
		}
		this.ViewBag.UserName = user.UserName;
		var model = new List<ManageRolesPlaygroundViewModel>();
		foreach (var role in this._roleManager.Roles.ToList()) {
			var roles = new ManageRolesPlaygroundViewModel {
				RoleId = role.Id,
				RoleName = role.Name,
				Selected = await this._userManager.IsInRoleAsync(user, role.Name!)
			};
			model.Add(roles);
		}
		return this.View(model);
	}

	[HttpPost]
	public async Task<IActionResult> Manage(List<ManageRolesPlaygroundViewModel> model, string userId) {
		var user = await this._userManager.FindByIdAsync(userId);
		if (user == null) {
			return this.View();
		}
		var roles = await this._userManager.GetRolesAsync(user);
		var result = await this._userManager.RemoveFromRolesAsync(user, roles);
		if (!result.Succeeded) {
			this.ModelState.AddModelError("", "No se puede(n) remover el(los) rol(es).");
			return this.View(model);
		}
		result = await this._userManager.AddToRolesAsync(user, model.Where(x => x.Selected).Select(y => y.RoleName).ToList()!);
		if (!result.Succeeded) {
			this.ModelState.AddModelError("", "No se puede(n) añadir el(los) rol(es) al usuario.");
			return this.View(model);
		}
		return this.RedirectToAction("Index");
	}

	private async Task<List<string>> GetUserRoles(ApplicationUser user) => new List<string>(await this._userManager.GetRolesAsync(user));
}
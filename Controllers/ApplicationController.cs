using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;

public abstract class ApplicationController : Controller {
	protected readonly ApplicationDbContext _dbContext;
	protected readonly UserManager<ApplicationUser> _userManager;
	protected readonly IUserStore<ApplicationUser> _userStore;
	protected readonly IUserEmailStore<ApplicationUser> _emailStore;

	public ApplicationController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore, IUserEmailStore<ApplicationUser> emailStore) {
		this._dbContext = dbContext;
		this._userManager = userManager;
		this._userStore = userStore;
		this._emailStore = (IUserEmailStore<ApplicationUser>)this._userStore;
	}

	protected async Task<ApplicationUser> CheckSession() {
		var applicationUser = await this._userManager.GetUserAsync(this.User);
		return applicationUser is null || applicationUser.IsDeactivated ? null! : applicationUser;
	}

	protected async Task<ApplicationUser> CheckApplicationUser(string applicationUserId) {
		var applicationUser = await this._userManager.FindByIdAsync(applicationUserId);
		return applicationUser is null || applicationUser.IsDeactivated ? null! : applicationUser;
	}

	protected void SetSortParameters(string sortOrder, params string[] parameters) {
		foreach (var parameter in parameters) {
			this.ViewData[$"{parameter}SortParam"] = sortOrder == parameter ? $"{parameter}Desc" : parameter;
		}
		this.ViewData["CurrentSort"] = sortOrder;
	}
}
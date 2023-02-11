using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;
using Utal.Icc.Sgm.ViewModels;

namespace Utal.Icc.Sgm.Controllers;

public abstract class ApplicationController : Controller {
	protected readonly ApplicationDbContext _dbContext;
	protected readonly UserManager<ApplicationUser> _userManager;
	protected readonly IUserStore<ApplicationUser> _userStore;
	protected readonly IUserEmailStore<ApplicationUser> _emailStore;
	protected readonly SignInManager<ApplicationUser> _signInManager;

	public ApplicationController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore, SignInManager<ApplicationUser> signInManager) {
		this._dbContext = dbContext;
		this._userManager = userManager;
		this._userStore = userStore;
		this._emailStore = (IUserEmailStore<ApplicationUser>)this._userStore;
		this._signInManager = signInManager;
	}

	private void SetSortParameters(string sortOrder, params string[] parameters) {
		foreach (var parameter in parameters) {
			this.ViewData[$"{parameter}SortParam"] = sortOrder == parameter ? $"{parameter}Desc" : parameter;
		}
		this.ViewData["CurrentSort"] = sortOrder;
	}

	private IEnumerable<T> Filter<T>(string searchString, IEnumerable<T> viewModels, params string[] parameters) where T : ApplicationViewModel {
		var result = new List<T>();
		foreach (var parameter in parameters) {
			var partials = viewModels
					.Where(vm => !(vm.GetType().GetProperty(parameter)!.GetValue(vm, null) as string)!.IsNullOrEmpty() && (vm.GetType().GetProperty(parameter)!.GetValue(vm, null) as string)!.Contains(searchString));
			foreach (var partial in partials) {
				if (!result.Any(vm => vm.Id == partial.Id)) {
					result.Add(partial);
				}
			}
		}
		return result.AsEnumerable();
	}

	private IEnumerable<T> Sort<T>(string sortOrder, IEnumerable<T> viewModels, params string[] parameters) where T : ApplicationViewModel {
		foreach (var parameter in parameters) {
			if (parameter == sortOrder) {
				return viewModels.OrderBy(vm => vm.GetType().GetProperty(parameter)!.GetValue(vm, null));
			} else if ($"{parameter}Desc" == sortOrder) {
				return viewModels.OrderByDescending(vm => vm.GetType().GetProperty(parameter)!.GetValue(vm, null));
			}
		}
		return viewModels.OrderBy(vm => vm.GetType().GetProperty(parameters[0])).AsEnumerable();
	}

	protected async Task<ApplicationUser> CheckSession() {
		var applicationUser = await this._userManager.GetUserAsync(this.User);
		return applicationUser is null || applicationUser.IsDeactivated ? null! : applicationUser;
	}

	protected async Task<ApplicationUser> CheckApplicationUser(string applicationUserId) {
		var applicationUser = await this._userManager.FindByIdAsync(applicationUserId);
		return applicationUser is null || applicationUser.IsDeactivated ? null! : applicationUser;
	}

	protected async Task<IQueryable<ApplicationViewModel>> GetPaginatedViewModelsAsync<T>(string sortOrder, string currentFilter, string searchString, int? pageNumber, string[] parameters, Func<Task<IEnumerable<T>>> getViewModels) where T : ApplicationViewModel {
		this.SetSortParameters(sortOrder, parameters);
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		var users = await getViewModels();
		var ordered = this.Sort(sortOrder, users, parameters);
		var output = !searchString.IsNullOrEmpty() ? this.Filter(searchString, ordered, parameters) : ordered;
		return output.AsQueryable();
	}

	protected IQueryable<ApplicationViewModel> GetPaginatedViewModels<T>(string sortOrder, string currentFilter, string searchString, int? pageNumber, string[] parameters, Func<IEnumerable<T>> getViewModels) where T : ApplicationViewModel {
		this.SetSortParameters(sortOrder, parameters);
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		var users = getViewModels();
		var ordered = this.Sort(sortOrder, users, parameters);
		var output = !searchString.IsNullOrEmpty() ? this.Filter(searchString, ordered, parameters) : ordered;
		return output.AsQueryable();
	}
}
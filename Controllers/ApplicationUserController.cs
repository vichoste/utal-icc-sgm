using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Utal.Icc.Sgm.Areas.DirectorTeacher.ViewModels.Teacher;
using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;
using Utal.Icc.Sgm.ViewModels;

using static Utal.Icc.Sgm.Models.ApplicationUser;

namespace Utal.Icc.Sgm.Controllers;

public abstract class ApplicationUserController : ApplicationController, ISortable {
	public abstract string[]? Parameters { get; set; }

	public ApplicationUserController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore, SignInManager<ApplicationUser> signInManager) : base(dbContext, userManager, userStore, signInManager) { }

	public abstract void SetSortParameters(string sortOrder, params string[] parameters);

	protected T Create<T>() where T : ApplicationUserViewModel, new() => new T();

	protected async Task<ApplicationUser?> CreateAsync<T>([FromForm] T input, IEnumerable<string> roles) where T : ApplicationUserViewModel {
		var user = new ApplicationUser {
			FirstName = input.FirstName,
			LastName = input.LastName,
			Rut = input.Rut,
			CreatedAt = DateTimeOffset.Now,
			UpdatedAt = DateTimeOffset.Now
		};
		await this._userStore.SetUserNameAsync(user, input.Email, CancellationToken.None);
		await this._emailStore.SetEmailAsync(user, input.Email, CancellationToken.None);
		_ = await this._userManager.CreateAsync(user, input.Password!);
		_ = await this._userManager.AddToRoleAsync(user, nameof(Roles.Teacher));
		_ = await this._userManager.AddToRolesAsync(user, roles);
		return user;
	}

	protected async Task<T?> EditAsync<T>(string id) where T : ApplicationUserViewModel, new() {
		if (await this.CheckApplicationUser(id) is not ApplicationUser user) {
			return null;
		}
		var output = new T {
			Id = id,
			FirstName = user.FirstName,
			LastName = user.LastName,
			Rut = user.Rut,
			Email = user.Email,
			CreatedAt = user.CreatedAt,
			UpdatedAt = user.UpdatedAt
		};
		return output;
	}

	protected async Task<T?> EditAsync<T>([FromForm] T input) where T : ApplicationUserViewModel, new() {
		if (await this.CheckApplicationUser(input.Id!) is not ApplicationUser user) {
			return null;
		}
		await this._userStore.SetUserNameAsync(user, input.Email, CancellationToken.None);
		await this._emailStore.SetEmailAsync(user, input.Email, CancellationToken.None);
		user.FirstName = input.FirstName;
		user.LastName = input.LastName;
		user.Rut = input.Rut;
		user.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(user);
		if (input is EditTeacherViewModel teacher) {
			var roles = (await this._userManager.GetRolesAsync(user)).ToList();
			if (roles.Contains(nameof(Roles.Teacher))) {
				_ = roles.Remove(nameof(Roles.Teacher));
			}
			if (roles.Contains(nameof(Roles.DirectorTeacher))) {
				_ = roles.Remove(nameof(Roles.DirectorTeacher));
			}
			var removeRankRolesResult = await this._userManager.RemoveFromRolesAsync(user, roles);
			var rankRoles = new List<string>();
			if (teacher.IsGuideTeacher) {
				rankRoles.Add(nameof(Roles.GuideTeacher));
			}
			if (teacher.IsAssistantTeacher) {
				rankRoles.Add(nameof(Roles.AssistantTeacher));
			}
			if (teacher.IsCourseTeacher) {
				rankRoles.Add(nameof(Roles.CourseTeacher));
			}
			if (teacher.IsCommitteeTeacher) {
				rankRoles.Add(nameof(Roles.CommitteeTeacher));
			}
			_ = await this._userManager.AddToRolesAsync(user, rankRoles);
		}
		var output = new T {
			Id = user.Id,
			FirstName = user.FirstName,
			LastName = user.LastName,
			Rut = user.Rut,
			Email = user.Email,
			CreatedAt = user.CreatedAt,
			UpdatedAt = user.UpdatedAt
		};
		if (output is EditTeacherViewModel teacher1) {
			teacher1.IsAssistantTeacher = await this._userManager.IsInRoleAsync(user, nameof(Roles.AssistantTeacher));
			teacher1.IsCommitteeTeacher = await this._userManager.IsInRoleAsync(user, nameof(Roles.CommitteeTeacher));
			teacher1.IsCourseTeacher = await this._userManager.IsInRoleAsync(user, nameof(Roles.CourseTeacher));
			teacher1.IsGuideTeacher = await this._userManager.IsInRoleAsync(user, nameof(Roles.GuideTeacher));
		}
		return output;
	}

	protected async Task<T?> ToggleActivationAsync<T>(string id) where T : ApplicationUserViewModel, new() {
		var user = await this._userManager.FindByIdAsync(id);
		if (user is null || user.Id == this._userManager.GetUserId(this.User) || (await this._userManager.GetRolesAsync(user)).Contains(nameof(Roles.DirectorTeacher))) {
			return null;
		}
		var output = new T {
			Id = user!.Id,
			Email = user.Email,
			IsDeactivated = user.IsDeactivated
		};
		return output;
	}

	protected async Task<ApplicationUser?> ToggleActivationAsync<T>([FromForm] ApplicationUserViewModel input) where T : ApplicationUserViewModel, new() {
		var user = await this._userManager.FindByIdAsync(input.Id!);
		if (user is null || user.Id == this._userManager.GetUserId(this.User) || (await this._userManager.GetRolesAsync(user)).Contains(nameof(Roles.DirectorTeacher))) {
			return null;
		}
		user!.IsDeactivated = !user.IsDeactivated;
		user.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(user);
		return user;
	}
}
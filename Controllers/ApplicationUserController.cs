using Microsoft.AspNetCore.Identity;

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

	protected async Task<ApplicationUser?> CreateAsync<T>(T input, IEnumerable<string> roles) where T : ApplicationUserViewModel {
		var user = new ApplicationUser {
			FirstName = input.FirstName,
			LastName = input.LastName,
			Rut = input.Rut,
			CreatedAt = DateTimeOffset.Now,
			UpdatedAt = DateTimeOffset.Now
		};
		await base._userStore.SetUserNameAsync(user, input.Email, CancellationToken.None);
		await base._emailStore.SetEmailAsync(user, input.Email, CancellationToken.None);
		_ = await base._userManager.CreateAsync(user, input.Password!);
		_ = await base._userManager.AddToRoleAsync(user, nameof(Roles.Teacher));
		_ = await base._userManager.AddToRolesAsync(user, roles);
		return user;
	}

	protected async Task<T?> EditAsync<T>(string id) where T : ApplicationUserViewModel, new() {
		if (await base.CheckApplicationUser(id) is not ApplicationUser user) {
			return null;
		}
		var output = user switch {
			_ when await base._userManager.IsInRoleAsync(user, nameof(Roles.Student)) => new T {
				Id = user.Id,
				FirstName = user.FirstName,
				LastName = user.LastName,
				Rut = user.Rut,
				Email = user.Email,
				CreatedAt = user.CreatedAt,
				UpdatedAt = user.UpdatedAt,
				StudentUniversityId = user.StudentUniversityId
			},
			_ when await base._userManager.IsInRoleAsync(user, nameof(Roles.Teacher)) => new T {
				Id = user.Id,
				FirstName = user.FirstName,
				LastName = user.LastName,
				Rut = user.Rut,
				Email = user.Email,
				CreatedAt = user.CreatedAt,
				UpdatedAt = user.UpdatedAt,
			},
			_ => null
		};
		if (output is EditTeacherViewModel teacher) {
			teacher.IsGuideTeacher = await base._userManager.IsInRoleAsync(user, nameof(Roles.GuideTeacher));
			teacher.IsAssistantTeacher = await base._userManager.IsInRoleAsync(user, nameof(Roles.AssistantTeacher));
			teacher.IsCourseTeacher = await base._userManager.IsInRoleAsync(user, nameof(Roles.CourseTeacher));
			teacher.IsCommitteeTeacher = await base._userManager.IsInRoleAsync(user, nameof(Roles.CommitteeTeacher));
		}
		return output;
	}

	protected async Task<T?> EditAsync<T>(T input) where T : ApplicationUserViewModel, new() {
		if (await base.CheckApplicationUser(input.Id!) is not ApplicationUser user) {
			return null;
		}
		await base._userStore.SetUserNameAsync(user, input.Email, CancellationToken.None);
		await base._emailStore.SetEmailAsync(user, input.Email, CancellationToken.None);
		user.FirstName = input.FirstName;
		user.LastName = input.LastName;
		user.Rut = input.Rut;
		user.UpdatedAt = DateTimeOffset.Now;
		if (await base._userManager.IsInRoleAsync(user, nameof(Roles.Student))) {
			user.StudentUniversityId = input.StudentUniversityId;
		}
		_ = await base._userManager.UpdateAsync(user);
		if (input is EditTeacherViewModel teacher) {
			var roles = (await base._userManager.GetRolesAsync(user)).ToList();
			if (roles.Contains(nameof(Roles.Teacher))) {
				_ = roles.Remove(nameof(Roles.Teacher));
			}
			if (roles.Contains(nameof(Roles.DirectorTeacher))) {
				_ = roles.Remove(nameof(Roles.DirectorTeacher));
			}
			var removeRankRolesResult = await base._userManager.RemoveFromRolesAsync(user, roles);
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
			_ = await base._userManager.AddToRolesAsync(user, rankRoles);
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
			teacher1.IsAssistantTeacher = await base._userManager.IsInRoleAsync(user, nameof(Roles.AssistantTeacher));
			teacher1.IsCommitteeTeacher = await base._userManager.IsInRoleAsync(user, nameof(Roles.CommitteeTeacher));
			teacher1.IsCourseTeacher = await base._userManager.IsInRoleAsync(user, nameof(Roles.CourseTeacher));
			teacher1.IsGuideTeacher = await base._userManager.IsInRoleAsync(user, nameof(Roles.GuideTeacher));
		}
		return output;
	}

	protected async Task<T?> ToggleAsync<T>(string id) where T : ApplicationUserViewModel, new() {
		var user = await base._userManager.FindByIdAsync(id);
		if (user is null || user.Id == base._userManager.GetUserId(base.User) || (await base._userManager.GetRolesAsync(user)).Contains(nameof(Roles.DirectorTeacher))) {
			return null;
		}
		var output = new T {
			Id = user!.Id,
			Email = user.Email,
			IsDeactivated = user.IsDeactivated
		};
		return output;
	}

	protected async Task<ApplicationUser?> ToggleAsync<T>(T input) where T : ApplicationUserViewModel, new() {
		var user = await base._userManager.FindByIdAsync(input.Id!);
		if (user is null || user.Id == base._userManager.GetUserId(base.User) || (await base._userManager.GetRolesAsync(user)).Contains(nameof(Roles.DirectorTeacher))) {
			return null;
		}
		user!.IsDeactivated = !user.IsDeactivated;
		user.UpdatedAt = DateTimeOffset.Now;
		_ = await base._userManager.UpdateAsync(user);
		return user;
	}
}
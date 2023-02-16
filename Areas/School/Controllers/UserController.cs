using System.Globalization;

using CsvHelper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Utal.Icc.Sgm.Controllers;
using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;
using Utal.Icc.Sgm.ViewModels;
using Utal.Icc.Sgm.Areas.School.Helpers;
using Utal.Icc.Sgm.Areas.School.ViewModels.UserController;
using Microsoft.EntityFrameworkCore;

namespace Utal.Icc.Sgm.Areas.School.Controllers;

[Area(nameof(School)), Authorize(Roles = nameof(Role.DirectorTeacher))]
public class UserController : Controller, ISortable {
	private readonly ApplicationDbContext _dbContext;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly IUserStore<ApplicationUser> _userStore;
	private readonly IUserEmailStore<ApplicationUser> _emailStore;
	private readonly SignInManager<ApplicationUser> _signInManager;

	public string[]? Parameters { get; set; }

	public UserController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore, SignInManager<ApplicationUser> signInManager) {
		this._dbContext = dbContext;
		this._userManager = userManager;
		this._userStore = userStore;
		this._emailStore = (IUserEmailStore<ApplicationUser>)this._userStore;
		this._signInManager = signInManager;
	}

	public void SetSortParameters(string sortOrder, params string[] parameters) {
		foreach (var parameter in parameters) {
			this.ViewData[$"{parameter}SortParam"] = sortOrder == parameter ? $"{parameter}Desc" : parameter;
		}
		this.ViewData["CurrentSort"] = sortOrder;
	}

	public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		var parameters = new[] { nameof(ApplicationUserViewModel.FirstName), nameof(ApplicationUserViewModel.LastName), nameof(ApplicationUserViewModel.StudentUniversityId), nameof(ApplicationUserViewModel.Rut), nameof(ApplicationUserViewModel.Email) };
		this.SetSortParameters(sortOrder, parameters);
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		var users = await this._userManager.Users.ToListAsync();
		var paginator = Paginator<ApplicationViewModel>.Create(users.Select(async u => new ApplicationUserViewModel {
			Id = u.Id,
			FirstName = u.FirstName,
			LastName = u.LastName,
			StudentUniversityId = u.StudentUniversityId,
			Rut = u.Rut,
			Email = u.Email,
			IsDeactivated = u.IsDeactivated,
			IsDirectorTeacher = await this._userManager.IsInRoleAsync(u, nameof(Role.DirectorTeacher)),
		}).Select(u => u.Result).AsQueryable(), pageNumber ?? 1, 10);
		if (!string.IsNullOrEmpty(sortOrder)) {
			paginator.Sort(sortOrder);
		}
		if (!string.IsNullOrEmpty(currentFilter)) {
			paginator.Filter(currentFilter);
		}
		return this.View(paginator);
	}

	public IActionResult StudentsBatchCreate() => this.View(new CsvFileViewModel());

	[HttpPost]
	public async Task<IActionResult> StudentsBatchCreate([FromForm] CsvFileViewModel input) {
		try {
			var errorMessages = new List<string>(); ;
			var successMessages = new List<string>();
			using var reader = new StreamReader(input.CsvFile!.OpenReadStream());
			using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
			var records = csv.GetRecords<CsvFileHelper>();
			foreach (var record in records) {
				var user = new ApplicationUser {
					FirstName = record.FirstName,
					LastName = record.LastName,
					StudentUniversityId = record.UniversityId,
					Rut = record.Rut,
					CreatedAt = DateTimeOffset.Now,
					UpdatedAt = DateTimeOffset.Now
				};
				await this._userStore.SetUserNameAsync(user, record.Email, CancellationToken.None);
				await this._emailStore.SetEmailAsync(user, record.Email, CancellationToken.None);
				var result = await this._userManager.CreateAsync(user, record!.Password!);
				if (!result.Succeeded) {
					errorMessages.Add($"Estudiante {record.Email} ya existe");
					continue;
				}
				_ = await this._userManager.AddToRoleAsync(user, nameof(Role.Student));
				successMessages.Add($"Estudiante {record.Email} creado correctamente.");
			}
			if (errorMessages.Any()) {
				this.TempData["ErrorMessages"] = errorMessages.AsEnumerable();
			}
			if (successMessages.Any()) {
				this.TempData["SuccessMessages"] = successMessages.AsEnumerable();
			}
			return this.RedirectToAction(nameof(UserController.Index), nameof(UserController).Replace("Controller", string.Empty), new { area = nameof(School) });
		} catch {
			this.TempData["ErrorMessage"] = "Error al importar el archivo CSV.";
			return this.RedirectToAction(nameof(UserController.Index), nameof(UserController).Replace("Controller", string.Empty), new { area = nameof(School) });
		}
	}

	public IActionResult CreateTeacher() => this.View(new ApplicationUserViewModel());

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> CreateTeacher([FromForm] ApplicationUserViewModel input) {
		var roles = new List<string>();
		if (input.IsGuideTeacher) {
			roles.Add(nameof(Role.GuideTeacher));
		}
		if (input.IsAssistantTeacher) {
			roles.Add(nameof(Role.AssistantTeacher));
		}
		if (input.IsCourseTeacher) {
			roles.Add(nameof(Role.CourseTeacher));
		}
		if (input.IsCommitteeTeacher) {
			roles.Add(nameof(Role.CommitteeTeacher));
		}
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
		_ = await this._userManager.AddToRoleAsync(user, nameof(Role.Teacher));
		_ = await this._userManager.AddToRolesAsync(user, roles);
		this.TempData["SuccessMessage"] = "Profesor creado exitosamente.";
		return this.RedirectToAction(nameof(UserController.Index), nameof(UserController).Replace("Controller", string.Empty), new { area = nameof(School) });
	}

	public async Task<IActionResult> Edit(string id) {
		var user = await this._userManager.FindByIdAsync(id);
		if (user is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al usuario.";
			return this.RedirectToAction(nameof(UserController.Index), nameof(UserController).Replace("Controller", string.Empty), new { area = nameof(School) });
		}
		var output = user switch {
			_ when await this._userManager.IsInRoleAsync(user, nameof(Role.Student)) => new ApplicationUserViewModel {
				Id = user.Id,
				FirstName = user.FirstName,
				LastName = user.LastName,
				Rut = user.Rut,
				Email = user.Email,
				CreatedAt = user.CreatedAt,
				UpdatedAt = user.UpdatedAt,
				StudentUniversityId = user.StudentUniversityId
			},
			_ when await this._userManager.IsInRoleAsync(user, nameof(Role.Teacher)) => new ApplicationUserViewModel {
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
		if (output is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al usuario.";
			if (await this._userManager.IsInRoleAsync(user, nameof(Role.Student))) {
				return this.RedirectToAction(nameof(UserController.Index), nameof(UserController).Replace("Controller", string.Empty), new { area = nameof(School) });
			}
			if (await this._userManager.IsInRoleAsync(user, nameof(Role.Teacher))) {
				return this.RedirectToAction(nameof(UserController.Index), nameof(UserController).Replace("Controller", string.Empty), new { area = nameof(School) });
			}
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		return this.View(output);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit([FromForm] ApplicationUserViewModel input) {
		var user = await this._userManager.FindByIdAsync(input.Id!);
		if (user is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al usuario.";
			return this.RedirectToAction(nameof(UserController.Index), nameof(UserController).Replace("Controller", string.Empty), new { area = nameof(School) });
		}
		await this._userStore.SetUserNameAsync(user, input.Email, CancellationToken.None);
		await this._emailStore.SetEmailAsync(user, input.Email, CancellationToken.None);
		user.FirstName = input.FirstName;
		user.LastName = input.LastName;
		user.Rut = input.Rut;
		user.UpdatedAt = DateTimeOffset.Now;
		if (await this._userManager.IsInRoleAsync(user, nameof(Role.Student))) {
			user.StudentUniversityId = input.StudentUniversityId;
		}
		_ = await this._userManager.UpdateAsync(user);
		if (await this._userManager.IsInRoleAsync(user, nameof(Role.Teacher))) {
			var roles = (await this._userManager.GetRolesAsync(user)).ToList();
			if (roles.Contains(nameof(Role.Teacher))) {
				_ = roles.Remove(nameof(Role.Teacher));
			}
			if (roles.Contains(nameof(Role.DirectorTeacher))) {
				_ = roles.Remove(nameof(Role.DirectorTeacher));
			}
			var removeRankRolesResult = await this._userManager.RemoveFromRolesAsync(user, roles);
			var rankRoles = new List<string>();
			if (input.IsGuideTeacher) {
				rankRoles.Add(nameof(Role.GuideTeacher));
			}
			if (input.IsAssistantTeacher) {
				rankRoles.Add(nameof(Role.AssistantTeacher));
			}
			if (input.IsCourseTeacher) {
				rankRoles.Add(nameof(Role.CourseTeacher));
			}
			if (input.IsCommitteeTeacher) {
				rankRoles.Add(nameof(Role.CommitteeTeacher));
			}
			_ = await this._userManager.AddToRolesAsync(user, rankRoles);
		}
		var output = new ApplicationUserViewModel {
			Id = user.Id,
			FirstName = user.FirstName,
			LastName = user.LastName,
			Rut = user.Rut,
			Email = user.Email,
			CreatedAt = user.CreatedAt,
			UpdatedAt = user.UpdatedAt
		};
		if (await this._userManager.IsInRoleAsync(user, nameof(Role.Teacher))) {
			output.IsAssistantTeacher = await this._userManager.IsInRoleAsync(user, nameof(Role.AssistantTeacher));
			output.IsCommitteeTeacher = await this._userManager.IsInRoleAsync(user, nameof(Role.CommitteeTeacher));
			output.IsCourseTeacher = await this._userManager.IsInRoleAsync(user, nameof(Role.CourseTeacher));
			output.IsGuideTeacher = await this._userManager.IsInRoleAsync(user, nameof(Role.GuideTeacher));
		}
		this.TempData["SuccessMessage"] = "Usuario editado correctamente.";
		return this.View(output);
	}

	public async Task<IActionResult> Toggle(string id) {
		var user = await this._userManager.FindByIdAsync(id);
		if (user is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al usuario.";
			return this.RedirectToAction(nameof(UserController.Index), nameof(UserController).Replace("Controller", string.Empty), new { area = nameof(School) });
		}
		if (user.Id == this._userManager.GetUserId(this.User) || (await this._userManager.GetRolesAsync(user)).Contains(nameof(Role.DirectorTeacher))) {
			this.TempData["ErrorMessage"] = "Error al cambiar el estado de la activación al usuario.";
			if (await this._userManager.IsInRoleAsync(user, nameof(Role.Student))) {
				return this.RedirectToAction(nameof(UserController.Index), nameof(UserController).Replace("Controller", string.Empty), new { area = nameof(School) });
			}
			return this.RedirectToAction(nameof(UserController.Index), nameof(UserController).Replace("Controller", string.Empty), new { area = nameof(School) });
		}
		var output = new ApplicationUserViewModel {
			Id = user.Id,
			Email = user.Email,
			IsDeactivated = user.IsDeactivated
		};
		return this.View(output);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Toggle([FromForm] ApplicationUserViewModel input) {
		var user = await this._userManager.FindByIdAsync(input.Id!);
		if (user is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al usuario.";
			return this.RedirectToAction(nameof(UserController.Index), nameof(UserController).Replace("Controller", string.Empty), new { area = nameof(School) });
		}
		if (user.Id == this._userManager.GetUserId(this.User) || (await this._userManager.GetRolesAsync(user)).Contains(nameof(Role.DirectorTeacher))) {
			this.TempData["ErrorMessage"] = "Error al cambiar el estado de la activación al usuario.";
			if (await this._userManager.IsInRoleAsync(user, nameof(Role.Student))) {
				return this.RedirectToAction(nameof(UserController.Index), nameof(UserController).Replace("Controller", string.Empty), new { area = nameof(School) });
			}
			if (await this._userManager.IsInRoleAsync(user, nameof(Role.Teacher))) {
				return this.RedirectToAction(nameof(UserController.Index), nameof(UserController).Replace("Controller", string.Empty), new { area = nameof(School) });
			}
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		user.IsDeactivated = !user.IsDeactivated;
		user.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(user);
		this.TempData["SuccessMessage"] = user.IsDeactivated ? "Usuario desactivado correctamente." : "Usuario activado correctamente.";
		return this.RedirectToAction(nameof(UserController.Index), nameof(UserController).Replace("Controller", string.Empty), new { area = nameof(School) });
	}

	public async Task<IActionResult> Transfer(string currentId, string @newId) {
		var current = await this._userManager.FindByIdAsync(currentId);
		var @new = await this._userManager.FindByIdAsync(@newId);
		var check = (current, @new) switch {
			(ApplicationUser, ApplicationUser) => true,
			(ApplicationUser teacher, _) when teacher.IsDeactivated || (await this._userManager.IsInRoleAsync(teacher, nameof(Role.Teacher)) && await this._userManager.IsInRoleAsync(teacher, nameof(Role.DirectorTeacher))) => false,
			(_, ApplicationUser teacher) when teacher.IsDeactivated || await this._userManager.IsInRoleAsync(teacher, nameof(Role.Teacher)) => false,
			_ => false
		};
		check = check && (current!.Id != @new!.Id);
		if (!check) {
			this.TempData["ErrorMessage"] = "Revisa los profesores fuente y objetivo antes de hacer la transferencia.";
			return this.RedirectToAction(nameof(UserController.Index), nameof(UserController).Replace("Controller", string.Empty), new { area = nameof(School) });
		}
		var transferViewModel = new TransferViewModel {
			CurrentDirectorTeacherId = current!.Id,
			NewDirectorTeacherId = @new!.Id,
			NewDirectorTeacherName = $"{@new.FirstName} {@new.LastName}"
		};
		return this.View(transferViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Transfer([FromForm] TransferViewModel model) {
		var current = await this._userManager.FindByIdAsync(model.CurrentDirectorTeacherId!);
		var @new = await this._userManager.FindByIdAsync(model.NewDirectorTeacherId!);
		var check = (current, @new) switch {
			(ApplicationUser, ApplicationUser) => true,
			(ApplicationUser teacher, _) when teacher.IsDeactivated || (await this._userManager.IsInRoleAsync(teacher, nameof(Role.Teacher)) && await this._userManager.IsInRoleAsync(teacher, nameof(Role.DirectorTeacher))) => false,
			(_, ApplicationUser teacher) when teacher.IsDeactivated || await this._userManager.IsInRoleAsync(teacher, nameof(Role.Teacher)) => false,
			_ => false
		};
		check = check && (current!.Id != @new!.Id);
		if (!check) {
			this.TempData["ErrorMessage"] = "Revisa los profesores fuente y objetivo antes de hacer la transferencia.";
			return this.RedirectToAction(nameof(UserController.Index), nameof(UserController).Replace("Controller", string.Empty), new { area = nameof(School) });
		}
		_ = await this._userManager.RemoveFromRoleAsync(current!, nameof(Role.DirectorTeacher));
		_ = await this._userManager.AddToRoleAsync(@new!, nameof(Role.DirectorTeacher));
		current!.UpdatedAt = DateTimeOffset.Now;
		@new!.UpdatedAt = DateTimeOffset.Now;
		_ = await this._userManager.UpdateAsync(current);
		this.TempData["SuccessMessage"] = "Director de carrera transferido correctamente.";
		return this.RedirectToAction(nameof(UserController.Index), nameof(UserController).Replace("Controller", string.Empty), new { area = nameof(School) });
	}
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Utal.Icc.Sgm.Areas.DirectorTeacher.Views.Student;
using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Areas.DirectorTeacher.Controllers;

[Area("DirectorTeacher"), Authorize(Roles = "DirectorTeacher")]
public class StudentController : Controller {
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly IUserStore<ApplicationUser> _userStore;
	private readonly IUserEmailStore<ApplicationUser> _emailStore;
	private readonly RoleManager<IdentityRole> _roleManager;
	private readonly ApplicationDbContext _dbContext;

	public StudentController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore, RoleManager<IdentityRole> roleManager, ApplicationDbContext dbContext) {
		this._signInManager = signInManager;
		this._userManager = userManager;
		this._userStore = userStore;
		this._emailStore = (IUserEmailStore<ApplicationUser>)this._userStore;
		this._roleManager = roleManager;
		this._dbContext = dbContext;
	}

	public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		this.ViewData["FirstNameSortParam"] = sortOrder == "FirstName" ? "FirstNameDesc" : "FirstName";
		this.ViewData["LastNameSortParam"] = sortOrder == "LastName" ? "LastNameDesc" : "LastName";
		this.ViewData["UniversityIdSortParam"] = sortOrder == "UniversityId" ? "UniversityIdDesc" : "UniversityId";
		this.ViewData["RutSortParam"] = sortOrder == "Rut" ? "RutDesc" : "Rut";
		this.ViewData["EmailSortParam"] = sortOrder == "Email" ? "EmailDesc" : "Email";
		this.ViewData["CurrentSort"] = sortOrder;
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		var students = new List<ApplicationUser>();
		foreach (var applicationUser in this._userManager.Users.Include(a => a.StudentProfile).ToList()) {
			if (await this._userManager.IsInRoleAsync(applicationUser, Roles.Student.ToString())) {
				students.Add(applicationUser);
			}
		}
		students = sortOrder switch {
			"FirstName" => students.OrderBy(a => a.FirstName).ToList(),
			"FirstNameDesc" => students.OrderByDescending(a => a.FirstName).ToList(),
			"UniversityId" => students.OrderBy(a => a.StudentProfile!.UniversityId).ToList(),
			"Rut" => students.OrderBy(a => a.Rut).ToList(),
			"RutDesc" => students.OrderByDescending(a => a.Rut).ToList(),
			"Email" => students.OrderBy(a => a.Email).ToList(),
			"EmailDesc" => students.OrderByDescending(a => a.Email).ToList(),
			"LastName" => students.OrderBy(a => a.LastName).ToList(),
			"LastNameDesc" => students.OrderByDescending(a => a.LastName).ToList(),
			_ => students.OrderBy(a => a.LastName).ToList()
		};
		if (!string.IsNullOrEmpty(searchString)) {
			students = students.Where(s => s.FirstName!.ToUpper().Contains(searchString.ToUpper()) || s.LastName!.ToUpper().Contains(searchString.ToUpper()) || s.Rut!.ToUpper().Contains(searchString.ToUpper()) || s.Email == searchString).ToList();
		}
		var indexViewModels = students.Select(a => new IndexViewModel {
			Id = a.Id,
			FirstName = a.FirstName,
			LastName = a.LastName,
			UniversityId = a.StudentProfile!.UniversityId,
			Rut = a.Rut,
			Email = a.Email
		});
		var pageSize = 6;
		return this.View(PaginatedList<IndexViewModel>.Create(indexViewModels.AsQueryable(), pageNumber ?? 1, pageSize));
	}

	public IActionResult Edit(string id) {
		var applicationUser = this._userManager.Users.Include(a => a.StudentProfile).FirstOrDefault(a => a.Id == id);
		if (applicationUser is null) {
			this.ViewBag.ErrorMessage = "Error al obtener al estudiante.";
			return this.View();
		}
		var editViewModel = new EditViewModel {
			FirstName = applicationUser.FirstName,
			LastName = applicationUser.LastName,
			UniversityId = applicationUser!.StudentProfile!.UniversityId,
			Rut = applicationUser.Rut,
			Email = applicationUser.Email
		};
		return this.View(editViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit(string id, [FromForm] EditViewModel model) {
		var applicationUser = this._userManager.Users.Include(a => a.StudentProfile).FirstOrDefault(a => a.Id == id);
		if (applicationUser is null) {
			this.ViewBag.ErrorMessage = "Error al obtener al estudiante.";
			return this.View();
		}
		await this._userStore.SetUserNameAsync(applicationUser, userName: model.Email, CancellationToken.None);
		await this._emailStore.SetEmailAsync(applicationUser, model.Email, CancellationToken.None);
		applicationUser.FirstName = !model.FirstName.IsNullOrEmpty() ? model.FirstName : applicationUser.FirstName;
		applicationUser.LastName = !model.LastName.IsNullOrEmpty() ? model.LastName : applicationUser.LastName;
		applicationUser.Rut = !model.Rut.IsNullOrEmpty() ? model.Rut : applicationUser.Rut;
		var updateResult = await this._userManager.UpdateAsync(applicationUser);
		if (updateResult.Succeeded) {
			applicationUser.StudentProfile!.UniversityId = model.UniversityId;
			_ = await this._dbContext.SaveChangesAsync();
			this.ViewBag.SuccessMessage = "Estudiante actualizado con éxito.";
			return this.View();
		}
		if (updateResult.Errors.Any()) {
			this.ViewBag.ErrorMessages = updateResult.Errors.Select(e => e.Description).ToList();
		}
		this.ViewBag.ErrorMessage = "Error al actualizar al estudiante.";
		return this.View();
	}

	public IActionResult Delete(string id) {
		var applicationUser = this._userManager.Users.FirstOrDefault(a => a.Id == id);
		if (applicationUser is null) {
			this.ViewBag.ErrorMessage = "Error al obtener al estudiante.";
			return this.View();
		}
		var deleteViewModel = new DeleteViewModel {
			Id = applicationUser.Id,
			Email = applicationUser.Email
		};
		return this.View(deleteViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete(string id, [FromForm] DeleteViewModel model) {
		var applicationUser = this._userManager.Users.Include(a => a.StudentProfile).FirstOrDefault(a => a.Id == id);
		if (applicationUser is null) {
			this.ViewBag.ErrorMessage = "Error al obtener al estudiante.";
			return this.View();
		}
		if (applicationUser!.Id == this._userManager.GetUserId(this.User)) {
			this.ViewBag.ErrorMessage = "No te puedes eliminar a tí mismo.";
			return this.View();
		}
		_ = this._dbContext.StudentProfiles.Remove(applicationUser.StudentProfile!);
		_ = await this._dbContext.SaveChangesAsync();
		var result = await this._userManager.DeleteAsync(applicationUser);
		if (result.Succeeded) {
			this.ViewBag.SuccessMessage = "Estudiante eliminado con éxito.";
			return this.View();
		}
		this.ViewBag.ErrorMessage = "Error al eliminar al estudiante.";
		this.ViewBag.ErrorMessages = result.Errors.Select(e => e.Description).ToList();
		return this.View();
	}
}
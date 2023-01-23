using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Utal.Icc.Sgm.Areas.DirectorTeacher.Views.Student;
using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Areas.DirectorTeacher.Controllers;

[Area("DirectorTeacher"), Authorize(Roles = "DirectorTeacher")]
public class StudentController : Controller {
	private readonly UserManager<ApplicationUser> _userManager;

	public StudentController(UserManager<ApplicationUser> userManager) => this._userManager = userManager;

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
			if (await this._userManager.IsInRoleAsync(applicationUser, "Student")) {
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
}
using System.Globalization;

using CsvHelper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;
using Utal.Icc.Sgm.ViewModels;
using Utal.Icc.Sgm.Areas.University.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Utal.Icc.Sgm.Areas.University.Controllers;

[Area("University"), Authorize]
public class MemoirController : Controller {
	private readonly ApplicationDbContext _dbContext;
	private readonly UserManager<ApplicationUser> _userManager;
	public MemoirController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager) {
		this._dbContext = dbContext;
		this._userManager = userManager;
	}

	[Authorize(Roles = "Memorist")]
	public async Task<IActionResult> Guide(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		var parameters = new[] { "FirstName", "LastName", "Email", "TeacherSpecialization" };
		foreach (var parameter in parameters) {
			this.ViewData[$"{parameter}SortParam"] = sortOrder == parameter ? $"{parameter}Desc" : parameter;
		}
		this.ViewData["CurrentSort"] = sortOrder;
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		var users = await this._userManager.GetUsersInRoleAsync("Guide");
		var paginator = Paginator<ApplicationViewModel>.Create(users.Select(u => new ApplicationUserViewModel {
			Id = u.Id,
			FirstName = u.FirstName,
			LastName = u.LastName,
			Rut = u.Rut,
			Email = u.Email,
			Specialization = u.Specialization,
		}).AsQueryable(), pageNumber ?? 1, 10);
		if (!string.IsNullOrEmpty(sortOrder)) {
			paginator.Sort(sortOrder);
		}
		if (!string.IsNullOrEmpty(currentFilter)) {
			paginator.Filter(currentFilter);
		}
		return this.View(paginator);
	}

	[Authorize(Roles = "Candidate,Guide")]
	public async Task<IActionResult> CandidateProposal(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		var parameters = new[] { "Title", "FirstName", "LastName" };
		foreach (var parameter in parameters) {
			this.ViewData[$"{parameter}SortParam"] = sortOrder == parameter ? $"{parameter}Desc" : parameter;
		}
		this.ViewData["CurrentSort"] = sortOrder;
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		if (this.User.IsInRole("Memorist"))
			var memoirs = this._dbContext.Memoirs!.AsNoTracking()
				.Include(m => m.Owners).AsNoTracking()
				.Where(m => m.Owners.Any(o => o!.Id == this._userManager.GetUserId(this.User))
							&& m.Phase == Phase.SentToGuide);
		var paginator = Paginator<ApplicationViewModel>.Create(memoirs.Select(m => new MemoirViewModel {
			Id = m.Id,
			Title = m.Title,
			FirstName = m.Owners.First()!.FirstName,
			LastName = m.Owners.First()!.LastName,
			Phase = m.Phase.ToString(),
		}).AsQueryable(), pageNumber ?? 1, 10);
		if (!string.IsNullOrEmpty(sortOrder)) {
			paginator.Sort(sortOrder);
		}
		if (!string.IsNullOrEmpty(currentFilter)) {
			paginator.Filter(currentFilter);
		}
		return this.View(paginator);
	}
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Utal.Icc.Sgm.Areas.Student.Views.Proposal;
using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Areas.Student.Controllers;

[Area("Student"), Authorize(Roles = "Student")]
public class ProposalController : Controller {
	private readonly ApplicationDbContext _context;
	private readonly UserManager<ApplicationUser> _userManager;

	public ProposalController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) {
		this._context = context;
		this._userManager = userManager;
	}
	
	public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		this.ViewData["TitleSortParam"] = sortOrder == "Title" ? "TitleDesc" : "Title";
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		var student = await this._userManager.GetUserAsync(this.User);
		if (student is null) {
			this.ViewBag.ErrorMessage = "Error al obtener al estudiante.";
			return this.View();
		}
		var orderedProposals = sortOrder switch {
			"Title" => student.StudentProposalsWhichIOwn!.OrderBy(sp => sp.Title),
			"TitleDesc" => student.StudentProposalsWhichIOwn!.OrderByDescending(sp => sp.Title),
			_ => student.StudentProposalsWhichIOwn!.OrderBy(sp => sp.Title)
		};
		var filteredAndOrderedProposals = orderedProposals.ToList();
		if (!string.IsNullOrEmpty(searchString)) {
			filteredAndOrderedProposals = orderedProposals.Where(sp => sp.Title!.ToUpper().Contains(searchString.ToUpper())).ToList();
		}
		var indexViewModels = filteredAndOrderedProposals.Select(sp => new IndexViewModel {
			Title = sp.Title,
			IsDraft = sp.IsDraft,
			IsPending = sp.IsPending,
			IsAccepted = sp.IsAccepted,
		});
		var pageSize = 6;
		return this.View(PaginatedList<IndexViewModel>.Create(indexViewModels.AsQueryable(), pageNumber ?? 1, pageSize));
	}

	public async Task<IActionResult> Create() {
		var guideTeachers = (await this._userManager.GetUsersInRoleAsync(Roles.GuideTeacher.ToString())).OrderBy(gt => gt.LastName);
		var assistantTeachers = (await this._userManager.GetUsersInRoleAsync(Roles.AssistantTeacher.ToString())).OrderBy(at => at.LastName);
		return this.View();
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Create([FromForm] CreateViewModel model) {
		return this.View();
	}
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Utal.Icc.Sgm.Areas.GuideTeacher.Views.Proposal;
using Utal.Icc.Sgm.Data;

using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Areas.GuideTeacher.Controllers;

[Area("GuideTeacher"), Authorize(Roles = "GuideTeacher")]
public class ProposalController : Controller {
	private readonly ApplicationDbContext _dbContext;
	private readonly UserManager<ApplicationUser> _userManager;

	public ProposalController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager) {
		this._dbContext = dbContext;
		this._userManager = userManager;
	}

	public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		var teacher = await this._userManager.GetUserAsync(this.User);
		if (teacher is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (teacher.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		this.ViewData["TitleSortParam"] = sortOrder == "Title" ? "TitleDesc" : "Title";
		this.ViewData["StudentFirstNameSortParam"] = sortOrder == "StudentFirstName" ? "StudentFirstNameDesc" : "StudentFirstName";
		this.ViewData["StudentLastNameSortParam"] = sortOrder == "StudentLastName" ? "StudentLastNameDesc" : "StudentLastName";
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		var studentProposals = this._dbContext.StudentProposals.AsNoTracking()
			.Include(sp => sp.GuideTeacherOfTheStudentProposal).AsNoTracking()
			.Include(sp => sp.StudentOwnerOfTheStudentProposal).AsNoTracking()
			.Where(sp => sp.GuideTeacherOfTheStudentProposal == teacher && sp.ProposalStatus == StudentProposal.Status.Sent);
		var orderedProposals = sortOrder switch {
			"Title" => studentProposals.OrderBy(sp => sp.Title),
			"TitleDesc" => studentProposals.OrderByDescending(sp => sp.Title),
			"StudentFirstName" => studentProposals.OrderBy(sp => sp.StudentOwnerOfTheStudentProposal!.FirstName),
			"StudentFirstNameDesc" => studentProposals.OrderByDescending(sp => sp.StudentOwnerOfTheStudentProposal!.FirstName),
			"StudentLastName" => studentProposals.OrderBy(sp => sp.StudentOwnerOfTheStudentProposal!.LastName),
			"StudentLastNameDesc" => studentProposals.OrderByDescending(sp => sp.StudentOwnerOfTheStudentProposal!.LastName),
			_ => studentProposals.OrderBy(sp => sp.Title)
		};
		var filteredAndOrderedProposals = orderedProposals.ToList();
		if (!string.IsNullOrEmpty(searchString)) {
			filteredAndOrderedProposals = orderedProposals
				.Where(
					sp => sp.Title!.ToUpper().Contains(searchString.ToUpper())
						|| sp.StudentOwnerOfTheStudentProposal!.FirstName!.ToUpper().Contains(searchString.ToUpper())
						|| sp.StudentOwnerOfTheStudentProposal!.LastName!.ToUpper().Contains(searchString.ToUpper()))
				.ToList();
		}
		var indexViewModels = filteredAndOrderedProposals.Select(sp => new IndexViewModel {
			Id = sp.Id.ToString(),
			Title = sp.Title,
			Student = $"{sp.StudentOwnerOfTheStudentProposal!.FirstName} {sp.StudentOwnerOfTheStudentProposal!.LastName}",
			ProposalStatus = sp.ProposalStatus.ToString(),
		});
		var pageSize = 6;
		return this.View(PaginatedList<IndexViewModel>.Create((await this._userManager.GetUserAsync(this.User))!.Id, indexViewModels.AsQueryable(), pageNumber ?? 1, pageSize));
	}
}
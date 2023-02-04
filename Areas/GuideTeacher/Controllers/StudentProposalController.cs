using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Utal.Icc.Sgm.Areas.GuideTeacher.Views.Proposal;
using Utal.Icc.Sgm.Data;

using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Areas.GuideTeacher.Controllers;

[Area("GuideTeacher"), Authorize(Roles = "GuideTeacher")]
public class StudentProposalController : Controller {
	private readonly ApplicationDbContext _dbContext;
	private readonly UserManager<ApplicationUser> _userManager;

	public StudentProposalController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager) {
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
		this.ViewData["StudentLastNameSortParam"] = sortOrder == "StudentLastName" ? "StudentLastNameDesc" : "StudentLastName";
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		var studentProposals = this._dbContext.StudentProposals.AsNoTracking()
			.Where(sp => sp.GuideTeacherOfTheStudentProposal == teacher && (
				sp.ProposalStatus == StudentProposal.Status.SentToGuideTeacher
				|| sp.ProposalStatus == StudentProposal.Status.ApprovedByGuideTeacher))
			.Include(sp => sp.StudentOwnerOfTheStudentProposal).AsNoTracking();
		var orderedProposals = sortOrder switch {
			"Title" => studentProposals.OrderBy(sp => sp.Title),
			"TitleDesc" => studentProposals.OrderByDescending(sp => sp.Title),
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
			Id = sp.Id,
			Title = sp.Title,
			Student = $"{sp.StudentOwnerOfTheStudentProposal!.FirstName} {sp.StudentOwnerOfTheStudentProposal!.LastName}",
			ProposalStatus = sp.ProposalStatus.ToString(),
		});
		var pageSize = 6;
		return this.View(PaginatedList<IndexViewModel>.Create((await this._userManager.GetUserAsync(this.User))!.Id, indexViewModels.AsQueryable(), pageNumber ?? 1, pageSize));
	}

	public new async Task<IActionResult> View(string id) {
		var teacher = await this._userManager.GetUserAsync(this.User);
		if (teacher is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (teacher.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var studentProposal = await this._dbContext.StudentProposals.AsNoTracking()
			.Where(sp => sp.GuideTeacherOfTheStudentProposal == teacher && sp.ProposalStatus == StudentProposal.Status.SentToGuideTeacher)
			.Include(sp => sp.StudentOwnerOfTheStudentProposal).AsNoTracking()
			.Include(sp => sp.AssistantTeacher1OfTheStudentProposal).AsNoTracking()
			.Include(sp => sp.AssistantTeacher2OfTheStudentProposal).AsNoTracking()
			.Include(sp => sp.AssistantTeacher3OfTheStudentProposal).AsNoTracking()
			.FirstOrDefaultAsync(sp => sp.Id == id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction("Index", "StudentProposal", new { area = "GuideTeacher" });
		}
		var viewModel = new ViewModel {
			Id = id,
			Title = studentProposal.Title,
			Description = studentProposal.Description,
			StudentName = $"{studentProposal.StudentOwnerOfTheStudentProposal!.FirstName} {studentProposal.StudentOwnerOfTheStudentProposal!.LastName}",
			StudentEmail = studentProposal.StudentOwnerOfTheStudentProposal.Email,
			StudentRemainingCourses = studentProposal.StudentOwnerOfTheStudentProposal.StudentRemainingCourses,
			StudentIsDoingThePractice = studentProposal.StudentOwnerOfTheStudentProposal.StudentIsDoingThePractice,
			StudentIsWorking = studentProposal.StudentOwnerOfTheStudentProposal.StudentIsWorking,
			AssistantTeacher1 = studentProposal.AssistantTeacher1OfTheStudentProposal is null ? "No asignado" : $"{studentProposal.AssistantTeacher1OfTheStudentProposal!.FirstName} {studentProposal.AssistantTeacher1OfTheStudentProposal!.LastName}",
			AssistantTeacher2 = studentProposal.AssistantTeacher2OfTheStudentProposal is null ? "No asignado" : $"{studentProposal.AssistantTeacher2OfTheStudentProposal!.FirstName} {studentProposal.AssistantTeacher2OfTheStudentProposal!.LastName}",
			AssistantTeacher3 = studentProposal.AssistantTeacher3OfTheStudentProposal is null ? "No asignado" : $"{studentProposal.AssistantTeacher3OfTheStudentProposal!.FirstName} {studentProposal.AssistantTeacher3OfTheStudentProposal!.LastName}",
			CreatedAt = studentProposal.CreatedAt,
			UpdatedAt = studentProposal.UpdatedAt,
		};
		return this.View(viewModel);
	}
	
	public async Task<IActionResult> Reject(string id) {
		var teacher = await this._userManager.GetUserAsync(this.User);
		if (teacher is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (teacher.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var studentProposal = await this._dbContext.StudentProposals.AsNoTracking()
			.Where(sp => sp.GuideTeacherOfTheStudentProposal == teacher && sp.ProposalStatus == StudentProposal.Status.SentToGuideTeacher).AsNoTracking()
			.Include(sp => sp.StudentOwnerOfTheStudentProposal).AsNoTracking()
			.FirstOrDefaultAsync(sp => sp.Id == id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction("Index", "StudentProposal", new { area = "GuideTeacher" });
		}
		var rejectViewModel = new RejectViewModel {
			Id = id,
			Title = studentProposal.Title,
			Student = $"{studentProposal.StudentOwnerOfTheStudentProposal!.FirstName} {studentProposal.StudentOwnerOfTheStudentProposal.LastName}"
		};
		return this.View(rejectViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Reject([FromForm] RejectViewModel model) {
		var teacher = await this._userManager.GetUserAsync(this.User);
		if (teacher is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (teacher.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (!this.ModelState.IsValid) {
			this.ViewBag.WarningMessage = "Revisa que los campos estén correctos.";
			return this.View(new RejectViewModel {
				Id = model.Id,
				Title = model.Title,
				Student = model.Student,
			});
		}
		var studentProposal = await this._dbContext.StudentProposals
			.Where(sp => sp.GuideTeacherOfTheStudentProposal == teacher && sp.ProposalStatus == StudentProposal.Status.SentToGuideTeacher)
			.FirstOrDefaultAsync(sp => sp.Id == model.Id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction("Index", "StudentProposal", new { area = "GuideTeacher" });
		}
		studentProposal.ProposalStatus = StudentProposal.Status.RejectedByGuideTeacher;
		studentProposal.RejectionReason = model.Reason;
		studentProposal.UpdatedAt = DateTimeOffset.Now;
		_ = this._dbContext.StudentProposals.Update(studentProposal);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "La propuesta ha sido rechazada correctamente.";
		return this.RedirectToAction("Index", "StudentProposal", new { area = "GuideTeacher" });
	}

	public async Task<IActionResult> Approve(string id) {
		var teacher = await this._userManager.GetUserAsync(this.User);
		if (teacher is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (teacher.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var studentProposal = await this._dbContext.StudentProposals.AsNoTracking()
			.Where(sp => sp.GuideTeacherOfTheStudentProposal == teacher && sp.ProposalStatus == StudentProposal.Status.SentToGuideTeacher).AsNoTracking()
			.Include(sp => sp.StudentOwnerOfTheStudentProposal).AsNoTracking()
			.FirstOrDefaultAsync(sp => sp.Id == id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction("Index", "StudentProposal", new { area = "GuideTeacher" });
		}
		var approveViewModel = new ApproveViewModel {
			Id = id,
			Title = studentProposal.Title,
			Student = $"{studentProposal.StudentOwnerOfTheStudentProposal!.FirstName} {studentProposal.StudentOwnerOfTheStudentProposal.LastName}"
		};
		return this.View(approveViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Approve([FromForm] ApproveViewModel model) {
		var teacher = await this._userManager.GetUserAsync(this.User);
		if (teacher is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (teacher.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var studentProposal = await this._dbContext.StudentProposals
			.Where(sp => sp.GuideTeacherOfTheStudentProposal == teacher && sp.ProposalStatus == StudentProposal.Status.SentToGuideTeacher)
			.FirstOrDefaultAsync(sp => sp.Id == model.Id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction("Index", "StudentProposal", new { area = "GuideTeacher" });
		}
		studentProposal.ProposalStatus = StudentProposal.Status.ApprovedByGuideTeacher;
		studentProposal.UpdatedAt = DateTimeOffset.Now;
		_ = this._dbContext.StudentProposals.Update(studentProposal);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "La propuesta ha sido aceptada correctamente.";
		return this.RedirectToAction("Index", "StudentProposal", new { area = "GuideTeacher" });
	}
}
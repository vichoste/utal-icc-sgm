using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Utal.Icc.Sgm.Areas.GuideTeacher.Views.Proposal;
using Utal.Icc.Sgm.Data;

using Utal.Icc.Sgm.Models;

using static Utal.Icc.Sgm.Models.ApplicationUser;

namespace Utal.Icc.Sgm.Areas.GuideTeacher.Controllers;

[Area(nameof(Roles.GuideTeacher)), Authorize(Roles = nameof(Roles.GuideTeacher))]
public class StudentProposalController : Controller {
	private readonly ApplicationDbContext _dbContext;
	private readonly UserManager<ApplicationUser> _userManager;

	public StudentProposalController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager) {
		this._dbContext = dbContext;
		this._userManager = userManager;
	}

	protected async Task<ApplicationUser> CheckTeacherSession() {
		var teacher = await this._userManager.GetUserAsync(this.User);
		return teacher is null || teacher.IsDeactivated ? null! : teacher;
	}

	protected async Task<ApplicationUser> CheckApplicationUser(string applicationUserId) {
		var applicationUser = await this._userManager.FindByIdAsync(applicationUserId);
		return applicationUser is null || applicationUser.IsDeactivated ? null! : applicationUser;
	}

	protected void SetSortParameters(string sortOrder, params string[] parameters) {
		foreach (var parameter in parameters) {
			this.ViewData[$"{parameter}SortParam"] = sortOrder == parameter ? $"{parameter}Desc" : parameter;
		}
		this.ViewData["CurrentSort"] = sortOrder;
	}

	protected IOrderedQueryable<StudentProposal> OrderProposals(string sortOrder, IQueryable<StudentProposal> studentProposals, params string[] parameters) {
		foreach (var parameter in parameters) {
			if (parameter == sortOrder) {
				return studentProposals.OrderBy(sp => sp.GetType().GetProperty(parameter));
			} else if ($"{parameter}Desc" == sortOrder) {
				return studentProposals.OrderByDescending(sp => sp.GetType().GetProperty(parameter));
			}
		}
		return studentProposals.OrderBy(sp => sp.GetType().GetProperty(parameters[0]));
	}

	protected List<IndexViewModel> FilterProposals(string searchString, string includeProperty, IQueryable<StudentProposal> studentProposals, params string[] parameters) {
		var result = new List<IndexViewModel>();
		foreach (var parameter in parameters) {
			var partials = studentProposals
					.Where(sp => (sp.GetType().GetProperty(parameter)!.GetValue(sp) as string)!.Contains(searchString))
					.Include(sp => sp.GetType().GetProperty(includeProperty)!.GetValue(sp, null))
					.Select(sp => new IndexViewModel {
				Id = sp.Id,
				Title = sp.Title,
				Student = $"{(sp.GetType().GetProperty(includeProperty)!.GetValue(sp) as ApplicationUser)!.FirstName} {(sp.GetType().GetProperty(includeProperty)!.GetValue(sp, null) as ApplicationUser)!.LastName}",
				ProposalStatus = sp.ProposalStatus.ToString(),
			});
			foreach (var partial in partials) {
				if (!result.Any(ivm => ivm.Id == partial.Id)) {
					result.Add(partial);
				}
			}
		}
		return result;
	}

	public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		if (await this.CheckTeacherSession() is not ApplicationUser teacher) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var parameters = new[] { "Title", "StudentLastName" };
		this.SetSortParameters(sortOrder, parameters);
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
		var orderedProposals = this.OrderProposals(sortOrder, studentProposals, parameters);
		var indexViewModels = !searchString.IsNullOrEmpty() ? this.FilterProposals(searchString, nameof(StudentProposal.StudentOwnerOfTheStudentProposal), orderedProposals, parameters) : orderedProposals.Select(sp => new IndexViewModel {
			Id = sp.Id,
			Title = sp.Title,
			Student = $"{sp.StudentOwnerOfTheStudentProposal!.FirstName} {sp.StudentOwnerOfTheStudentProposal!.LastName}",
			ProposalStatus = sp.ProposalStatus.ToString(),
		}).ToList();
		return this.View(PaginatedList<IndexViewModel>.Create((await this._userManager.GetUserAsync(this.User))!.Id, indexViewModels.AsQueryable(), pageNumber ?? 1, 6));
	}

	public new async Task<IActionResult> View(string id) {
		if (await this.CheckTeacherSession() is not ApplicationUser teacher) {
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
			return this.RedirectToAction("Index", nameof(StudentProposal), new { area = nameof(Roles.GuideTeacher) });
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
		if (await this.CheckTeacherSession() is not ApplicationUser teacher) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var studentProposal = await this._dbContext.StudentProposals.AsNoTracking()
			.Where(sp => sp.GuideTeacherOfTheStudentProposal == teacher && sp.ProposalStatus == StudentProposal.Status.SentToGuideTeacher).AsNoTracking()
			.Include(sp => sp.StudentOwnerOfTheStudentProposal).AsNoTracking()
			.FirstOrDefaultAsync(sp => sp.Id == id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction("Index", nameof(StudentProposal), new { area = nameof(Roles.GuideTeacher) });
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
		if (await this.CheckTeacherSession() is not ApplicationUser teacher) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var studentProposal = await this._dbContext.StudentProposals
			.Where(sp => sp.GuideTeacherOfTheStudentProposal == teacher && sp.ProposalStatus == StudentProposal.Status.SentToGuideTeacher)
			.FirstOrDefaultAsync(sp => sp.Id == model.Id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction("Index", nameof(StudentProposal), new { area = nameof(Roles.GuideTeacher) });
		}
		studentProposal.ProposalStatus = StudentProposal.Status.RejectedByGuideTeacher;
		studentProposal.RejectionReason = model.Reason;
		studentProposal.UpdatedAt = DateTimeOffset.Now;
		_ = this._dbContext.StudentProposals.Update(studentProposal);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "La propuesta ha sido rechazada correctamente.";
		return this.RedirectToAction("Index", nameof(StudentProposal), new { area = nameof(Roles.GuideTeacher) });
	}

	public async Task<IActionResult> Approve(string id) {
		if (await this.CheckTeacherSession() is not ApplicationUser teacher) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var studentProposal = await this._dbContext.StudentProposals.AsNoTracking()
			.Where(sp => sp.GuideTeacherOfTheStudentProposal == teacher && sp.ProposalStatus == StudentProposal.Status.SentToGuideTeacher).AsNoTracking()
			.Include(sp => sp.StudentOwnerOfTheStudentProposal).AsNoTracking()
			.FirstOrDefaultAsync(sp => sp.Id == id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction("Index", nameof(StudentProposal), new { area = nameof(Roles.GuideTeacher) });
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
		if (await this.CheckTeacherSession() is not ApplicationUser teacher) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var studentProposal = await this._dbContext.StudentProposals
			.Where(sp => sp.GuideTeacherOfTheStudentProposal == teacher && sp.ProposalStatus == StudentProposal.Status.SentToGuideTeacher)
			.FirstOrDefaultAsync(sp => sp.Id == model.Id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction("Index", nameof(StudentProposal), new { area = nameof(Roles.GuideTeacher) });
		}
		studentProposal.ProposalStatus = StudentProposal.Status.ApprovedByGuideTeacher;
		studentProposal.UpdatedAt = DateTimeOffset.Now;
		_ = this._dbContext.StudentProposals.Update(studentProposal);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "La propuesta ha sido aceptada correctamente.";
		return this.RedirectToAction("Index", nameof(StudentProposal), new { area = nameof(Roles.GuideTeacher) });
	}
}
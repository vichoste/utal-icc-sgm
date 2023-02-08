using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Utal.Icc.Sgm.Controllers;
using Utal.Icc.Sgm.Data;

using Utal.Icc.Sgm.Models;
using Utal.Icc.Sgm.ViewModels;

using static Utal.Icc.Sgm.Models.ApplicationUser;

namespace Utal.Icc.Sgm.Areas.GuideTeacher.Controllers;

[Area(nameof(GuideTeacher)), Authorize(Roles = nameof(Roles.GuideTeacher))]
public class StudentProposalController : ApplicationController {
	public StudentProposalController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore, SignInManager<ApplicationUser> signInManager) : base(dbContext, userManager, userStore, signInManager) { }

	protected async Task Populate(ApplicationUser guideTeacher) {
		var assistantTeachers = (
			await this._userManager.GetUsersInRoleAsync(nameof(Roles.AssistantTeacher)))
				.Where(at => at != guideTeacher && !at.IsDeactivated)
				.OrderBy(at => at.LastName)
				.ToList();
		this.ViewData[$"{nameof(Roles.AssistantTeacher)}s"] = assistantTeachers.Select(at => new SelectListItem {
			Text = $"{at.FirstName} {at.LastName}",
			Value = at.Id
		});
	}
	
	protected IEnumerable<StudentProposalViewModel> Filter(string searchString, IOrderedEnumerable<StudentProposalViewModel> viewModels, params string[] parameters) {
		var result = new List<StudentProposalViewModel>();
		foreach (var parameter in parameters) {
			var partials = viewModels
					.Where(vm => (vm.GetType().GetProperty(parameter)!.GetValue(vm) as string)!.Contains(searchString));
			foreach (var partial in partials) {
				if (!result.Any(vm => vm.Id == partial.Id)) {
					result.Add(partial);
				}
			}
		}
		return result.AsEnumerable();
	}

	protected IOrderedEnumerable<StudentProposalViewModel> Sort(string sortOrder, IEnumerable<StudentProposalViewModel> viewModels, params string[] parameters) {
		foreach (var parameter in parameters) {
			if (parameter == sortOrder) {
				return viewModels.OrderBy(vm => vm.GetType().GetProperty(parameter)!.GetValue(vm, null));
			} else if ($"{parameter}Desc" == sortOrder) {
				return viewModels.OrderByDescending(vm => vm.GetType().GetProperty(parameter)!.GetValue(vm, null));
			}
		}
		return viewModels.OrderBy(vm => vm.GetType().GetProperty(parameters[0]));
	}

	public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var parameters = new[] { nameof(StudentProposalViewModel.Title), nameof(StudentProposalViewModel.StudentName) };
		this.SetSortParameters(sortOrder, parameters);
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		var proposals = this._dbContext.StudentProposals!.AsNoTracking()
			.Where(p => p.GuideTeacherOfTheStudentProposal == user)
			.Include(p => p.GuideTeacherOfTheStudentProposal).AsNoTracking()
			.Select(p => new StudentProposalViewModel {
				Id = p.Id,
				Title = p.Title,
				StudentName = $"{p.StudentOwnerOfTheStudentProposal!.FirstName} {p.StudentOwnerOfTheStudentProposal!.LastName}",
				ProposalStatus = p.ProposalStatus.ToString(),
			}).AsEnumerable();
		var ordered = this.Sort(sortOrder, proposals, parameters);
		var output = !searchString.IsNullOrEmpty() ? this.Filter(searchString, ordered!, parameters) : ordered;
		return this.View(PaginatedList<StudentProposalViewModel>.Create(output!.AsQueryable(), pageNumber ?? 1, 6));
	}

	public new async Task<IActionResult> View(string id) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.StudentProposals!.AsNoTracking()
			.Include(p => p.GuideTeacherOfTheStudentProposal).AsNoTracking()
			.Where(p => p.GuideTeacherOfTheStudentProposal == user && p.ProposalStatus == StudentProposal.Status.Draft)
			.Include(p => p.StudentOwnerOfTheStudentProposal).AsNoTracking()
			.Include(p => p.AssistantTeachersOfTheStudentProposal).AsNoTracking()
			.FirstOrDefaultAsync(p => p.Id == id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		var output = new StudentProposalViewModel {
			Id = id,
			Title = proposal.Title,
			Description = proposal.Description,
			StudentName = $"{proposal.StudentOwnerOfTheStudentProposal!.FirstName} {proposal.StudentOwnerOfTheStudentProposal!.LastName}",
			StudentEmail = proposal.StudentOwnerOfTheStudentProposal.Email,
			StudentRemainingCourses = proposal.StudentOwnerOfTheStudentProposal.StudentRemainingCourses,
			StudentIsDoingThePractice = proposal.StudentOwnerOfTheStudentProposal.StudentIsDoingThePractice,
			StudentIsWorking = proposal.StudentOwnerOfTheStudentProposal.StudentIsWorking,
			AssistantTeachers = proposal.AssistantTeachersOfTheStudentProposal!.Select(at => at!.Id).ToList(),
			CreatedAt = proposal.CreatedAt,
			UpdatedAt = proposal.UpdatedAt,
		};
		return this.View(output);
	}

	public async Task<IActionResult> Reject(string id) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.StudentProposals!.AsNoTracking()
			.Where(sp => sp.GuideTeacherOfTheStudentProposal == user && sp.ProposalStatus == StudentProposal.Status.SentToGuideTeacher).AsNoTracking()
			.Include(sp => sp.StudentOwnerOfTheStudentProposal).AsNoTracking()
			.FirstOrDefaultAsync(sp => sp.Id == id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		var output = new StudentProposalViewModel {
			Id = id,
			Title = proposal.Title,
			StudentName = $"{proposal.StudentOwnerOfTheStudentProposal!.FirstName} {proposal.StudentOwnerOfTheStudentProposal.LastName}"
		};
		return this.View(output);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Reject([FromForm] StudentProposalViewModel input) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.StudentProposals!
			.Where(sp => sp.GuideTeacherOfTheStudentProposal == user && sp.ProposalStatus == StudentProposal.Status.SentToGuideTeacher)
			.FirstOrDefaultAsync(sp => sp.Id == input.Id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });

		}
		proposal.ProposalStatus = StudentProposal.Status.RejectedByGuideTeacher;
		proposal.RejectionReason = input.RejectionReason;
		proposal.UpdatedAt = DateTimeOffset.Now;
		_ = this._dbContext.StudentProposals!.Update(proposal);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "La propuesta ha sido rechazada correctamente.";
		return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
	}

	public async Task<IActionResult> Approve(string id) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.StudentProposals!.AsNoTracking()
			.Where(sp => sp.GuideTeacherOfTheStudentProposal == user && sp.ProposalStatus == StudentProposal.Status.SentToGuideTeacher).AsNoTracking()
			.Include(sp => sp.StudentOwnerOfTheStudentProposal).AsNoTracking()
			.FirstOrDefaultAsync(sp => sp.Id == id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });

		}
		var output = new StudentProposalViewModel {
			Id = id,
			Title = proposal.Title,
			StudentName = $"{proposal.StudentOwnerOfTheStudentProposal!.FirstName} {proposal.StudentOwnerOfTheStudentProposal.LastName}"
		};
		return this.View(output);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Approve([FromForm] StudentProposalViewModel input) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.StudentProposals!
			.Where(sp => sp.GuideTeacherOfTheStudentProposal == user && sp.ProposalStatus == StudentProposal.Status.SentToGuideTeacher)
			.FirstOrDefaultAsync(sp => sp.Id == input.Id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });

		}
		proposal.ProposalStatus = StudentProposal.Status.ApprovedByGuideTeacher;
		proposal.UpdatedAt = DateTimeOffset.Now;
		_ = this._dbContext.StudentProposals!.Update(proposal);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "La propuesta ha sido aceptada correctamente.";
		return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
	}
}
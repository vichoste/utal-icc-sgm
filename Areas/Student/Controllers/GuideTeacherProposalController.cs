using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Utal.Icc.Sgm.Controllers;
using Utal.Icc.Sgm.Data;

using Utal.Icc.Sgm.Models;
using Utal.Icc.Sgm.ViewModels;

using static Utal.Icc.Sgm.Models.ApplicationUser;
using static Utal.Icc.Sgm.Models.Proposal;

namespace Utal.Icc.Sgm.Areas.Student.Controllers;

[Area(nameof(Student)), Authorize(Roles = nameof(Roles.Student))]
public class GuideTeacherProposalController : ProposalController {
	public override string[]? Parameters { get; set; }

	public GuideTeacherProposalController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore, SignInManager<ApplicationUser> signInManager) : base(dbContext, userManager, userStore, signInManager) { }

	public override async Task PopulateAssistantTeachers(ApplicationUser guideTeacher) {
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

	public override void SetSortParameters(string sortOrder, params string[] parameters) {
		foreach (var parameter in parameters) {
			this.ViewData[$"{parameter}SortParam"] = sortOrder == parameter ? $"{parameter}Desc" : parameter;
		}
		this.ViewData["CurrentSort"] = sortOrder;
	}

	public async Task<IActionResult> List(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var parameters = new[] { nameof(ProposalViewModel.Title), nameof(ProposalViewModel.GuideTeacherName) };
		this.SetSortParameters(sortOrder, parameters);
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		return this.View(PaginatedList<ProposalViewModel>.Create((base.GetPaginatedViewModels<ProposalViewModel>(sortOrder, currentFilter, searchString, pageNumber, parameters,
			() => this._dbContext.Proposals!.AsNoTracking()
				.Where(p => (p.StudentsWhoAreInterestedInThisProposal!.Contains(user) || p.StudentOfTheProposal == user) && (p.ProposalStatus == Status.Published || p.ProposalStatus == Status.Ready) && p.WasMadeByGuideTeacher)
				.Include(p => p.GuideTeacherOfTheProposal).AsNoTracking()
				.Select(p => new ProposalViewModel {
					Id = p.Id,
					Title = p.Title,
					GuideTeacherName = $"{p.GuideTeacherOfTheProposal!.FirstName} {p.GuideTeacherOfTheProposal!.LastName}",
					ProposalStatus = p.ProposalStatus.ToString()
				}).AsEnumerable()
		)), pageNumber ?? 1, 6));
	}

	public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var parameters = new[] { nameof(ProposalViewModel.Title), nameof(ProposalViewModel.GuideTeacherName) };
		this.SetSortParameters(sortOrder, parameters);
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		return this.View(PaginatedList<ProposalViewModel>.Create(base.GetPaginatedViewModels<ProposalViewModel>(sortOrder, currentFilter, searchString, pageNumber, parameters,
			() => this._dbContext.Proposals!.AsNoTracking()
				.Where(p => (p.StudentsWhoAreInterestedInThisProposal!.Contains(user) || p.StudentOfTheProposal == user) && p.ProposalStatus == Status.Published && p.WasMadeByGuideTeacher)
				.Include(p => p.GuideTeacherOfTheProposal).AsNoTracking()
				.Select(p => new ProposalViewModel {
					Id = p.Id,
					Title = p.Title,
					GuideTeacherName = $"{p.GuideTeacherOfTheProposal!.FirstName} {p.GuideTeacherOfTheProposal!.LastName}",
					ProposalStatus = p.ProposalStatus.ToString()
				}).AsEnumerable()
		), pageNumber ?? 1, 6));
	}

	public new async Task<IActionResult> View(string id) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var output = await base.GetAsync<ProposalViewModel>(id, user);
		if (output is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		return this.View(output);
	}

	public async Task<IActionResult> Apply(string id) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.Proposals!.AsNoTracking()
			.Where(p => p.ProposalStatus == Status.Published && p.WasMadeByGuideTeacher)
			.Include(p => p.GuideTeacherOfTheProposal).AsNoTracking()
			.FirstOrDefaultAsync(p => p.Id == id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });

		}
		var output = new ProposalViewModel {
			Id = id,
			Title = proposal.Title,
			GuideTeacherName = $"{proposal.GuideTeacherOfTheProposal!.FirstName} {proposal.GuideTeacherOfTheProposal.LastName}"
		};
		return this.View(output);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Apply([FromForm] ProposalViewModel input) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.Proposals!.AsNoTracking()
			.Where(p => p.ProposalStatus == Status.Published && p.WasMadeByGuideTeacher)
			.Include(p => p.GuideTeacherOfTheProposal).AsNoTracking()
			.FirstOrDefaultAsync(p => p.Id == input.Id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		proposal.StudentsWhoAreInterestedInThisProposal!.Add(user);
		proposal.UpdatedAt = DateTimeOffset.Now;
		_ = this._dbContext.Proposals!.Update(proposal);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "Has postulado a la propuesta correctamente.";
		return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
	}
	public async Task<IActionResult> Summary(string id) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.Proposals!.AsNoTracking()
			.Where(p => (p.StudentsWhoAreInterestedInThisProposal!.Contains(user) || p.StudentOfTheProposal == user) && (p.ProposalStatus == Status.Published || p.ProposalStatus == Status.Ready) && p.WasMadeByGuideTeacher)
			.Include(p => p.GuideTeacherOfTheProposal).AsNoTracking()
			.Include(p => p.StudentOfTheProposal).AsNoTracking()
			.Include(p => p.AssistantTeachersOfTheProposal).AsNoTracking()
			.FirstOrDefaultAsync(p => p.Id == id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		var output = new ProposalViewModel {
			Id = id,
			Title = proposal.Title,
			Description = proposal.Description,
			Requirements = proposal.Requirements,
			GuideTeacherName = $"{proposal.GuideTeacherOfTheProposal!.FirstName} {proposal.GuideTeacherOfTheProposal!.LastName}",
			GuideTeacherEmail = proposal.GuideTeacherOfTheProposal.Email,
			GuideTeacherOffice = proposal.GuideTeacherOfTheProposal.TeacherOffice,
			GuideTeacherSchedule = proposal.GuideTeacherOfTheProposal.TeacherSchedule,
			GuideTeacherSpecialization = proposal.GuideTeacherOfTheProposal.TeacherSpecialization,
			AssistantTeachers = proposal.AssistantTeachersOfTheProposal!.Select(at => $"{at!.FirstName} {at!.LastName}").ToList(),
			CreatedAt = proposal.CreatedAt,
			UpdatedAt = proposal.UpdatedAt,
		};
		return this.View(output);
	}
}
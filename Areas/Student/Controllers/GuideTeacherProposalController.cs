using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Utal.Icc.Sgm.Controllers;
using Utal.Icc.Sgm.Data;

using Utal.Icc.Sgm.Models;
using Utal.Icc.Sgm.ViewModels;

using static Utal.Icc.Sgm.Models.ApplicationUser;
using static Utal.Icc.Sgm.Models.GuideTeacherProposal;

namespace Utal.Icc.Sgm.Areas.Student.Controllers;

[Area(nameof(Student)), Authorize(Roles = nameof(Roles.Student))]
public class GuideTeacherProposalController : ProposalController {
	public GuideTeacherProposalController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore, SignInManager<ApplicationUser> signInManager) : base(dbContext, userManager, userStore, signInManager) { }

	public async Task<IActionResult> List(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		return await base.Index<GuideTeacherProposalViewModel>(sortOrder, currentFilter, searchString, pageNumber, new[] { nameof(GuideTeacherProposalViewModel.Title), nameof(GuideTeacherProposalViewModel.GuideTeacherName) },
			() => (this._dbContext.GuideTeacherProposals!.AsNoTracking()
				.Where(p => (p.StudentsWhoAreInterestedInThisGuideTeacherProposal!.Contains(user) || p.StudentWhoIsAssignedToThisGuideTeacherProposal == user) && (p.ProposalStatus == Status.Published || p.ProposalStatus == Status.Ready))
				.Include(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal).AsNoTracking()
				.Select(p => new GuideTeacherProposalViewModel {
					Id = p.Id,
					Title = p.Title,
					GuideTeacherName = $"{p.GuideTeacherOwnerOfTheGuideTeacherProposal!.FirstName} {p.GuideTeacherOwnerOfTheGuideTeacherProposal!.LastName}",
					ProposalStatus = p.ProposalStatus.ToString(),
					StudentIsAccepted = p.StudentWhoIsAssignedToThisGuideTeacherProposal! == user
				}).AsEnumerable()
			)
		);
	}

	public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		return await base.Index<GuideTeacherProposalViewModel>(sortOrder, currentFilter, searchString, pageNumber, new[] { nameof(GuideTeacherProposalViewModel.Title), nameof(GuideTeacherProposalViewModel.GuideTeacherName) },
			() => (this._dbContext.GuideTeacherProposals!.AsNoTracking()
				.Where(p => !p.StudentsWhoAreInterestedInThisGuideTeacherProposal!.Contains(user) && p.ProposalStatus == Status.Published)
				.Include(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal).AsNoTracking()
				.Select(p => new GuideTeacherProposalViewModel {
					Id = p.Id,
					Title = p.Title,
					GuideTeacherName = $"{p.GuideTeacherOwnerOfTheGuideTeacherProposal!.FirstName} {p.GuideTeacherOwnerOfTheGuideTeacherProposal!.LastName}",
				}).AsEnumerable()
			)
		);
	}

	public new async Task<IActionResult> View(string id) => return base.View();

	public async Task<IActionResult> Apply(string id) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.GuideTeacherProposals!.AsNoTracking()
			.Where(p => p.ProposalStatus == Status.Published)
			.Include(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal).AsNoTracking()
			.FirstOrDefaultAsync(p => p.Id == id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });

		}
		var output = new GuideTeacherProposalViewModel {
			Id = id,
			Title = proposal.Title,
			GuideTeacherName = $"{proposal.GuideTeacherOwnerOfTheGuideTeacherProposal!.FirstName} {proposal.GuideTeacherOwnerOfTheGuideTeacherProposal.LastName}"
		};
		return this.View(output);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Apply([FromForm] GuideTeacherProposalViewModel input) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.GuideTeacherProposals!.AsNoTracking()
			.Where(p => p.ProposalStatus == Status.Published)
			.Include(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal).AsNoTracking()
			.FirstOrDefaultAsync(p => p.Id == input.Id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		proposal.StudentsWhoAreInterestedInThisGuideTeacherProposal!.Add(user);
		proposal.UpdatedAt = DateTimeOffset.Now;
		_ = this._dbContext.GuideTeacherProposals!.Update(proposal);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "Has postulado a la propuesta correctamente.";
		return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
	}
	public async Task<IActionResult> Summary(string id) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.GuideTeacherProposals!.AsNoTracking()
			.Where(p => (p.StudentsWhoAreInterestedInThisGuideTeacherProposal!.Contains(user) || p.StudentWhoIsAssignedToThisGuideTeacherProposal == user) && (p.ProposalStatus == Status.Published || p.ProposalStatus == Status.Ready))
			.Include(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal).AsNoTracking()
			.Include(p => p.StudentWhoIsAssignedToThisGuideTeacherProposal).AsNoTracking()
			.Include(p => p.AssistantTeachersOfTheGuideTeacherProposal).AsNoTracking()
			.FirstOrDefaultAsync(p => p.Id == id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		var output = new GuideTeacherProposalViewModel {
			Id = id,
			Title = proposal.Title,
			Description = proposal.Description,
			Requirements = proposal.Requirements,
			GuideTeacherName = $"{proposal.GuideTeacherOwnerOfTheGuideTeacherProposal!.FirstName} {proposal.GuideTeacherOwnerOfTheGuideTeacherProposal!.LastName}",
			GuideTeacherEmail = proposal.GuideTeacherOwnerOfTheGuideTeacherProposal.Email,
			GuideTeacherOffice = proposal.GuideTeacherOwnerOfTheGuideTeacherProposal.TeacherOffice,
			GuideTeacherSchedule = proposal.GuideTeacherOwnerOfTheGuideTeacherProposal.TeacherSchedule,
			GuideTeacherSpecialization = proposal.GuideTeacherOwnerOfTheGuideTeacherProposal.TeacherSpecialization,
			AssistantTeachers = proposal.AssistantTeachersOfTheGuideTeacherProposal!.Select(at => $"{at!.FirstName} {at!.LastName}").ToList(),
			CreatedAt = proposal.CreatedAt,
			UpdatedAt = proposal.UpdatedAt,
			StudentIsAccepted = proposal.StudentWhoIsAssignedToThisGuideTeacherProposal is not null ? proposal.StudentWhoIsAssignedToThisGuideTeacherProposal.Id == user.Id ? true : false : false, // For some reason, IDs must be checked...
		};
		return this.View(output);
	}
}
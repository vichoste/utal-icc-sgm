using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;
using Utal.Icc.Sgm.ViewModels;

using static Utal.Icc.Sgm.Models.ApplicationUser;

namespace Utal.Icc.Sgm.Controllers;

public abstract class ProposalController : ApplicationController {
	public ProposalController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore, SignInManager<ApplicationUser> signInManager) : base(dbContext, userManager, userStore, signInManager) { }

	protected async Task<IActionResult> View<T>(string id, string action, string controller, string area, Func<Task<Proposal>> getProposal) where T : ProposalViewModel, new() {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		if (await getProposal() is not Proposal proposal) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(action, controller, new { area });
		}
		var output = new T {
			Id = id,
			Title = proposal.Title,
			Description = proposal.Description,
			AssistantTeachers = proposal.AssistantTeachersOfTheProposal!.Select(at => $"{at!.FirstName} {at!.LastName}").ToList(),
			CreatedAt = proposal.CreatedAt,
			UpdatedAt = proposal.UpdatedAt,
			GuideTeacherId = proposal.GuideTeacherOfTheProposal?.Id,
			GuideTeacherName = $"{proposal.GuideTeacherOfTheProposal?.FirstName} {proposal.GuideTeacherOfTheProposal?.LastName}",
			Requirements = proposal.Requirements,
			GuideTeacherEmail = proposal.GuideTeacherOfTheProposal?.Email,
			GuideTeacherOffice = proposal.GuideTeacherOfTheProposal?.TeacherOffice,
			GuideTeacherSchedule = proposal.GuideTeacherOfTheProposal?.TeacherSchedule,
			GuideTeacherSpecialization = proposal.GuideTeacherOfTheProposal?.TeacherSpecialization,
			StudentsWhoAreInterestedInThisProposal = proposal.StudentsWhoAreInterestedInThisProposal?.Select(s => $"{s!.FirstName} {s!.LastName}"),
			StudentId = proposal.StudentOfTheProposal?.Id,
			StudentName = $"{proposal.StudentOfTheProposal?.FirstName} {proposal.StudentOfTheProposal?.LastName}",
			StudentEmail = proposal.StudentOfTheProposal?.Email,
			StudentUniversityId = proposal.StudentOfTheProposal?.StudentUniversityId,
			StudentRemainingCourses = proposal.StudentOfTheProposal?.StudentRemainingCourses,
			StudentIsDoingThePractice = proposal.StudentOfTheProposal?.StudentIsDoingThePractice,
			StudentIsWorking = proposal.StudentOfTheProposal?.StudentIsWorking,
		};
		return this.View(output);
	}

	public async Task<IActionResult> Create(string? id, string action, string controller, string area) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		await this.PopulateAssistantTeachers(user);
		var output = new ProposalViewModel();
		if (id is string guideTeacherId) {
			if (await base.CheckApplicationUser(id) is not ApplicationUser guideTeacher) {
				this.TempData["ErrorMessage"] = "Error al obtener al profesor guía.";
				return this.RedirectToAction(action, controller, new { area });
			}
			output.GuideTeacherId = guideTeacher.Id;
			output.GuideTeacherName = $"{guideTeacher.FirstName} {guideTeacher.LastName}";
			output.GuideTeacherEmail = guideTeacher.Email;
			output.GuideTeacherOffice = guideTeacher.TeacherOffice;
			output.GuideTeacherSchedule = guideTeacher.TeacherSchedule;
			output.GuideTeacherSpecialization = guideTeacher.TeacherSpecialization;
		}
		var output = new ProposalViewModel {
			GuideTeacherId = user.Id,
			GuideTeacherName = $"{user.FirstName} {user.LastName}",
			GuideTeacherEmail = user.Email,
			GuideTeacherOffice = user.TeacherOffice,
			GuideTeacherSchedule = user.TeacherSchedule,
			GuideTeacherSpecialization = user.TeacherSpecialization,
		};
		return this.View(output);
	}

	public async Task<IActionResult> Create([FromForm] GuideTeacherProposalViewModel input) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var assistantTeachers = input.AssistantTeachers!.Select(async at => await this.CheckApplicationUser(at)).Select(at => at.Result).ToList();
		var proposal = new GuideTeacherProposal {
			Id = Guid.NewGuid().ToString(),
			Title = input.Title,
			Description = input.Description,
			Requirements = input.Requirements,
			GuideTeacherOwnerOfTheGuideTeacherProposal = user,
			AssistantTeachersOfTheGuideTeacherProposal = assistantTeachers!,
			ProposalStatus = GuideTeacherProposal.Status.Draft,
			CreatedAt = DateTimeOffset.Now,
			UpdatedAt = DateTimeOffset.Now
		};
		_ = await this._dbContext.GuideTeacherProposals!.AddAsync(proposal);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "Tu propuesta ha sido registrada correctamente.";
		return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
	}
}
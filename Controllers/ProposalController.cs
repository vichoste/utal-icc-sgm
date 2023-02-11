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

	protected async Task PopulateAssistantTeachers(ApplicationUser guideTeacher) {
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
		};
		if (proposal is GuideTeacherProposal guideTeacherProposal && output is GuideTeacherProposalViewModel guideTeacherProposalViewModel) {
			guideTeacherProposalViewModel.Requirements = guideTeacherProposal.Requirements;
			guideTeacherProposalViewModel.GuideTeacherName = $"{guideTeacherProposal.GuideTeacherOwnerOfTheGuideTeacherProposal!.FirstName} {guideTeacherProposal.GuideTeacherOwnerOfTheGuideTeacherProposal!.LastName}";
			guideTeacherProposalViewModel.GuideTeacherEmail = guideTeacherProposal.GuideTeacherOwnerOfTheGuideTeacherProposal!.Email;
			guideTeacherProposalViewModel.GuideTeacherOffice = guideTeacherProposal.GuideTeacherOwnerOfTheGuideTeacherProposal!.TeacherOffice;
			guideTeacherProposalViewModel.GuideTeacherSchedule = guideTeacherProposal.GuideTeacherOwnerOfTheGuideTeacherProposal!.TeacherSpecialization;
			guideTeacherProposalViewModel.GuideTeacherSpecialization = guideTeacherProposal.GuideTeacherOwnerOfTheGuideTeacherProposal!.TeacherSpecialization;
		} else if (proposal is StudentProposal studentProposal && output is StudentProposalViewModel studentProposalViewModel) {
			studentProposalViewModel.StudentUniversityId = studentProposal.StudentOwnerOfTheStudentProposal!.StudentUniversityId;
			studentProposalViewModel.StudentName = $"{studentProposal.StudentOwnerOfTheStudentProposal!.FirstName} {studentProposal.StudentOwnerOfTheStudentProposal!.LastName}";
			studentProposalViewModel.StudentEmail = studentProposal.StudentOwnerOfTheStudentProposal!.Email;
			studentProposalViewModel.StudentRemainingCourses = studentProposal.StudentOwnerOfTheStudentProposal!.StudentRemainingCourses;
			studentProposalViewModel.StudentIsDoingThePractice = studentProposal.StudentOwnerOfTheStudentProposal!.StudentIsDoingThePractice;
			studentProposalViewModel.StudentIsWorking = studentProposal.StudentOwnerOfTheStudentProposal!.StudentIsWorking;
		}
		return this.View(output);
	}
}
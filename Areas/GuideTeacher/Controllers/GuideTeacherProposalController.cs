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

	public async Task<IActionResult> Students(string id, string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var parameters = new[] { nameof(ApplicationUserViewModel.FirstName), nameof(ApplicationUserViewModel.LastName), nameof(ApplicationUserViewModel.StudentUniversityId), nameof(ApplicationUserViewModel.Rut), nameof(ApplicationUserViewModel.Email) };
		this.SetSortParameters(sortOrder, parameters);
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		return this.View(base.GetPaginatedViewModels<ApplicationUserViewModel>(sortOrder, currentFilter, searchString, pageNumber, parameters,
			() => (this._dbContext.Proposals!
				.Where(p => p.GuideTeacherOfTheProposal == user && p.Id == id)
				.Include(p => p.StudentsWhoAreInterestedInThisProposal)
				.SelectMany(p => p.StudentsWhoAreInterestedInThisProposal!.Select(
					u => new ApplicationUserViewModel {
						Id = u!.Id,
						FirstName = u.FirstName,
						LastName = u.LastName,
						StudentUniversityId = u.StudentUniversityId,
						Rut = u.Rut,
						Email = u.Email,
						IsDeactivated = u.IsDeactivated
					}
				)).AsEnumerable()
			)
		));
	}

	public async Task<IActionResult> Index(string id, string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var parameters = new[] { nameof(ApplicationUserViewModel.FirstName), nameof(ApplicationUserViewModel.LastName), nameof(ApplicationUserViewModel.StudentUniversityId), nameof(ApplicationUserViewModel.Rut), nameof(ApplicationUserViewModel.Email) };
		this.SetSortParameters(sortOrder, parameters);
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		return this.View(base.GetPaginatedViewModels<ProposalViewModel>(sortOrder, currentFilter, searchString, pageNumber, parameters,
			() => (this._dbContext.Proposals!.AsNoTracking()
				.Where(p => p.GuideTeacherOfTheProposal == user)
				.Select(p => new ProposalViewModel {
					Id = p.Id,
					Title = p.Title,
					ProposalStatus = p.ProposalStatus.ToString(),
				}).AsEnumerable()
			)
		));
	}

	public async Task<IActionResult> Create() {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		await this.PopulateAssistantTeachers(user);
		var output = base.Create<ProposalViewModel>();
		return this.View(output);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Create([FromForm] ProposalViewModel input) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var output = base.CreateAsync<ProposalViewModel>(input, user);
		if (output is null) {
			this.TempData["ErrorMessage"] = "Error al registrar tu propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		this.TempData["SuccessMessage"] = "Tu propuesta ha sido registrada correctamente.";
		return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
	}

	public async Task<IActionResult> Edit(string id) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var output = await base.EditAsync<ProposalViewModel>(id, user);
		return this.View(output);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit([FromForm] ProposalViewModel input) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var output = await base.EditAsync<ProposalViewModel>(input, user);
		if (output is null) {
			this.TempData["ErrorMessage"] = "Error al actualizar tu propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		this.ViewBag.SuccessMessage = "Tu propuesta ha sido actualizada correctamente.";
		return this.View(output);
	}

	public async Task<IActionResult> Delete(string id) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var output = await base.DeleteAsync<ProposalViewModel>(id, user);
		return this.View(output);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete([FromForm] ProposalViewModel input) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		if (!await base.DeleteAsync<ProposalViewModel>(input, user)) {
			this.TempData["ErrorMessage"] = "Error al eliminar tu propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		this.TempData["SuccessMessage"] = "Tu propuesta ha sido eliminada correctamente.";
		return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
	}

	public async Task<IActionResult> Publish(string id) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.GuideTeacherProposals!.AsNoTracking()
			.Where(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal == user && p.ProposalStatus == GuideTeacherProposal.Status.Draft)
			.FirstOrDefaultAsync(p => p.Id == id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		var output = new GuideTeacherProposalViewModel {
			Id = id,
			Title = proposal.Title,
		};
		return this.View(output);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Publish([FromForm] GuideTeacherProposalViewModel input) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.GuideTeacherProposals!
			.Where(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal == user && p.ProposalStatus == GuideTeacherProposal.Status.Draft)
			.Include(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal)
			.Include(p => p.AssistantTeachersOfTheGuideTeacherProposal)
			.FirstOrDefaultAsync(p => p.Id == input.Id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		if (await base.CheckApplicationUser(proposal.GuideTeacherOwnerOfTheGuideTeacherProposal!.Id) is null) {
			this.TempData["ErrorMessage"] = "Error al obtener el profesor guía.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		foreach (var assistantTeacher in proposal.AssistantTeachersOfTheGuideTeacherProposal!) {
			if (await base.CheckApplicationUser(assistantTeacher!.Id) is null) {
				this.TempData["ErrorMessage"] = "Error al obtener el profesor co-guía.";
				return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
			}
		}
		proposal.ProposalStatus = GuideTeacherProposal.Status.Published;
		proposal.UpdatedAt = DateTimeOffset.Now;
		_ = this._dbContext.GuideTeacherProposals!.Update(proposal);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "Tu propuesta ha sido enviada correctamente.";
		return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
	}

	public async Task<IActionResult> Select(string proposalId, string studentId) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		if (await base.CheckApplicationUser(studentId) is not ApplicationUser student) {
			this.TempData["ErrorMessage"] = "Error al obtener el estudiante.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		var proposal = await this._dbContext.GuideTeacherProposals!.AsNoTracking()
			.Where(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal == user && p.ProposalStatus == GuideTeacherProposal.Status.Published)
			.Include(p => p.StudentsWhoAreInterestedInThisGuideTeacherProposal).AsNoTracking()
			.Where(p => p.StudentsWhoAreInterestedInThisGuideTeacherProposal!.Any(s => s!.Id == studentId))
			.FirstOrDefaultAsync(p => p.Id == proposalId);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		var output = new GuideTeacherProposalViewModel {
			Id = proposalId,
			StudentName = $"{student.FirstName} {student.LastName}",
			StudentId = studentId,
			StudentEmail = student.Email,
			StudentRemainingCourses = student.StudentRemainingCourses,
			StudentIsDoingThePractice = student.StudentIsDoingThePractice,
			StudentIsWorking = student.StudentIsWorking
		};
		return this.View(output);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Select([FromForm] GuideTeacherProposalViewModel input) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		if (await base.CheckApplicationUser(input.StudentId!) is not ApplicationUser student) {
			this.TempData["ErrorMessage"] = "Error al obtener el estudiante.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		var proposal = await this._dbContext.GuideTeacherProposals!
			.Where(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal == user && p.ProposalStatus == GuideTeacherProposal.Status.Published)
			.Include(p => p.StudentsWhoAreInterestedInThisGuideTeacherProposal)
			.Where(p => p.StudentsWhoAreInterestedInThisGuideTeacherProposal!.Any(s => s!.Id == input.StudentId!))
			.FirstOrDefaultAsync(p => p.Id == input.Id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		if (proposal.StudentsWhoAreInterestedInThisGuideTeacherProposal!.Any(s => s!.Id == student.Id)) {
			_ = proposal.StudentsWhoAreInterestedInThisGuideTeacherProposal!.Remove(student);
		}
		proposal.ProposalStatus = GuideTeacherProposal.Status.Ready;
		proposal.StudentWhoIsAssignedToThisGuideTeacherProposal = student;
		proposal.UpdatedAt = DateTimeOffset.Now;
		_ = this._dbContext.GuideTeacherProposals!.Update(proposal);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "El estudiante ha sido seleccionado a la propuesta correctamente.";
		return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
	}

	public async Task<IActionResult> Convert(string id) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.GuideTeacherProposals!.AsNoTracking()
			.Where(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal == user && p.ProposalStatus == GuideTeacherProposal.Status.Ready)
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
			StudentName = $"{proposal.StudentWhoIsAssignedToThisGuideTeacherProposal!.FirstName} {proposal.StudentWhoIsAssignedToThisGuideTeacherProposal!.LastName}",
			StudentEmail = proposal.StudentWhoIsAssignedToThisGuideTeacherProposal!.Email,
			StudentUniversityId = proposal.StudentWhoIsAssignedToThisGuideTeacherProposal!.StudentUniversityId,
			StudentRemainingCourses = proposal.StudentWhoIsAssignedToThisGuideTeacherProposal!.StudentRemainingCourses,
			StudentIsDoingThePractice = proposal.StudentWhoIsAssignedToThisGuideTeacherProposal!.StudentIsDoingThePractice,
			StudentIsWorking = proposal.StudentWhoIsAssignedToThisGuideTeacherProposal!.StudentIsWorking,
			AssistantTeachers = proposal.AssistantTeachersOfTheGuideTeacherProposal!.Select(at => $"{at!.FirstName} {at!.LastName}").ToList(),
			CreatedAt = proposal.CreatedAt,
			UpdatedAt = proposal.UpdatedAt
		};
		return this.View(output);
	}
	public new async Task<IActionResult> View(string id) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.GuideTeacherProposals!
			.Include(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal)
			.Where(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal == user && p.ProposalStatus == GuideTeacherProposal.Status.Published)
			.Include(p => p.AssistantTeachersOfTheGuideTeacherProposal)
			.FirstOrDefaultAsync(p => p.Id == id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		var output = new GuideTeacherProposalViewModel {
			Id = id,
			Title = proposal.Title,
			Description = proposal.Description,
			Requirements = proposal.Requirements,
			AssistantTeachers = proposal.AssistantTeachersOfTheGuideTeacherProposal!.Select(at => $"{at!.FirstName} {at!.LastName}").ToList(),
			CreatedAt = proposal.CreatedAt,
			UpdatedAt = proposal.UpdatedAt,
		};
		return this.View(output);
	}
}
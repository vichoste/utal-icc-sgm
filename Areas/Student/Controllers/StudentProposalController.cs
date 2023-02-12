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
public class StudentProposalController : ProposalController {
	public override string[]? Parameters { get; set; }

	public StudentProposalController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore, SignInManager<ApplicationUser> signInManager) : base(dbContext, userManager, userStore, signInManager) { }

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

	public async Task<IActionResult> GuideTeacher(string id, string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var parameters = new[] { nameof(ApplicationUserViewModel.FirstName), nameof(ApplicationUserViewModel.LastName), nameof(ApplicationUserViewModel.Email), nameof(ApplicationUserViewModel.TeacherSpecialization) };
		this.SetSortParameters(sortOrder, parameters);
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		return this.View(base.GetPaginatedViewModelsAsync<ApplicationUserViewModel>(sortOrder, currentFilter, searchString, pageNumber, parameters,
			async () => (await this._userManager.GetUsersInRoleAsync(nameof(Roles.GuideTeacher)))
				.Where(u => !u.IsDeactivated)
				.Select(
					u => new ApplicationUserViewModel {
						Id = u.Id,
						FirstName = u.FirstName,
						LastName = u.LastName,
						Email = u.Email,
						TeacherSpecialization = u.TeacherSpecialization
					}
				).AsEnumerable()
			)
		));
	}

	public async Task<IActionResult> Index(string id, string sortOrder, string currentFilter, string searchString, int? pageNumber) {
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
		return this.View(base.GetPaginatedViewModels<ProposalViewModel>(sortOrder, currentFilter, searchString, pageNumber, parameters,
			() => this._dbContext.Proposals!.AsNoTracking()
				.Where(p => p.StudentOfTheProposal == user)
				.Include(p => p.GuideTeacherOfTheProposal).AsNoTracking()
				.Select(p => new ProposalViewModel {
					Id = p.Id,
					Title = p.Title,
					GuideTeacherName = $"{p.GuideTeacherOfTheProposal!.FirstName} {p.GuideTeacherOfTheProposal!.LastName}",
					ProposalStatus = p.ProposalStatus.ToString(),
			}).AsEnumerable()
		));
	}

	public async Task<IActionResult> Create(string id) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		await this.PopulateAssistantTeachers(user);
		var output = base.CreateAsync<ProposalViewModel>(id);
		if (output is null) {
			this.TempData["ErrorMessage"] = "Error al registrar tu propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
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
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		this.TempData["SuccessMessage"] = "Tu propuesta ha sido registrada correctamente.";
		return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
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
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
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
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		this.TempData["SuccessMessage"] = "Tu propuesta ha sido eliminada correctamente.";
		return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
	}

	public async Task<IActionResult> Convert(string id) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var output = await base.GetAsync<ProposalViewModel>(id, user);
		if (output is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		return this.View(output);
	}

	public new async Task<IActionResult> View(string id) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var output = await base.GetAsync<ProposalViewModel>(id, user);
		if (output is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		return this.View(output);
	}

	public async Task<IActionResult> Publish(string id) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var output = await base.PublishAsync<ProposalViewModel>(id, user);
		if (output is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		return this.View(output);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Publish([FromForm] ProposalViewModel input) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		if (!await base.PublishAsync<ProposalViewModel>(input, user)) {
			this.TempData["ErrorMessage"] = base._userManager.IsInRoleAsync(user, nameof(Roles.Student)) ? "Error al enviar tu propuesta." : "Error al publicar tu propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		this.TempData["SuccessMessage"] = base._userManager.IsInRoleAsync(user, nameof(Roles.Student)) ? "Tu propuesta ha sido enviada correctamente." : "Tu propuesta ha sido publicada correctamente.";
		return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
	}

	public async Task<IActionResult> Select(string proposalId, string studentId) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		if (await base.CheckApplicationUser(studentId) is not ApplicationUser student) {
			this.TempData["ErrorMessage"] = "Error al obtener el estudiante.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		var proposal = await this._dbContext.Proposals!.AsNoTracking()
			.Where(p => p.StudentOfTheProposal == user && p.ProposalStatus == Status.Published)
			.Include(p => p.StudentsWhoAreInterestedInThisProposal).AsNoTracking()
			.Where(p => p.StudentsWhoAreInterestedInThisProposal!.Any(s => s!.Id == studentId))
			.FirstOrDefaultAsync(p => p.Id == proposalId);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		var output = new ProposalViewModel {
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
	public async Task<IActionResult> Select([FromForm] ProposalViewModel input) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		if (await base.CheckApplicationUser(input.StudentId!) is not ApplicationUser student) {
			this.TempData["ErrorMessage"] = "Error al obtener el estudiante.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		var proposal = await this._dbContext.Proposals!
			.Where(p => p.StudentOfTheProposal == user && p.ProposalStatus == Status.Published)
			.Include(p => p.StudentsWhoAreInterestedInThisProposal)
			.Where(p => p.StudentsWhoAreInterestedInThisProposal!.Any(s => s!.Id == input.StudentId!))
			.FirstOrDefaultAsync(p => p.Id == input.Id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		if (proposal.StudentsWhoAreInterestedInThisProposal!.Any(s => s!.Id == student.Id)) {
			_ = proposal.StudentsWhoAreInterestedInThisProposal!.Remove(student);
		}
		proposal.ProposalStatus = Status.Ready;
		proposal.StudentOfTheProposal = student;
		proposal.UpdatedAt = DateTimeOffset.Now;
		_ = this._dbContext.Proposals!.Update(proposal);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "El estudiante ha sido seleccionado a la propuesta correctamente.";
		return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
	}
}
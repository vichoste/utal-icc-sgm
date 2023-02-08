using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Utal.Icc.Sgm.Areas.Student.Views.StudentProposal;
using Utal.Icc.Sgm.Controllers;
using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;

using static Utal.Icc.Sgm.Models.ApplicationUser;

namespace Utal.Icc.Sgm.Areas.Student.Controllers;

[Area(nameof(Student)), Authorize(Roles = nameof(Roles.Student))]
public class StudentProposalController : ApplicationController, IAssistantTeacherPopulateable {
	public StudentProposalController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager) : base(dbContext, userManager) {
	}

	public async Task Populate(ApplicationUser guideTeacher) {
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

	public async Task<IActionResult> GuideTeacher(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		if (await this.CheckSession() is not ApplicationUser student) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var parameters = new[] { nameof(ApplicationUser.FirstName), nameof(ApplicationUser.LastName), nameof(ApplicationUser.Email), nameof(ApplicationUser.TeacherOffice), nameof(ApplicationUser.TeacherSchedule), nameof(ApplicationUser.TeacherSpecialization) };
		this.SetSortParameters(sortOrder, parameters);
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		var guideTeachers = await this._userManager.GetUsersInRoleAsync(nameof(Roles.GuideTeacher));
		var orderedGuideTeachers = this.OrderGuideTeachers(sortOrder, guideTeachers, parameters);
		var guideTeacherViewModels = !searchString.IsNullOrEmpty() ? this.FilterGuideTeachers(searchString, orderedGuideTeachers, parameters) : orderedGuideTeachers.Select(gt => new GuideTeacherViewModel {
			Id = gt.Id,
			GuideTeacherFirstName = gt.FirstName,
			GuideTeacherLastName = gt.LastName,
			GuideTeacherEmail = gt.Email,
			GuideTeacherOffice = gt.TeacherOffice,
			GuideTeacherSchedule = gt.TeacherSchedule,
			GuideTeacherSpecialization = gt.TeacherSpecialization
		}).ToList();
		return this.View(PaginatedList<GuideTeacherViewModel>.Create(guideTeacherViewModels.AsQueryable(), pageNumber ?? 1, 6));
	}

	public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		if (await this.CheckSession() is not ApplicationUser student) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var parameters = new[] { nameof(StudentProposal.Title), nameof(Roles.GuideTeacher) };
		this.SetSortParameters(sortOrder, parameters);
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		var proposals = this._dbContext.StudentProposals!.AsNoTracking()
			.Where(sp => sp.StudentOwnerOfTheStudentProposal == student)
			.Include(sp => sp.GuideTeacherOfTheStudentProposal).AsNoTracking()
			.Select(sp => new IndexViewModel {
				Id = sp.Id,
				Title = sp.Title,
				GuideTeacherName = $"{sp.GuideTeacherOfTheStudentProposal!.FirstName} {sp.GuideTeacherOfTheStudentProposal!.LastName}",
				ProposalStatus = sp.ProposalStatus.ToString(),
			}).AsEnumerable();
		var orderedProposals = this.OrderProposals(sortOrder, proposals, parameters);
		var filteredAndOrderedProposals = !searchString.IsNullOrEmpty() ?
			this.FilterProposals(searchString, orderedProposals, parameters)
			: orderedProposals;
		return this.View(PaginatedList<IndexViewModel>.Create(filteredAndOrderedProposals.AsQueryable(), pageNumber ?? 1, 6));
	}

	public async Task<IActionResult> Create(string id) {
		if (await this.CheckSession() is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		if (await this.CheckApplicationUser(id) is not ApplicationUser guideTeacher) {
			this.TempData["ErrorMessage"] = "Error al obtener al profesor guía.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		await this.Populate(guideTeacher);
		var createViewModel = new CreateViewModel {
			GuideTeacherId = guideTeacher.Id,
			GuideTeacherName = $"{guideTeacher.FirstName} {guideTeacher.LastName}",
		};
		return this.View(createViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Create([FromForm] CreateViewModel model) {
		if (await this.CheckSession() is not ApplicationUser student) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		if (await this.CheckApplicationUser(model.GuideTeacherId!) is not ApplicationUser guideTeacher) {
			this.TempData["ErrorMessage"] = "Revisa tu selección del profesor guía.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		if (!this.ModelState.IsValid) {
			await this.Populate(guideTeacher);
			this.ViewBag.WarningMessage = "Revisa que los campos estén correctos.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
				GuideTeacherId = model.GuideTeacherId,
				GuideTeacherName = model.GuideTeacherName,
				AssistantTeachers = model.AssistantTeachers
			});
		}
		var assistantTeachers = model.AssistantTeachers!.Select(async at => await this.CheckApplicationUser(at)).Select(at => at.Result).ToList();
		var proposal = new StudentProposal {
			Id = Guid.NewGuid().ToString(),
			Title = model.Title,
			Description = model.Description,
			StudentOwnerOfTheStudentProposal = student,
			GuideTeacherOfTheStudentProposal = guideTeacher,
			AssistantTeachersOfTheStudentProposal = assistantTeachers!,
			ProposalStatus = StudentProposal.Status.Draft,
			CreatedAt = DateTimeOffset.Now,
			UpdatedAt = DateTimeOffset.Now
		};
		_ = await this._dbContext.StudentProposals!.AddAsync(proposal);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "Tu propuesta ha sido registrada correctamente.";
		return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
	}

	public async Task<IActionResult> Edit(string id) {
		if (await this.CheckSession() is not ApplicationUser student) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.StudentProposals!.AsNoTracking()
			.Include(sp => sp.StudentOwnerOfTheStudentProposal).AsNoTracking()
			.Where(sp => sp.StudentOwnerOfTheStudentProposal == student && sp.ProposalStatus == StudentProposal.Status.Draft)
			.Include(sp => sp.GuideTeacherOfTheStudentProposal).AsNoTracking()
			.Include(sp => sp.AssistantTeachersOfTheStudentProposal).AsNoTracking()
			.FirstOrDefaultAsync(sp => sp.Id == id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		await this.Populate(proposal.GuideTeacherOfTheStudentProposal!);
		var editViewModel = new EditViewModel {
			Id = id,
			Title = proposal!.Title,
			Description = proposal.Description,
			GuideTeacherName = proposal.GuideTeacherOfTheStudentProposal!.Id,
			AssistantTeachers = proposal.AssistantTeachersOfTheStudentProposal!.Select(at => at!.Id).ToList(),
			CreatedAt = proposal.CreatedAt,
			UpdatedAt = proposal.UpdatedAt
		};
		return this.View(editViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit([FromForm] EditViewModel model) {
		if (await this.CheckSession() is not ApplicationUser student) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.StudentProposals!
			.Where(sp => sp.StudentOwnerOfTheStudentProposal == student && sp.ProposalStatus == StudentProposal.Status.Draft)
			.Include(sp => sp.StudentOwnerOfTheStudentProposal)
			.Include(sp => sp.GuideTeacherOfTheStudentProposal)
			.Include(sp => sp.AssistantTeachersOfTheStudentProposal)
			.FirstOrDefaultAsync(sp => sp.Id == model.Id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		if (!this.ModelState.IsValid) {
			await this.Populate(proposal.GuideTeacherOfTheStudentProposal!);
			this.ViewBag.WarningMessage = "Revisa que los campos estén correctos.";
			return this.View(new EditViewModel {
				Id = model.Id,
				Title = model.Title,
				Description = model.Description,
				GuideTeacherName = proposal.GuideTeacherOfTheStudentProposal!.Id,
				AssistantTeachers = proposal.AssistantTeachersOfTheStudentProposal!.Select(at => at!.Id).ToList(),
				CreatedAt = proposal.CreatedAt,
				UpdatedAt = proposal.UpdatedAt
			});
		}
		proposal.Title = model.Title;
		proposal.Description = model.Description;


		proposal.UpdatedAt = DateTimeOffset.Now;
		_ = this._dbContext.StudentProposals!.Update(proposal);
		_ = await this._dbContext.SaveChangesAsync();
		var editViewModel = new EditViewModel {
			Id = proposal.Id,
			Title = proposal!.Title,
			Description = proposal.Description,
			GuideTeacher = proposal.GuideTeacherOfTheStudentProposal!.Id,
			AssistantTeacher1 = proposal.AssistantTeacher1OfTheStudentProposal?.Id,
			AssistantTeacher2 = proposal.AssistantTeacher2OfTheStudentProposal?.Id,
			AssistantTeacher3 = proposal.AssistantTeacher3OfTheStudentProposal?.Id,
			CreatedAt = proposal.CreatedAt,
			UpdatedAt = proposal.UpdatedAt
		};
		this.ViewBag.SuccessMessage = "Tu propuesta ha sido actualizada correctamente.";
		return this.View(editViewModel);
	}

	public async Task<IActionResult> Delete(string id) {
		if (await this.CheckSession() is not ApplicationUser student) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.StudentProposals!.AsNoTracking()
			.Where(sp => sp.StudentOwnerOfTheStudentProposal == student && sp.ProposalStatus == StudentProposal.Status.Draft)
			.FirstOrDefaultAsync(sp => sp.Id == id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		var deleteViewModel = new DeleteViewModel {
			Id = id,
			Title = proposal.Title
		};
		return this.View(deleteViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete([FromForm] DeleteViewModel model) {
		if (await this.CheckSession() is not ApplicationUser student) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.StudentProposals!
			.Where(sp => sp.StudentOwnerOfTheStudentProposal == student && sp.ProposalStatus == StudentProposal.Status.Draft)
			.FirstOrDefaultAsync(sp => sp.Id == model.Id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		_ = this._dbContext.StudentProposals!.Remove(proposal);
		_ = this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "Tu propuesta ha sido eliminada correctamente.";
		return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
	}

	public async Task<IActionResult> Send(string id) {
		if (await this.CheckSession() is not ApplicationUser student) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.StudentProposals!.AsNoTracking()
			.Where(sp => sp.StudentOwnerOfTheStudentProposal == student && sp.ProposalStatus == StudentProposal.Status.Draft)
			.Include(sp => sp.GuideTeacherOfTheStudentProposal)
			.FirstOrDefaultAsync(sp => sp.Id == id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		var sendViewModel = new SendViewModel {
			Id = id,
			Title = proposal.Title,
			GuideTeacherName = $"{proposal.GuideTeacherOfTheStudentProposal!.FirstName} {proposal.GuideTeacherOfTheStudentProposal!.LastName}"
		};
		return this.View(sendViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Send([FromForm] SendViewModel model) {
		if (await this.CheckSession() is not ApplicationUser student) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.StudentProposals!
			.Where(sp => sp.StudentOwnerOfTheStudentProposal == student && sp.ProposalStatus == StudentProposal.Status.Draft)
			.Include(sp => sp.GuideTeacherOfTheStudentProposal)
			.Include(sp => sp.AssistantTeacher1OfTheStudentProposal)
			.Include(sp => sp.AssistantTeacher2OfTheStudentProposal)
			.Include(sp => sp.AssistantTeacher3OfTheStudentProposal)
			.FirstOrDefaultAsync(sp => sp.Id == model.Id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		var teacherCheck = (proposal.GuideTeacherOfTheStudentProposal, proposal.AssistantTeacher1OfTheStudentProposal, proposal.AssistantTeacher2OfTheStudentProposal, proposal.AssistantTeacher3OfTheStudentProposal) switch {
			(ApplicationUser teacher, _, _, _) when teacher.IsDeactivated => "El profesor guía está desactivado.",
			(_, ApplicationUser teacher, _, _) when teacher.IsDeactivated => "El profesor co-guía 1 está desactivado.",
			(_, _, ApplicationUser teacher, _) when teacher.IsDeactivated => "El profesor co-guía 2 está desactivado.",
			(_, _, _, ApplicationUser teacher) when teacher.IsDeactivated => "El profesor co-guía 3 está desactivado.",
			_ => null
		};
		if (teacherCheck is not null) {
			this.TempData["ErrorMessage"] = teacherCheck;
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		proposal.ProposalStatus = StudentProposal.Status.SentToGuideTeacher;
		proposal.UpdatedAt = DateTimeOffset.Now;
		_ = this._dbContext.StudentProposals!.Update(proposal);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "Tu propuesta ha sido enviada correctamente.";
		return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
	}

	public async Task<IActionResult> ViewRejectionReason(string id) {
		if (await this.CheckSession() is not ApplicationUser student) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.StudentProposals!.AsNoTracking()
			.Where(sp => sp.StudentOwnerOfTheStudentProposal == student && sp.ProposalStatus == StudentProposal.Status.RejectedByGuideTeacher)
			.Include(sp => sp.GuideTeacherOfTheStudentProposal)
			.FirstOrDefaultAsync(sp => sp.Id == id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		var viewRejectionReasonViewModel = new ViewRejectionReasonViewModel {
			Id = id,
			Title = proposal.Title,
			Description = proposal.Description,
			RejectedBy = $"{proposal.GuideTeacherOfTheStudentProposal!.FirstName} {proposal.GuideTeacherOfTheStudentProposal!.LastName}",
			Reason = proposal.RejectionReason,
			CreatedAt = proposal.CreatedAt,
			UpdatedAt = proposal.UpdatedAt
		};
		return this.View(viewRejectionReasonViewModel);
	}

	public async Task<IActionResult> Convert(string id) {
		if (await this.CheckSession() is not ApplicationUser student) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.StudentProposals!.AsNoTracking()
			.Where(sp => sp.StudentOwnerOfTheStudentProposal == student && sp.ProposalStatus == StudentProposal.Status.ApprovedByGuideTeacher)
			.Include(sp => sp.GuideTeacherOfTheStudentProposal).AsNoTracking()
			.Include(sp => sp.AssistantTeacher1OfTheStudentProposal).AsNoTracking()
			.Include(sp => sp.AssistantTeacher2OfTheStudentProposal).AsNoTracking()
			.Include(sp => sp.AssistantTeacher3OfTheStudentProposal).AsNoTracking()
			.FirstOrDefaultAsync(sp => sp.Id == id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		var convertViewModel = new ConvertViewModel {
			Id = id,
			Title = proposal.Title,
			Description = proposal.Description,
			GuideTeacherName = $"{proposal.GuideTeacherOfTheStudentProposal!.FirstName} {proposal.GuideTeacherOfTheStudentProposal!.LastName}",
			GuideTeacherEmail = proposal.GuideTeacherOfTheStudentProposal!.Email,
			GuideTeacherOffice = proposal.GuideTeacherOfTheStudentProposal!.TeacherOffice,
			GuideTeacherSchedule = proposal.GuideTeacherOfTheStudentProposal!.TeacherSchedule,
			GuideTeacherSpecialization = proposal.GuideTeacherOfTheStudentProposal!.TeacherSpecialization,
			AssistantTeacher1 = proposal.AssistantTeacher1OfTheStudentProposal is not null ? $"{proposal.AssistantTeacher1OfTheStudentProposal!.FirstName} {proposal.AssistantTeacher1OfTheStudentProposal!.LastName}" : "No asignado",
			AssistantTeacher2 = proposal.AssistantTeacher2OfTheStudentProposal is not null ? $"{proposal.AssistantTeacher2OfTheStudentProposal!.FirstName} {proposal.AssistantTeacher2OfTheStudentProposal!.LastName}" : "No asignado",
			AssistantTeacher3 = proposal.AssistantTeacher3OfTheStudentProposal is not null ? $"{proposal.AssistantTeacher3OfTheStudentProposal!.FirstName} {proposal.AssistantTeacher3OfTheStudentProposal!.LastName}" : "No asignado",
			CreatedAt = proposal.CreatedAt,
			UpdatedAt = proposal.UpdatedAt
		};
		return this.View(convertViewModel);
	}
}
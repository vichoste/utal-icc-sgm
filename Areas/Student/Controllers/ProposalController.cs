using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Utal.Icc.Sgm.Areas.Student.Views.Proposal;
using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;

using static Utal.Icc.Sgm.Models.ApplicationUser;

namespace Utal.Icc.Sgm.Areas.Student.Controllers;

[Area("Student"), Authorize(Roles = "Student")]
public class ProposalController : Controller {
	private readonly ApplicationDbContext _dbContext;
	private readonly UserManager<ApplicationUser> _userManager;

	public ProposalController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager) {
		this._dbContext = dbContext;
		this._userManager = userManager;
	}

	public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		this.ViewData["TitleSortParam"] = sortOrder == "Title" ? "TitleDesc" : "Title";
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		var student = this._dbContext.Users.AsNoTracking()
			.Include(s => s.StudentProposalsWhichIOwn).AsNoTracking()
			.FirstOrDefault(s => s.Id == this._userManager.GetUserId(this.User));
		if (student is null) {
			this.ViewBag.ErrorMessage = "Error al obtener al estudiante.";
			return this.View();
		}
		var orderedProposals = sortOrder switch {
			"Title" => student.StudentProposalsWhichIOwn!.OrderBy(sp => sp.Title),
			"TitleDesc" => student.StudentProposalsWhichIOwn!.OrderByDescending(sp => sp.Title),
			_ => student.StudentProposalsWhichIOwn!.OrderBy(sp => sp.Title)
		};
		var filteredAndOrderedProposals = orderedProposals.ToList();
		if (!string.IsNullOrEmpty(searchString)) {
			filteredAndOrderedProposals = orderedProposals
				.Where(sp => sp.Title!.ToUpper()
					.Contains(searchString.ToUpper()))
				.ToList();
		}
		var indexViewModels = filteredAndOrderedProposals.Select(sp => new IndexViewModel {
			Title = sp.Title,
			ProposalStatus = sp.ProposalStatus.ToString(),
		});
		var pageSize = 6;
		return this.View(PaginatedList<IndexViewModel>.Create((await this._userManager.GetUserAsync(this.User))!.Id, indexViewModels.AsQueryable(), pageNumber ?? 1, pageSize));
	}

	public async Task<IActionResult> Create() {
		var guideTeachers = (
			await this._userManager.GetUsersInRoleAsync(Roles.GuideTeacher.ToString()))
			.OrderBy(gt => gt.LastName)
			.ToList();
		var assistantTeachers1 = (
			await this._userManager.GetUsersInRoleAsync(Roles.AssistantTeacher.ToString()))
			.OrderBy(at => at.LastName)
			.ToList();
		var assistantTeachers2 = (
			await this._userManager.GetUsersInRoleAsync(Roles.AssistantTeacher.ToString()))
			.OrderBy(at => at.LastName)
			.ToList();
		var assistantTeachers3 = (
			await this._userManager.GetUsersInRoleAsync(Roles.AssistantTeacher.ToString()))
			.OrderBy(at => at.LastName)
			.ToList();
		this.ViewData["GuideTeachers"] = guideTeachers.Select(gt => new SelectListItem {
			Text = $"{gt.FirstName} {gt.LastName}",
			Value = gt.Id.ToString()
		});
		this.ViewData["AssistantTeachers1"] = assistantTeachers1.Select(gt => new SelectListItem {
			Text = $"{gt.FirstName} {gt.LastName}",
			Value = gt.Id.ToString()
		});
		this.ViewData["AssistantTeachers2"] = assistantTeachers2.Select(gt => new SelectListItem {
			Text = $"{gt.FirstName} {gt.LastName}",
			Value = gt.Id.ToString()
		});
		this.ViewData["AssistantTeachers3"] = assistantTeachers3.Select(gt => new SelectListItem {
			Text = $"{gt.FirstName} {gt.LastName}",
			Value = gt.Id.ToString()
		});
		return this.View(new CreateViewModel());
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Create([FromForm] CreateViewModel model) {
		if (!this.ModelState.IsValid) {
			this.ViewBag.WarningMessage = "Revisa que los campos estén correctos.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		var student = await this._userManager.GetUserAsync(this.User);
		var guideTeacher = await this._userManager.FindByIdAsync(model.GuideTeacher!.ToString());
		if (student is null || guideTeacher is null) {
			this.TempData["ErrorMessage"] = "Error al obtener a los involucrados.";
			return this.RedirectToAction("Index", "Proposal", new { area = "Student" });
		}
		ApplicationUser? assistantTeacher1 = null;
		ApplicationUser? assistantTeacher2 = null;
		ApplicationUser? assistantTeacher3 = null;
		if (!model.AssistantTeacher1.IsNullOrEmpty()) {
			assistantTeacher1 = await this._userManager.FindByIdAsync(model.AssistantTeacher1!.ToString());
			if (assistantTeacher1 is null) {
				this.TempData["ErrorMessage"] = "Error al obtener a los involucrados.";
				return this.RedirectToAction("Index", "Proposal", new { area = "Student" });
			}
		}
		if (!model.AssistantTeacher2.IsNullOrEmpty()) {
			assistantTeacher2 = await this._userManager.FindByIdAsync(model.AssistantTeacher2!.ToString());
			if (assistantTeacher2 is null) {
				this.TempData["ErrorMessage"] = "Error al obtener a los involucrados.";
				return this.RedirectToAction("Index", "Proposal", new { area = "Student" });
			}
		}
		if (!model.AssistantTeacher3.IsNullOrEmpty()) {
			assistantTeacher3 = await this._userManager.FindByIdAsync(model.AssistantTeacher3!.ToString());
			if (assistantTeacher3 is null) {
				this.TempData["ErrorMessage"] = "Error al obtener a los involucrados.";
				return this.RedirectToAction("Index", "Proposal", new { area = "Student" });
			}
		}
		if (assistantTeacher1 is not null && guideTeacher == assistantTeacher1) {
			this.ViewBag.WarningMessage = "El profesor guía no puede ser un profesor co-guía a la vez.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		if (assistantTeacher1 is not null && assistantTeacher2 is not null && assistantTeacher1 == assistantTeacher2) {
			this.ViewBag.WarningMessage = "El profesor co-guía no puede repetirse más de una vez.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		if (assistantTeacher1 is not null && assistantTeacher3 is not null && assistantTeacher1 == assistantTeacher3) {
			this.ViewBag.WarningMessage = "El profesor co-guía no puede repetirse más de una vez.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		if (assistantTeacher2 is not null && guideTeacher == assistantTeacher2) {
			this.ViewBag.WarningMessage = "El profesor guía no puede ser un profesor co-guía a la vez.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		if (assistantTeacher2 is not null && assistantTeacher1 is not null && assistantTeacher2 == assistantTeacher1) {
			this.ViewBag.WarningMessage = "El profesor co-guía no puede repetirse más de una vez.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		if (assistantTeacher2 is not null && assistantTeacher3 is not null && assistantTeacher2 == assistantTeacher3) {
			this.ViewBag.WarningMessage = "El profesor co-guía no puede repetirse más de una vez.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		if (assistantTeacher3 is not null && guideTeacher == assistantTeacher3) {
			this.ViewBag.WarningMessage = "El profesor guía no puede ser un profesor co-guía a la vez.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		if (assistantTeacher3 is not null && assistantTeacher1 is not null && assistantTeacher3 == assistantTeacher1) {
			this.ViewBag.WarningMessage = "El profesor co-guía no puede repetirse más de una vez.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		if (assistantTeacher3 is not null && assistantTeacher2 is not null && assistantTeacher3 == assistantTeacher2) {
			this.ViewBag.WarningMessage = "El profesor co-guía no puede repetirse más de una vez.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		var studentProposal = new StudentProposal {
			Title = model.Title,
			Description = model.Description,
			StudentOwnerOfTheStudentProposal = student,
			GuideTeacherOfTheStudentProposal = guideTeacher,
			AssistantTeacherOfTheStudentProposal1 = assistantTeacher1,
			AssistantTeacherOfTheStudentProposal2 = assistantTeacher2,
			AssistantTeacherOfTheStudentProposal3 = assistantTeacher3,
			ProposalStatus = StudentProposal.Status.Sent,
			CreatedAt = DateTimeOffset.Now,
			UpdatedAt = DateTimeOffset.Now
		};
		_ = await this._dbContext.StudentProposals.AddAsync(studentProposal);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "Tu propuesta ha sido registrada correctamente.";
		return this.RedirectToAction("Index", "Proposal", new { area = "Student" });
	}

	public async Task<IActionResult> Edit(string id) {
		var guideTeachers = (
			await this._userManager.GetUsersInRoleAsync(Roles.GuideTeacher.ToString()))
			.OrderBy(gt => gt.LastName)
			.ToList();
		var assistantTeachers1 = (
			await this._userManager.GetUsersInRoleAsync(Roles.AssistantTeacher.ToString()))
			.OrderBy(at => at.LastName)
			.ToList();
		var assistantTeachers2 = (
			await this._userManager.GetUsersInRoleAsync(Roles.AssistantTeacher.ToString()))
			.OrderBy(at => at.LastName)
			.ToList();
		var assistantTeachers3 = (
			await this._userManager.GetUsersInRoleAsync(Roles.AssistantTeacher.ToString()))
			.OrderBy(at => at.LastName)
			.ToList();
		this.ViewData["GuideTeachers"] = guideTeachers.Select(gt => new SelectListItem {
			Text = $"{gt.FirstName} {gt.LastName}",
			Value = gt.Id.ToString()
		});
		this.ViewData["AssistantTeachers1"] = assistantTeachers1.Select(gt => new SelectListItem {
			Text = $"{gt.FirstName} {gt.LastName}",
			Value = gt.Id.ToString()
		});
		this.ViewData["AssistantTeachers2"] = assistantTeachers2.Select(gt => new SelectListItem {
			Text = $"{gt.FirstName} {gt.LastName}",
			Value = gt.Id.ToString()
		});
		this.ViewData["AssistantTeachers3"] = assistantTeachers3.Select(gt => new SelectListItem {
			Text = $"{gt.FirstName} {gt.LastName}",
			Value = gt.Id.ToString()
		});
		var student = await this._userManager.GetUserAsync(this.User);
		if (student is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al estudiante.";
			return this.RedirectToAction("Index", "Proposal", new { area = "Student" });
		}
		var studentProposal = await this._dbContext.StudentProposals.AsNoTracking()
			.Include(sp => sp.StudentOwnerOfTheStudentProposal).AsNoTracking()
			.Where(sp => sp.StudentOwnerOfTheStudentProposal == student).AsNoTracking()
			.Include(sp => sp.GuideTeacherOfTheStudentProposal).AsNoTracking()
			.Include(sp => sp.AssistantTeacherOfTheStudentProposal1).AsNoTracking()
			.Include(sp => sp.AssistantTeacherOfTheStudentProposal2).AsNoTracking()
			.Include(sp => sp.AssistantTeacherOfTheStudentProposal3).AsNoTracking()
			.FirstOrDefaultAsync(sp => sp.Id.ToString() == id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction("Index", "Proposal", new { area = "Student" });
		}
		var editViewModel = new EditViewModel {
			Id = id,
			Title = studentProposal!.Title,
			Description = studentProposal.Description,
			GuideTeacher = studentProposal.GuideTeacherOfTheStudentProposal!.Id,
			AssistantTeacher1 = studentProposal.AssistantTeacherOfTheStudentProposal1!.Id,
			AssistantTeacher2 = studentProposal.AssistantTeacherOfTheStudentProposal2!.Id,
			AssistantTeacher3 = studentProposal.AssistantTeacherOfTheStudentProposal3!.Id,
		};
		return this.View(editViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit([FromForm] EditViewModel model) {
		if (!this.ModelState.IsValid) {
			this.ViewBag.WarningMessage = "Revisa que los campos estén correctos.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		var student = await this._userManager.GetUserAsync(this.User);
		var guideTeacher = await this._userManager.FindByIdAsync(model.GuideTeacher!.ToString());
		if (student is null || guideTeacher is null) {
			this.TempData["ErrorMessage"] = "Error al obtener a los involucrados.";
			return this.RedirectToAction("Index", "Proposal", new { area = "Student" });
		}
		var studentProposal = await this._dbContext.StudentProposals
			.Include(sp => sp.StudentOwnerOfTheStudentProposal)
			.Where(sp => sp.StudentOwnerOfTheStudentProposal == student)
			.Include(sp => sp.GuideTeacherOfTheStudentProposal)
			.Include(sp => sp.AssistantTeacherOfTheStudentProposal1)
			.Include(sp => sp.AssistantTeacherOfTheStudentProposal2)
			.Include(sp => sp.AssistantTeacherOfTheStudentProposal3)
			.FirstOrDefaultAsync(sp => sp.Id.ToString() == model.Id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction("Index", "Proposal", new { area = "Student" });
		}
		ApplicationUser? assistantTeacher1 = null;
		ApplicationUser? assistantTeacher2 = null;
		ApplicationUser? assistantTeacher3 = null;
		if (!model.AssistantTeacher1.IsNullOrEmpty()) {
			assistantTeacher1 = await this._userManager.FindByIdAsync(model.AssistantTeacher1!.ToString());
			if (assistantTeacher1 is null) {
				this.TempData["ErrorMessage"] = "Error al obtener a los involucrados.";
				return this.RedirectToAction("Index", "Proposal", new { area = "Student" });
			}
		}
		if (!model.AssistantTeacher2.IsNullOrEmpty()) {
			assistantTeacher2 = await this._userManager.FindByIdAsync(model.AssistantTeacher2!.ToString());
			if (assistantTeacher2 is null) {
				this.TempData["ErrorMessage"] = "Error al obtener a los involucrados.";
				return this.RedirectToAction("Index", "Proposal", new { area = "Student" });
			}
		}
		if (!model.AssistantTeacher3.IsNullOrEmpty()) {
			assistantTeacher3 = await this._userManager.FindByIdAsync(model.AssistantTeacher3!.ToString());
			if (assistantTeacher3 is null) {
				this.TempData["ErrorMessage"] = "Error al obtener a los involucrados.";
				return this.RedirectToAction("Index", "Proposal", new { area = "Student" });
			}
		}
		if (assistantTeacher1 is not null && guideTeacher == assistantTeacher1) {
			this.ViewBag.WarningMessage = "El profesor guía no puede ser un profesor co-guía a la vez.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		if (assistantTeacher1 is not null && assistantTeacher2 is not null && assistantTeacher1 == assistantTeacher2) {
			this.ViewBag.WarningMessage = "El profesor co-guía no puede repetirse más de una vez.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		if (assistantTeacher1 is not null && assistantTeacher3 is not null && assistantTeacher1 == assistantTeacher3) {
			this.ViewBag.WarningMessage = "El profesor co-guía no puede repetirse más de una vez.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		if (assistantTeacher2 is not null && guideTeacher == assistantTeacher2) {
			this.ViewBag.WarningMessage = "El profesor guía no puede ser un profesor co-guía a la vez.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		if (assistantTeacher2 is not null && assistantTeacher1 is not null && assistantTeacher2 == assistantTeacher1) {
			this.ViewBag.WarningMessage = "El profesor co-guía no puede repetirse más de una vez.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		if (assistantTeacher2 is not null && assistantTeacher3 is not null && assistantTeacher2 == assistantTeacher3) {
			this.ViewBag.WarningMessage = "El profesor co-guía no puede repetirse más de una vez.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		if (assistantTeacher3 is not null && guideTeacher == assistantTeacher3) {
			this.ViewBag.WarningMessage = "El profesor guía no puede ser un profesor co-guía a la vez.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		if (assistantTeacher3 is not null && assistantTeacher1 is not null && assistantTeacher3 == assistantTeacher1) {
			this.ViewBag.WarningMessage = "El profesor co-guía no puede repetirse más de una vez.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		if (assistantTeacher3 is not null && assistantTeacher2 is not null && assistantTeacher3 == assistantTeacher2) {
			this.ViewBag.WarningMessage = "El profesor co-guía no puede repetirse más de una vez.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		studentProposal.Title = model.Title;
		studentProposal.Description = model.Description;
		studentProposal.GuideTeacherOfTheStudentProposal = guideTeacher;
		studentProposal.AssistantTeacherOfTheStudentProposal1 = assistantTeacher1;
		studentProposal.AssistantTeacherOfTheStudentProposal2 = assistantTeacher2;
		studentProposal.AssistantTeacherOfTheStudentProposal3 = assistantTeacher3;
		studentProposal.UpdatedAt = DateTimeOffset.Now;
		_ = this._dbContext.StudentProposals.Update(studentProposal);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "Tu propuesta ha sido actualizada correctamente.";
		return this.View(model);
	}

	public async Task<IActionResult> Delete(string id) {
		var student = await this._userManager.GetUserAsync(this.User);
		if (student is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al estudiante.";
			return this.RedirectToAction("Index", "Proposal", new { area = "Student" });
		}
		var studentProposal = await this._dbContext.StudentProposals.AsNoTracking()
			.FirstOrDefaultAsync(sp => sp.Id.ToString() == id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction("Index", "Proposal", new { area = "Student" });
		}
		
	}
}
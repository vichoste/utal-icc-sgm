using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Utal.Icc.Sgm.Areas.Student.Views.Proposal;
using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Areas.Student.Controllers;

[Area("Student"), Authorize(Roles = "Student")]
public class ProposalController : Controller {
	private readonly ApplicationDbContext _context;
	private readonly UserManager<ApplicationUser> _userManager;

	public ProposalController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) {
		this._context = context;
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
		var student = this._context.Users.AsNoTracking().Include(s => s.StudentProposalsWhichIOwn).AsNoTracking().FirstOrDefault(s => s.Id == this._userManager.GetUserId(this.User));
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
			filteredAndOrderedProposals = orderedProposals.Where(sp => sp.Title!.ToUpper().Contains(searchString.ToUpper())).ToList();
		}
		var indexViewModels = filteredAndOrderedProposals.Select(sp => new IndexViewModel {
			Title = sp.Title,
			IsDraft = sp.IsDraft,
			IsPending = sp.IsPending,
			IsAccepted = sp.IsAccepted,
		});
		var pageSize = 6;
		return this.View(PaginatedList<IndexViewModel>.Create(indexViewModels.AsQueryable(), pageNumber ?? 1, pageSize));
	}

	public async Task<IActionResult> Create() {
		var guideTeachers = (await this._userManager.GetUsersInRoleAsync(Roles.GuideTeacher.ToString())).OrderBy(gt => gt.LastName).ToList();
		var assistantTeachers1 = (await this._userManager.GetUsersInRoleAsync(Roles.AssistantTeacher.ToString())).OrderBy(at => at.LastName).ToList();
		var assistantTeachers2 = (await this._userManager.GetUsersInRoleAsync(Roles.AssistantTeacher.ToString())).OrderBy(at => at.LastName).ToList();
		var assistantTeachers3 = (await this._userManager.GetUsersInRoleAsync(Roles.AssistantTeacher.ToString())).OrderBy(at => at.LastName).ToList();
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
		var guideTeachers = (await this._userManager.GetUsersInRoleAsync(Roles.GuideTeacher.ToString())).OrderBy(gt => gt.LastName).ToList();
		var assistantTeachers1 = (await this._userManager.GetUsersInRoleAsync(Roles.AssistantTeacher.ToString())).OrderBy(at => at.LastName).ToList();
		var assistantTeachers2 = (await this._userManager.GetUsersInRoleAsync(Roles.AssistantTeacher.ToString())).OrderBy(at => at.LastName).ToList();
		var assistantTeachers3 = (await this._userManager.GetUsersInRoleAsync(Roles.AssistantTeacher.ToString())).OrderBy(at => at.LastName).ToList();
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
		if (!this.ModelState.IsValid) {
			this.ViewBag.WarningMessage = "Revisa que los campos estén correctos.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		var student = await this._userManager.GetUserAsync(this.User);
		if (student is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al estudiante.";
			return this.RedirectToAction("Index", "Proposal", new { area = "Student" });
		}
		var guideTeacher = await this._userManager.FindByIdAsync(model.GuideTeacher!.ToString());
		if (guideTeacher is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al profesor guía.";
			return this.RedirectToAction("Index", "Proposal", new { area = "Student " });
		}
		ApplicationUser? assistantTeacher1 = null;
		ApplicationUser? assistantTeacher2 = null;
		ApplicationUser? assistantTeacher3 = null;
		if (!model.AssistantTeacher1.IsNullOrEmpty()) {
			assistantTeacher1 = await this._userManager.FindByIdAsync(model.AssistantTeacher1!.ToString());
		}
		if (!model.AssistantTeacher2.IsNullOrEmpty()) {
			assistantTeacher2 = await this._userManager.FindByIdAsync(model.AssistantTeacher2!.ToString());
		}
		if (!model.AssistantTeacher3.IsNullOrEmpty()) {
			assistantTeacher3 = await this._userManager.FindByIdAsync(model.AssistantTeacher3!.ToString());
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
			GuideTeacherCandidateOfTheStudentProposal = guideTeacher,
			AssistantTeachersCandidatesOfTheStudentProposal = new List<ApplicationUser>(),
			IsDraft = true,
			IsPending = true,
			IsAccepted = false,
		};
		if (assistantTeacher1 is not null) {
			studentProposal.AssistantTeachersCandidatesOfTheStudentProposal.Add(assistantTeacher1);
		}
		if (assistantTeacher2 is not null) {
			studentProposal.AssistantTeachersCandidatesOfTheStudentProposal.Add(assistantTeacher2);
		}
		if (assistantTeacher3 is not null) {
			studentProposal.AssistantTeachersCandidatesOfTheStudentProposal.Add(assistantTeacher3);
		}
		_ = await this._context.StudentProposals.AddAsync(studentProposal);
		_ = await this._context.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "Propuesta de estudiante agregada con éxito.";
		return this.RedirectToAction("Index", "Proposal", new { area = "Student" });
	}
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;
using Utal.Icc.Sgm.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Utal.Icc.Sgm.Areas.University.Controllers;

[Area("University"), Authorize]
public class MemoirController : Controller {
	private readonly ApplicationDbContext _dbContext;
	private readonly UserManager<ApplicationUser> _userManager;
	public MemoirController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager) {
		this._dbContext = dbContext;
		this._userManager = userManager;
	}

	private async Task PopulateAssistantTeachers(ApplicationUser guideTeacher) {
		var assistantTeachers = (
			await this._userManager.GetUsersInRoleAsync(nameof(Role.Assistant)))
				.Where(at => at != guideTeacher && !at.IsDeactivated)
				.OrderBy(at => at.LastName)
				.ToList();
		this.ViewData[$"{nameof(Role.Assistant)}s"] = assistantTeachers.Select(at => new SelectListItem {
			Text = $"{at.FirstName} {at.LastName}",
			Value = at.Id
		});
	}

	[Authorize(Roles = "Student")]
	public async Task<IActionResult> Guide(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		var parameters = new[] { "FirstName", "LastName", "Email", "Specialization" };
		foreach (var parameter in parameters) {
			this.ViewData[$"{parameter}SortParam"] = sortOrder == parameter ? $"{parameter}Desc" : parameter;
		}
		this.ViewData["CurrentSort"] = sortOrder;
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		var users = await this._userManager.GetUsersInRoleAsync("Guide");
		var paginator = Paginator<ApplicationViewModel>.Create(users.Select(u => new ApplicationUserViewModel {
			Id = u.Id,
			FirstName = u.FirstName,
			LastName = u.LastName,
			Rut = u.Rut,
			Email = u.Email,
			Specialization = u.Specialization,
		}).AsQueryable(), pageNumber ?? 1, 10);
		if (!string.IsNullOrEmpty(sortOrder)) {
			paginator.Sort(sortOrder);
		}
		if (!string.IsNullOrEmpty(currentFilter)) {
			paginator.Filter(currentFilter);
		}
		return this.View(paginator);
	}

	#region My proposal
	[Authorize(Roles = "Student,Guide")]
	public IActionResult MyProposal(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		string[]? parameters = null;
		if (this.User.IsInRole("Student")) {
			parameters = new[] { "Title", "GuideName" };
		} else if (this.User.IsInRole("Guide")) {
			parameters = new[] { "Title", "MemoristName" };
		}
		foreach (var parameter in parameters!) {
			this.ViewData[$"{parameter}SortParam"] = sortOrder == parameter ? $"{parameter}Desc" : parameter;
		}
		this.ViewData["CurrentSort"] = sortOrder;
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		IQueryable<MemoirViewModel> memoirs = null!;
		if (this.User.IsInRole("Student")) {
			memoirs = this._dbContext.Memoirs!
				.Include(m => m.Memorist)
				.Where(m => m.Memorist!.Id == this._userManager.GetUserId(this.User))
				.Include(m => m.Guide).AsNoTracking()
				.Select(m => new MemoirViewModel {
					Id = m.Id,
					Title = m.Title,
					Phase = m.Phase.ToString(),
					GuideName = $"{m.Guide!.FirstName} {m.Guide!.LastName}"
				});
		} else if (this.User.IsInRole("Guide")) {
			memoirs = this._dbContext.Memoirs!
				.Include(m => m.Guide)
				.Where(m => m.Guide!.Id == this._userManager.GetUserId(this.User))
				.Include(m => m.Memorist).AsNoTracking()
				.Select(m => new MemoirViewModel {
					Id = m.Id,
					Title = m.Title,
					Phase = m.Phase.ToString(),
					MemoristName = $"{m.Memorist!.FirstName} {m.Memorist!.LastName}"
				});
		}
		var paginator = Paginator<MemoirViewModel>.Create(memoirs, pageNumber ?? 1, 6);
		if (!string.IsNullOrEmpty(sortOrder)) {
			paginator.Sort(sortOrder);
		}
		if (!string.IsNullOrEmpty(currentFilter)) {
			paginator.Filter(currentFilter);
		}
		return this.View(paginator);
	}

	[Authorize(Roles = "Guide")]
	public async Task<IActionResult> CreateMyProposal() {
		var guideTeacher = await this._userManager.GetUserAsync(this.User);
		await this.PopulateAssistantTeachers(guideTeacher!);
		return this.View(new MemoirViewModel());
	}

	[Authorize(Roles = "Student")]
	public async Task<IActionResult> CreateMyProposal(string id) {
		var guideTeacher = await this._userManager.FindByIdAsync(id);
		if (guideTeacher is null) {
			this.TempData["ErrorMessage"] = "Error al registrar tu propuesta.";
			return this.RedirectToAction("Guide", "MemoirController", new { area = "University" });
		}
		await this.PopulateAssistantTeachers(guideTeacher);
		var output = new MemoirViewModel {
			GuideId = guideTeacher.Id,
			GuideName = $"{guideTeacher.FirstName} {guideTeacher.LastName}",
			GuideEmail = guideTeacher.Email,
			Office = guideTeacher.Office,
			Schedule = guideTeacher.Schedule,
			Specialization = guideTeacher.Specialization,
		};
		return this.View(output);
	}

	[Authorize(Roles = "Student,Guide"), HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> CreateMyProposal([FromForm] MemoirViewModel input) {
		ApplicationUser? guide = null!;
		if (this.User.IsInRole("Student")) {
			guide = (await this._userManager.FindByIdAsync(input.GuideId!))!;
			if (guide is null) {
				this.ViewData["ErrorMessage"] = "Error al registrar tu propuesta.";
				return this.RedirectToAction("Guide", "MemoirController", new { area = "University" });
			}
		} else if (this.User.IsInRole("Guide")) {
			guide = (await this._userManager.GetUserAsync(this.User))!;
		}
		var assistantTeachers = input.AssistantTeachers!.Select(async at => await this._userManager.FindByIdAsync(at!)).Select(at => at.Result).Where(u => !u!.IsDeactivated).ToList();
		var memoir = new Memoir {
			Id = Guid.NewGuid().ToString(),
			Title = input.Title,
			Description = input.Description,
			Phase = Phase.Draft,
			Guide = guide,
			Assistants = assistantTeachers,
			CreatedAt = DateTimeOffset.Now,
			UpdatedAt = DateTimeOffset.Now,
		};
		if (this.User.IsInRole("Student")) {
			memoir.Memorist = await this._userManager.GetUserAsync(this.User);
		} else if (this.User.IsInRole("Guide")) {
			memoir.Requirements = input.Requirements;
		}
		_ = await this._dbContext.Memoirs!.AddAsync(memoir);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "Tu propouesta ha sido registrada correctamente.";
		return this.RedirectToAction("MyProposal", "Memoir", new { area = "University" });
	}
	#endregion
}
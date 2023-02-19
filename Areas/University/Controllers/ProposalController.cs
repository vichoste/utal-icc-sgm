using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;
using Utal.Icc.Sgm.ViewModels;

namespace Utal.Icc.Sgm.Areas.University.Controllers;

[Area("University"), Authorize]
public class ProposalController : Controller {
	private readonly ApplicationDbContext _dbContext;
	private readonly UserManager<ApplicationUser> _userManager;

	public ProposalController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager) {
		this._dbContext = dbContext;
		this._userManager = userManager;
	}

	private async Task PopulateAssistants(ApplicationUser guide) {
		var assistants = (
			await this._userManager.GetUsersInRoleAsync("Assistant"))
				.Where(u => u != guide && !u.IsDeactivated)
				.OrderBy(u => u.LastName)
				.ToList();
		this.ViewData[$"Assistants"] = assistants.Select(u => new SelectListItem {
			Text = $"{u.FirstName} {u.LastName}",
			Value = u.Id
		});
	}

	[Authorize(Roles = "Student")]
	public async Task<IActionResult> Guides(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
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
		var paginator = Paginator<ApplicationViewModel>.Create(users.Where(u => !u.IsDeactivated).Select(u => new ApplicationUserViewModel {
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

	[Authorize(Roles = "Guide")]
	public IActionResult Students(string id, string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		var memoir = this._dbContext.Memoirs!.Include(m => m.Candidates).AsNoTracking().FirstOrDefault(p => p.Id == id && p.Phase == Phase.PublishedByGuide);
		this.ViewData["Memoir"] = memoir;
		var parameters = new[] { "FirstName", "LastName", "Email" };
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
		var candidates = memoir!.Candidates!.AsEnumerable();
		var paginator = Paginator<ApplicationViewModel>.Create(candidates.Where(u => !u!.IsDeactivated).Select(u => new ApplicationUserViewModel {
			Id = u!.Id,
			FirstName = u.FirstName,
			LastName = u.LastName,
			Email = u.Email,
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
	public IActionResult Index(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
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
	public async Task<IActionResult> Create() {
		var guideTeacher = await this._userManager.GetUserAsync(this.User);
		await this.PopulateAssistants(guideTeacher!);
		return this.View(new MemoirViewModel());
	}

	[Authorize(Roles = "Student")]
	public async Task<IActionResult> Create(string id) {
		var guideTeacher = await this._userManager.FindByIdAsync(id);
		if (guideTeacher is null) {
			this.TempData["ErrorMessage"] = "El profesor guía no existe.";
			return this.RedirectToAction("Guide", "Proposal", new { area = "University" });
		}
		if (guideTeacher.IsDeactivated) {
			this.TempData["ErrorMessage"] = "El profesor guía está desactivado.";
			return this.RedirectToAction("Guide", "Proposal", new { area = "University" });
		}
		await this.PopulateAssistants(guideTeacher);
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
	public async Task<IActionResult> Create([FromForm] MemoirViewModel input) {
		ApplicationUser? guide = null!;
		if (this.User.IsInRole("Student")) {
			guide = (await this._userManager.FindByIdAsync(input.GuideId!))!;
			if (guide is null) {
				this.ViewData["ErrorMessage"] = "El profesor guía no existe.";
				return this.RedirectToAction("Guide", "Proposal", new { area = "University" });
			}
			if (guide.IsDeactivated) {
				this.ViewData["ErrorMessage"] = "El profesor guía está desactivado.";
				return this.RedirectToAction("Guide", "Proposal", new { area = "University" });
			}
		} else if (this.User.IsInRole("Guide")) {
			guide = (await this._userManager.GetUserAsync(this.User))!;
		}
		var assistants = input.Assistants!.Select(async a => await this._userManager.FindByIdAsync(a!)).Select(a => a.Result).Where(u => !u!.IsDeactivated).ToList();
		var memoir = new Memoir {
			Id = Guid.NewGuid().ToString(),
			Title = input.Title,
			Description = input.Description,
			Phase = Phase.Draft,
			Guide = guide,
			Assistants = assistants,
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
		return this.RedirectToAction("Index", "Proposal", new { area = "University" });
	}

	[Authorize(Roles = "Student,Guide")]
	public async Task<IActionResult> Edit(string id) {
		Memoir? memoir = null!;
		if (this.User.IsInRole("Student")) {
			memoir = await this._dbContext.Memoirs!
				.Include(m => m.Memorist)
				.Where(m => m.Memorist!.Id == this._userManager.GetUserId(this.User))
				.Include(m => m.Guide)
				.Include(m => m.Assistants).AsNoTracking()
				.FirstOrDefaultAsync(m => m.Id == id && (m.Phase == Phase.Draft || m.Phase == Phase.RejectedByGuide || m.Phase == Phase.RejectedByCommittee));
		} else if (this.User.IsInRole("Guide")) {
			memoir = await this._dbContext.Memoirs!
				.Include(m => m.Guide)
				.Where(m => m.Guide!.Id == this._userManager.GetUserId(this.User))
				.Include(m => m.Assistants).AsNoTracking()
				.FirstOrDefaultAsync(m => m.Id == id && (m.Phase == Phase.Draft || m.Phase == Phase.RejectedByGuide || m.Phase == Phase.RejectedByCommittee));
		}
		if (memoir is null) {
			this.TempData["ErrorMessage"] = "La propuesta no existe.";
			return this.RedirectToAction("Index", "Proposal", new { area = "University" });
		}
		if (memoir.Guide!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "El profesor guía está desactivado.";
			return this.RedirectToAction("Index", "Proposal", new { area = "University" });
		}
		await this.PopulateAssistants(memoir.Guide!);
		MemoirViewModel? output = null!;
		if (this.User.IsInRole("Student")) {
			output = new MemoirViewModel {
				Id = id,
				Title = memoir.Title,
				Description = memoir.Description,
				GuideId = memoir.Guide!.Id,
				GuideName = $"{memoir.Guide!.FirstName} {memoir.Guide!.LastName}",
				GuideEmail = memoir.Guide!.Email,
				Office = memoir.Guide!.Office,
				Schedule = memoir.Guide!.Schedule,
				Specialization = memoir.Guide!.Specialization,
				Assistants = memoir.Assistants!.Select(a => a!.Id).ToList(),
				CreatedAt = memoir.CreatedAt,
				UpdatedAt = memoir.UpdatedAt
			};
		} else if (this.User.IsInRole("Guide")) {
			output = new MemoirViewModel {
				Id = id,
				Title = memoir.Title,
				Description = memoir.Description,
				Requirements = memoir.Requirements,
				Assistants = memoir.Assistants!.Select(a => a!.Id).ToList(),
				CreatedAt = memoir.CreatedAt,
				UpdatedAt = memoir.UpdatedAt
			};
		}
		return this.View(output);
	}

	[Authorize(Roles = "Student,Guide"), HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit([FromForm] MemoirViewModel input) {
		Memoir? memoir = null!;
		if (this.User.IsInRole("Student")) {
			memoir = await this._dbContext.Memoirs!
				.Include(m => m.Memorist)
				.Where(m => m.Memorist!.Id == this._userManager.GetUserId(this.User))
				.Include(m => m.Guide)
				.Include(m => m.Assistants)
				.FirstOrDefaultAsync(m => m.Id == input.Id && (m.Phase == Phase.Draft || m.Phase == Phase.RejectedByGuide || m.Phase == Phase.RejectedByCommittee));
		} else if (this.User.IsInRole("Guide")) {
			memoir = await this._dbContext.Memoirs!
				.Include(m => m.Guide)
				.Where(m => m.Guide!.Id == this._userManager.GetUserId(this.User))
				.Include(m => m.Assistants)
				.FirstOrDefaultAsync(m => m.Id == input.Id && (m.Phase == Phase.Draft || m.Phase == Phase.RejectedByGuide || m.Phase == Phase.RejectedByCommittee));
		}
		if (memoir is null) {
			this.TempData["ErrorMessage"] = "La propuesta no existe.";
			return this.RedirectToAction("Index", "Proposal", new { area = "University" });
		}
		if (memoir.Guide!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "El profesor guía está desactivado.";
			return this.RedirectToAction("Index", "Proposal", new { area = "University" });
		}
		var assistants = input.Assistants!.Select(async a => await this._userManager.FindByIdAsync(a!)).Select(a => a.Result).Where(u => !u!.IsDeactivated).ToList();
		MemoirViewModel? output = null!;
		var canEdit = true;
		foreach (var assistant in assistants) {
			if (assistant!.IsDeactivated) {
				this.ViewBag.WarningMessage = $"El profesor co-guía {assistant.FirstName} {assistant.LastName} está desactivado.";
				canEdit = false;
				break;
			}
		}
		if (canEdit) {
			memoir.Title = input.Title;
			memoir.Description = input.Description;
			if (this.User.IsInRole("Guide")) {
				memoir.Requirements = input.Requirements;
			}
			memoir.Assistants = assistants;
			memoir.UpdatedAt = DateTimeOffset.Now;
			_ = this._dbContext.Memoirs!.Update(memoir);
			_ = await this._dbContext.SaveChangesAsync();
		}
		await this.PopulateAssistants(memoir.Guide!);
		if (this.User.IsInRole("Student")) {
			output = new MemoirViewModel {
				Id = memoir.Id,
				Title = memoir.Title,
				Description = memoir.Description,
				GuideId = memoir.Guide!.Id,
				GuideName = $"{memoir.Guide!.FirstName} {memoir.Guide!.LastName}",
				GuideEmail = memoir.Guide!.Email,
				Office = memoir.Guide!.Office,
				Schedule = memoir.Guide!.Schedule,
				Specialization = memoir.Guide!.Specialization,
				Assistants = memoir.Assistants!.Select(a => a!.Id).ToList(),
				CreatedAt = memoir.CreatedAt,
				UpdatedAt = memoir.UpdatedAt
			};
		} else if (this.User.IsInRole("Guide")) {
			output = new MemoirViewModel {
				Id = memoir.Id,
				Title = memoir.Title,
				Description = memoir.Description,
				Requirements = memoir.Requirements,
				Assistants = memoir.Assistants!.Select(a => a!.Id).ToList(),
				CreatedAt = memoir.CreatedAt,
				UpdatedAt = memoir.UpdatedAt
			};
		}
		this.ViewBag.SuccessMessage = "Tu propuesta ha sido editada correctamente.";
		return this.View(output);
	}

	[Authorize(Roles = "Student,Guide"), HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete(string id) {
		Memoir? memoir = null!;
		if (this.User.IsInRole("Student")) {
			memoir = await this._dbContext.Memoirs!
				.Include(m => m.Memorist)
				.Where(m => m.Memorist!.Id == this._userManager.GetUserId(this.User)).AsNoTracking()
				.FirstOrDefaultAsync(m => m.Id == id && m.Phase == Phase.Draft);
		} else if (this.User.IsInRole("Guide")) {
			memoir = await this._dbContext.Memoirs!
				.Include(m => m.Guide)
				.Where(m => m.Guide!.Id == this._userManager.GetUserId(this.User)).AsNoTracking()
				.FirstOrDefaultAsync(m => m.Id == id && m.Phase == Phase.Draft);
		}
		if (memoir is null) {
			this.TempData["ErrorMessage"] = "La propuesta no existe.";
			return this.RedirectToAction("Index", "Proposal", new { area = "University" });
		}
		var output = new MemoirViewModel {
			Id = id,
			Title = memoir.Title,
		};
		return this.View(output);
	}

	[Authorize(Roles = "Student,Guide"), HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete([FromForm] MemoirViewModel input) {
		Memoir? memoir = null!;
		if (this.User.IsInRole("Student")) {
			memoir = await this._dbContext.Memoirs!
				.Include(m => m.Memorist)
				.Where(m => m.Memorist!.Id == this._userManager.GetUserId(this.User))
				.FirstOrDefaultAsync(m => m.Id == input.Id && m.Phase == Phase.Draft);
		} else if (this.User.IsInRole("Guide")) {
			memoir = await this._dbContext.Memoirs!
				.Include(m => m.Guide)
				.Where(m => m.Guide!.Id == this._userManager.GetUserId(this.User))
				.FirstOrDefaultAsync(m => m.Id == input.Id && m.Phase == Phase.Draft);
		}
		if (memoir is null) {
			this.TempData["ErrorMessage"] = "La propuesta no existe.";
			return this.RedirectToAction("Index", "Proposal", new { area = "University" });
		}
		_ = this._dbContext.Memoirs!.Remove(memoir);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "Tu propuesta ha sido eliminada correctamente.";
		return this.RedirectToAction("Index", "Proposal", new { area = "University" });
	}

	[Authorize(Roles = "Student,Guide")]
	public async Task<IActionResult> Send(string id) {
		Memoir? memoir = null!;
		if (this.User.IsInRole("Student")) {
			memoir = await this._dbContext.Memoirs!
				.Include(m => m.Memorist)
				.Where(m => m.Memorist!.Id == this._userManager.GetUserId(this.User))
				.Include(m => m.Guide).AsNoTracking()
				.FirstOrDefaultAsync(m => m.Id == id && m.Phase == Phase.Draft);
		} else if (this.User.IsInRole("Guide")) {
			memoir = await this._dbContext.Memoirs!
				.Include(m => m.Guide)
				.Where(m => m.Guide!.Id == this._userManager.GetUserId(this.User)).AsNoTracking()
				.FirstOrDefaultAsync(m => m.Id == id && m.Phase == Phase.Draft);
		}
		if (memoir is null) {
			this.TempData["ErrorMessage"] = "La propuesta no existe.";
			return this.RedirectToAction("Index", "Proposal", new { area = "University" });
		}
		if (memoir.Guide!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "El profesor guía está desactivado.";
			return this.RedirectToAction("Index", "Proposal", new { area = "University" });
		}
		MemoirViewModel? output = null!;
		if (this.User.IsInRole("Student")) {
			output = new MemoirViewModel {
				Id = id,
				Title = memoir.Title,
				GuideName = $"{memoir.Guide!.FirstName} {memoir.Guide!.LastName}",
			};
		} else if (this.User.IsInRole("Guide")) {
			output = new MemoirViewModel {
				Id = id,
				Title = memoir.Title
			};
		}
		return this.View(output);
	}

	[Authorize(Roles = "Student,Guide"), HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Send([FromForm] MemoirViewModel input) {
		Memoir? memoir = null!;
		if (this.User.IsInRole("Student")) {
			memoir = await this._dbContext.Memoirs!
				.Include(m => m.Memorist)
				.Where(m => m.Memorist!.Id == this._userManager.GetUserId(this.User))
				.FirstOrDefaultAsync(m => m.Id == input.Id && m.Phase == Phase.Draft);
		} else if (this.User.IsInRole("Guide")) {
			memoir = await this._dbContext.Memoirs!
				.Include(m => m.Guide)
				.Where(m => m.Guide!.Id == this._userManager.GetUserId(this.User))
				.FirstOrDefaultAsync(m => m.Id == input.Id && m.Phase == Phase.Draft);
		}
		if (memoir is null) {
			this.TempData["ErrorMessage"] = "La propuesta no existe.";
			return this.RedirectToAction("Index", "Proposal", new { area = "University" });
		}
		if (memoir.Guide!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "El profesor guía está desactivado.";
			return this.RedirectToAction("Index", "Proposal", new { area = "University" });
		}
		if (this.User.IsInRole("Student")) {
			memoir.Phase = Phase.SentToGuide;
		} else if (this.User.IsInRole("Guide")) {
			memoir.Phase = Phase.PublishedByGuide;
		}
		memoir.UpdatedAt = DateTimeOffset.Now;
		_ = this._dbContext.Memoirs!.Update(memoir);
		_ = await this._dbContext.SaveChangesAsync();
		if (this.User.IsInRole("Student")) {
			this.TempData["SuccessMessage"] = "Tu propuesta ha sido enviada correctamente.";
		} else if (this.User.IsInRole("Guide")) {
			this.TempData["SuccessMessage"] = "Tu propuesta ha sido publicada correctamente.";
		}
		return this.RedirectToAction("Index", "Proposal", new { area = "University" });
	}

	[Authorize(Roles = "Student,Guide")]
	public new async Task<IActionResult> View(string id) {
		Memoir? memoir = null!;
		if (this.User.IsInRole("Student")) {
			memoir = await this._dbContext.Memoirs!
				.Include(m => m.Memorist)
				.Where(m => m.Memorist!.Id == this._userManager.GetUserId(this.User))
				.Include(m => m.Guide)
				.Include(m => m.Assistants).AsNoTracking()
				.FirstOrDefaultAsync(m => m.Id == id && m.Phase == Phase.SentToGuide);
		} else if (this.User.IsInRole("Guide")) {
			memoir = await this._dbContext.Memoirs!
				.Include(m => m.Guide)
				.Where(m => m.Guide!.Id == this._userManager.GetUserId(this.User))
				.Include(m => m.Memorist)
				.Include(m => m.Assistants).AsNoTracking()
				.FirstOrDefaultAsync(m => m.Id == id && m.Phase == Phase.PublishedByGuide);
		}
		if (memoir is null) {
			this.TempData["ErrorMessage"] = "La propuesta no existe.";
			return this.RedirectToAction("Index", "Proposal", new { area = "University" });
		}
		MemoirViewModel? output = null!;
		if (this.User.IsInRole("Student")) {
			output = new MemoirViewModel {
				Id = id,
				Title = memoir.Title,
				Description = memoir.Description,
				GuideId = memoir.Guide!.Id,
				GuideName = $"{memoir.Guide.FirstName} {memoir.Guide.LastName}",
				GuideEmail = memoir.Guide.Email,
				Office = memoir.Guide.Office,
				Schedule = memoir.Guide.Schedule,
				Specialization = memoir.Guide.Specialization,
				Assistants = memoir.Assistants!.Select(a => $"{a!.FirstName} {a.LastName}").ToList(),
				CreatedAt = memoir.CreatedAt,
				UpdatedAt = memoir.UpdatedAt,
				WhoRejected = memoir.WhoRejected is null ? "Memoria aún no rechazada" : $"{memoir.WhoRejected.FirstName} {memoir.WhoRejected.LastName}",
				Reason = memoir.Reason
			};
		} else if (this.User.IsInRole("Guide")) {
			output = new MemoirViewModel {
				Id = id,
				Title = memoir.Title,
				Description = memoir.Description,
				Requirements = memoir.Requirements,
				MemoristId = memoir.Memorist is null ? string.Empty : memoir.Memorist.Id,
				MemoristName = memoir.Memorist is null ? "Sin memorista asignado" : $"{memoir.Memorist.FirstName} {memoir.Memorist.LastName}",
				MemoristEmail = memoir.Memorist is null ? "Sin memorista asignado" : memoir.Memorist.Email,
				UniversityId = memoir.Memorist is null ? "Sin memorista asignado" : memoir.Memorist.UniversityId,
				RemainingCourses = memoir.Memorist is null ? "Sin memorista asignado" : memoir.Memorist.RemainingCourses,
				IsDoingThePractice = memoir.Memorist is not null && memoir.Memorist.IsDoingThePractice,
				IsWorking = memoir.Memorist is not null && memoir.Memorist.IsWorking,
				Assistants = memoir.Assistants!.Select(a => $"{a!.FirstName} {a.LastName}").ToList(),
				CreatedAt = memoir.CreatedAt,
				UpdatedAt = memoir.UpdatedAt,
				WhoRejected = memoir.WhoRejected is null ? "Memoria aún no rechazada" : $"{memoir.WhoRejected.FirstName} {memoir.WhoRejected.LastName}",
				Reason = memoir.Reason
			};
		}
		return this.View(output);
	}

	public async Task<IActionResult> Select(string proposalId, string studentId) {

	}
	#endregion
}
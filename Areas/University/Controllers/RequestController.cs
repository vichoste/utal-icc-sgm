using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;
using Utal.Icc.Sgm.ViewModels;

namespace Utal.Icc.Sgm.Areas.University.Controllers;

[Area("University"), Authorize]
public class RequestController : Controller {
	private readonly ApplicationDbContext _dbContext;
	private readonly UserManager<ApplicationUser> _userManager;

	public RequestController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager) {
		this._dbContext = dbContext;
		this._userManager = userManager;
	}

	[Authorize(Roles = "Student,Guide,Committee,Director")]
	public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "Tu cuenta está desactivada.";
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var parameters = new[] { "Title", "MemoristName", "GuideName" };
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
		if (this.User.IsInRole("Student") || this.User.IsInRole("Guide")) {
			memoirs = this._dbContext.Memoirs!
				.Include(m => m.Owner)
				.Where(m => m.Owner!.Id == this._userManager.GetUserId(this.User)
					&& (m.Phase == Phase.SentToCommittee || m.Phase == Phase.ApprovedByCommittee
						|| m.Phase == Phase.RejectedByCommittee))
				.Include(m => m.Guide).AsNoTracking()
				.Select(m => new MemoirViewModel {
					Id = m.Id,
					Title = m.Title,
					Phase = m.Phase.ToString(),
					MemoristName = $"{m.Memorist!.FirstName} {m.Memorist!.LastName}",
					GuideName = $"{m.Guide!.FirstName} {m.Guide!.LastName}"
				});
		} else if (this.User.IsInRole("Guide") || this.User.IsInRole("Director")) {
			memoirs = this._dbContext.Memoirs!
				.Include(m => m.Owner)
				.Where(m => m.Phase == Phase.SentToCommittee || m.Phase == Phase.ApprovedByCommittee)
				.Include(m => m.Guide).AsNoTracking()
				.Select(m => new MemoirViewModel {
					Id = m.Id,
					Title = m.Title,
					Phase = m.Phase.ToString(),
					MemoristName = $"{m.Memorist!.FirstName} {m.Memorist!.LastName}",
					GuideName = $"{m.Guide!.FirstName} {m.Guide!.LastName}"
				});
		}
		var paginator = Paginator<MemoirViewModel>.Create(memoirs, pageNumber ?? 1, 6);
		if (!string.IsNullOrEmpty(sortOrder)) {
			paginator = Paginator<MemoirViewModel>.Sort(paginator.AsQueryable(), sortOrder, pageNumber ?? 1, 6, parameters);
		}
		if (!string.IsNullOrEmpty(searchString)) {
			paginator = Paginator<MemoirViewModel>.Filter(paginator.AsQueryable(), searchString, pageNumber ?? 1, 6, parameters);
		}
		return this.View(paginator);
	}

	[Authorize(Roles = "Student,Guide,Committee,Director")]
	public new async Task<IActionResult> View(string id) {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "Tu cuenta está desactivada.";
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		Memoir? memoir = null!;
		if (this.User.IsInRole("Student")) {
			memoir = await this._dbContext.Memoirs!
				.Where(m => m.Phase == Phase.SentToCommittee || m.Phase == Phase.ApprovedByCommittee
						|| m.Phase == Phase.RejectedByCommittee)
				.Include(m => m.Owner)
				.Include(m => m.Guide)
				.Include(m => m.WhoRejected)
				.Include(m => m.Assistants).AsNoTracking()
				.FirstOrDefaultAsync(m => m.Id == id);
		} else if (this.User.IsInRole("Guide")) {
			memoir = await this._dbContext.Memoirs!
				.Where(m => m.Phase == Phase.SentToCommittee || m.Phase == Phase.ApprovedByCommittee
						|| m.Phase == Phase.RejectedByCommittee)
				.Include(m => m.Owner)
				.Include(m => m.Memorist)
				.Include(m => m.Assistants).AsNoTracking()
				.FirstOrDefaultAsync(m => m.Id == id);
		} else if (this.User.IsInRole("Committee") || this.User.IsInRole("Director")) {
			memoir = await this._dbContext.Memoirs!
				.Where(m => m.Phase == Phase.SentToCommittee || m.Phase == Phase.ApprovedByCommittee)
				.Include(m => m.Owner)
				.Include(m => m.Memorist)
				.Include(m => m.Guide)
				.Include(m => m.WhoRejected)
				.Include(m => m.Assistants).AsNoTracking()
				.FirstOrDefaultAsync(m => m.Id == id);
		}
		if (memoir is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta";
			return this.RedirectToAction("Index", "Proposal", new { area = "University" });
		}
		var output = new MemoirViewModel {
			Id = id,
			Title = memoir.Title,
			Description = memoir.Description,
			Phase = memoir.Phase.ToString(),
			CreatedAt = memoir.CreatedAt,
			UpdatedAt = memoir.UpdatedAt,
			Assistants = memoir.Assistants!.Select(a => $"{a!.FirstName} {a.LastName}").ToList(),
			WhoRejected = memoir.WhoRejected is null ? string.Empty : $"{memoir.WhoRejected.FirstName} {memoir.WhoRejected.LastName}",
			Reason = memoir.Reason
		};
		if (this.User.IsInRole("Student") || this.User.IsInRole("Committee") || this.User.IsInRole("Director")) {
			output = new MemoirViewModel {
				GuideId = memoir.Guide!.Id,
				GuideName = $"{memoir.Guide.FirstName} {memoir.Guide.LastName}",
				GuideEmail = memoir.Guide.Email,
				Office = memoir.Guide.Office,
				Schedule = memoir.Guide.Schedule,
				Specialization = memoir.Guide.Specialization
			};
		} else if (this.User.IsInRole("Guide") || this.User.IsInRole("Committee") || this.User.IsInRole("Director")) {
			output = new MemoirViewModel {
				Requirements = memoir.Requirements,
				MemoristId = memoir.Memorist is null ? string.Empty : memoir.Memorist.Id,
				MemoristName = memoir.Memorist is null ? string.Empty : $"{memoir.Memorist.FirstName} {memoir.Memorist.LastName}",
				MemoristEmail = memoir.Memorist is null ? string.Empty : memoir.Memorist.Email,
				UniversityId = memoir.Memorist is null ? string.Empty : memoir.Memorist.UniversityId,
				RemainingCourses = memoir.Memorist is null ? string.Empty : memoir.Memorist.RemainingCourses,
				IsDoingThePractice = memoir.Memorist is not null && memoir.Memorist.IsDoingThePractice,
				IsWorking = memoir.Memorist is not null && memoir.Memorist.IsWorking
			};
		}
		return this.View(output);
	}
}
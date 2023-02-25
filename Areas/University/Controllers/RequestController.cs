﻿using Microsoft.AspNetCore.Authorization;
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

	[Authorize(Roles = "Student,Guide")]
	public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "Tu cuenta está desactivada.";
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var parameters = new[] { "Title", "MemoristName", "GuideName", "UpdatedAt" };
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
		var memoirs = this._dbContext.Memoirs!
				.Include(m => m.Owner)
				.Where(m => m.Owner!.Id == this._userManager.GetUserId(this.User)
					&& (m.Phase == Phase.SentToCommittee || m.Phase == Phase.ApprovedByCommittee
						|| m.Phase == Phase.RejectedByCommittee || m.Phase == Phase.ApprovedByDirector
						|| m.Phase == Phase.RejectedByDirector))
				.Include(m => m.Guide).AsNoTracking()
				.Select(m => new MemoirViewModel {
					Id = m.Id,
					Title = m.Title,
					Phase = m.Phase.ToString(),
					MemoristName = $"{m.Memorist!.FirstName} {m.Memorist!.LastName}",
					GuideName = $"{m.Guide!.FirstName} {m.Guide!.LastName}",
					UpdatedAt = m.UpdatedAt
				});
		var paginator = Paginator<MemoirViewModel>.Create(memoirs, pageNumber ?? 1, 6);
		if (!string.IsNullOrEmpty(sortOrder)) {
			paginator = Paginator<MemoirViewModel>.Sort(paginator.AsQueryable(), sortOrder, pageNumber ?? 1, 6, parameters);
		}
		if (!string.IsNullOrEmpty(searchString)) {
			paginator = Paginator<MemoirViewModel>.Filter(paginator.AsQueryable(), searchString, pageNumber ?? 1, 6, parameters);
		}
		return this.View(paginator);
	}

	[Authorize(Roles = "Committee,Director")]
	public async Task<IActionResult> List(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "Tu cuenta está desactivada.";
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var parameters = new[] { "Title", "MemoristName", "GuideName", "UpdatedAt" };
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
		var memoirs = this._dbContext.Memoirs!
				.Include(m => m.Owner)
				.Where(m => m.Phase == Phase.SentToCommittee || m.Phase == Phase.ApprovedByCommittee
					|| m.Phase == Phase.ApprovedByDirector)
				.Include(m => m.Guide).AsNoTracking()
				.Select(m => new MemoirViewModel {
					Id = m.Id,
					Title = m.Title,
					Phase = m.Phase.ToString(),
					MemoristName = $"{m.Memorist!.FirstName} {m.Memorist!.LastName}",
					GuideName = $"{m.Guide!.FirstName} {m.Guide!.LastName}",
					UpdatedAt = m.UpdatedAt
				});
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
		if (this.User.IsInRole("Student") || this.User.IsInRole("Committee") || this.User.IsInRole("Director")) {
			memoir = await this._dbContext.Memoirs!
				.Where(m => m.Phase == Phase.SentToCommittee || m.Phase == Phase.ApprovedByCommittee
						|| m.Phase == Phase.RejectedByCommittee || m.Phase == Phase.ApprovedByDirector
						|| m.Phase == Phase.RejectedByDirector)
				.Include(m => m.Owner)
				.Include(m => m.Guide)
				.Include(m => m.WhoRejected)
				.Include(m => m.Assistants).AsNoTracking()
				.FirstOrDefaultAsync(m => m.Id == id);
		} else if (this.User.IsInRole("Guide") || this.User.IsInRole("Committee") || this.User.IsInRole("Director")) {
			memoir = await this._dbContext.Memoirs!
				.Where(m => m.Phase == Phase.SentToCommittee || m.Phase == Phase.ApprovedByCommittee
						|| m.Phase == Phase.RejectedByCommittee || m.Phase == Phase.ApprovedByDirector
						|| m.Phase == Phase.RejectedByDirector)
				.Include(m => m.Owner)
				.Include(m => m.Memorist)
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
			output.GuideId = memoir.Guide!.Id;
			output.GuideName = $"{memoir.Guide.FirstName} {memoir.Guide.LastName}";
			output.GuideEmail = memoir.Guide.Email;
			output.Office = memoir.Guide.Office;
			output.Schedule = memoir.Guide.Schedule;
			output.Specialization = memoir.Guide.Specialization;
		} else if (this.User.IsInRole("Guide") || this.User.IsInRole("Committee") || this.User.IsInRole("Director")) {
			output.Requirements = memoir.Requirements;
			output.MemoristId = memoir.Memorist is null ? string.Empty : memoir.Memorist.Id;
			output.MemoristName = memoir.Memorist is null ? string.Empty : $"{memoir.Memorist.FirstName} {memoir.Memorist.LastName}";
			output.MemoristEmail = memoir.Memorist is null ? string.Empty : memoir.Memorist.Email;
			output.UniversityId = memoir.Memorist is null ? string.Empty : memoir.Memorist.UniversityId;
			output.RemainingCourses = memoir.Memorist is null ? string.Empty : memoir.Memorist.RemainingCourses;
			output.IsDoingThePractice = memoir.Memorist is not null && memoir.Memorist.IsDoingThePractice;
			output.IsWorking = memoir.Memorist is not null && memoir.Memorist.IsWorking;
		}
		return this.View(output);
	}
}
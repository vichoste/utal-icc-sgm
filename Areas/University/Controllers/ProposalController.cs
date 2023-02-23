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
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "Tu cuenta está desactivada.";
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
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
		var users = (await this._userManager.GetUsersInRoleAsync("Guide"))
			.Where(u => !u.IsDeactivated)
				.Select(u => new ApplicationUserViewModel {
					Id = u.Id,
					FirstName = u.FirstName,
					LastName = u.LastName,
					Rut = u.Rut,
					Email = u.Email,
					Specialization = u.Specialization,
				}
		).AsQueryable();
		var paginator = Paginator<ApplicationUserViewModel>.Create(users, pageNumber ?? 1, 6);
		if (!string.IsNullOrEmpty(sortOrder)) {
			paginator = Paginator<ApplicationUserViewModel>.Sort(paginator.AsQueryable(), sortOrder, pageNumber ?? 1, 6, parameters);
		}
		if (!string.IsNullOrEmpty(searchString)) {
			paginator = Paginator<ApplicationUserViewModel>.Filter(paginator.AsQueryable(), searchString, pageNumber ?? 1, 6, parameters);
		}
		return this.View(paginator);
	}

	[Authorize(Roles = "Guide")]
	public async Task<IActionResult> Students(string id, string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "Tu cuenta está desactivada.";
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var memoir = this._dbContext.Memoirs!
			.Include(m => m.Owner)
			.Include(m => m.Candidates).AsNoTracking()
			.FirstOrDefault(m => m.Id == id
				&& m.Owner!.Id == user.Id
				&& m.Phase == Phase.PublishedByGuide);
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
		var candidates = memoir!.Candidates!.Where(u => !u!.IsDeactivated).Select(u => new ApplicationUserViewModel {
			Id = u!.Id,
			FirstName = u.FirstName,
			LastName = u.LastName,
			Email = u.Email,
		}).AsQueryable();
		var paginator = Paginator<ApplicationUserViewModel>.Create(candidates, pageNumber ?? 1, 10);
		if (!string.IsNullOrEmpty(sortOrder)) {
			paginator = Paginator<ApplicationUserViewModel>.Sort(paginator.AsQueryable(), sortOrder, pageNumber ?? 1, 6, parameters);
		}
		if (!string.IsNullOrEmpty(searchString)) {
			paginator = Paginator<ApplicationUserViewModel>.Filter(paginator.AsQueryable(), searchString, pageNumber ?? 1, 6, parameters);
		}
		return this.View(paginator);
	}

	[Authorize(Roles = "Student,Guide")]
	public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "Tu cuenta está desactivada.";
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
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
				.Include(m => m.Owner)
				.Where(m => m.Owner!.Id == this._userManager.GetUserId(this.User)
					&& (m.Phase != Phase.SentToCommittee || m.Phase != Phase.RejectedByCommittee
						|| m.Phase != Phase.ApprovedByCommittee || m.Phase != Phase.InProgress
						|| m.Phase != Phase.Abandoned || m.Phase != Phase.Completed))
				.Include(m => m.Guide).AsNoTracking()
				.Select(m => new MemoirViewModel {
					Id = m.Id,
					Title = m.Title,
					Phase = m.Phase.ToString(),
					GuideName = $"{m.Guide!.FirstName} {m.Guide!.LastName}"
				});
		} else if (this.User.IsInRole("Guide")) {
			memoirs = this._dbContext.Memoirs!
				.Include(m => m.Owner)
				.Where(m => m.Owner!.Id == this._userManager.GetUserId(this.User))
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
			paginator = Paginator<MemoirViewModel>.Sort(paginator.AsQueryable(), sortOrder, pageNumber ?? 1, 6, parameters);
		}
		if (!string.IsNullOrEmpty(searchString)) {
			paginator = Paginator<MemoirViewModel>.Filter(paginator.AsQueryable(), searchString, pageNumber ?? 1, 6, parameters);
		}
		return this.View(paginator);
	}

	[Authorize(Roles = "Student,Guide")]
	public async Task<IActionResult> List(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "Tu cuenta está desactivada.";
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
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
				.Where(m => m.Phase == Phase.PublishedByGuide)
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
				.Where(m => m.Guide!.Id == user.Id
					&& (m.Phase == Phase.SentToGuide || m.Phase == Phase.ApprovedByGuide)
					&& (m.Phase != Phase.SentToCommittee || m.Phase != Phase.RejectedByCommittee
						|| m.Phase != Phase.ApprovedByCommittee || m.Phase != Phase.InProgress
						|| m.Phase != Phase.Abandoned || m.Phase != Phase.Completed))
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
			paginator = Paginator<MemoirViewModel>.Sort(paginator.AsQueryable(), sortOrder, pageNumber ?? 1, 6, parameters);
		}
		if (!string.IsNullOrEmpty(searchString)) {
			paginator = Paginator<MemoirViewModel>.Filter(paginator.AsQueryable(), searchString, pageNumber ?? 1, 6, parameters);
		}
		return this.View(paginator);
	}

	[Authorize(Roles = "Student")]
	public async Task<IActionResult> Applications(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "Tu cuenta está desactivada.";
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var parameters = new[] { "Title", "GuideName" };
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
			.Include(m => m.Memorist)
			.Where(m => (m!.Candidates!.Contains(user) || m.Memorist == user)
				&& (m.Phase == Phase.PublishedByGuide || m.Phase == Phase.ReadyByGuide)
				&& (m.Phase != Phase.SentToCommittee || m.Phase != Phase.RejectedByCommittee
					|| m.Phase != Phase.ApprovedByCommittee || m.Phase != Phase.InProgress
					|| m.Phase != Phase.Abandoned || m.Phase != Phase.Completed))
			.Include(m => m.Guide).AsNoTracking()
			.Select(m => new MemoirViewModel {
				Id = m.Id,
				Title = m.Title,
				Phase = m.Phase.ToString(),
				GuideName = $"{m.Guide!.FirstName} {m.Guide!.LastName}"
			}
		);
		var paginator = Paginator<MemoirViewModel>.Create(memoirs, pageNumber ?? 1, 6);
		if (!string.IsNullOrEmpty(sortOrder)) {
			paginator = Paginator<MemoirViewModel>.Sort(paginator.AsQueryable(), sortOrder, pageNumber ?? 1, 6, parameters);
		}
		if (!string.IsNullOrEmpty(searchString)) {
			paginator = Paginator<MemoirViewModel>.Filter(paginator.AsQueryable(), searchString, pageNumber ?? 1, 6, parameters);
		}
		return this.View(paginator);
	}

	[Authorize(Roles = "Guide")]
	public async Task<IActionResult> Create() {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "Tu cuenta está desactivada.";
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		await this.PopulateAssistants(user!);
		return this.View(new MemoirViewModel());
	}

	[Authorize(Roles = "Student")]
	public async Task<IActionResult> CreateWithGuide(string id) {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "Tu cuenta está desactivada.";
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var guide = await this._userManager.FindByIdAsync(id);
		if (guide is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al profesor guía.";
			return this.RedirectToAction("Guide", "Proposal", new { area = "University" });
		}
		if (guide.IsDeactivated) {
			this.TempData["ErrorMessage"] = "El profesor guía está desactivado.";
			return this.RedirectToAction("Guide", "Proposal", new { area = "University" });
		}
		await this.PopulateAssistants(guide);
		var output = new MemoirViewModel {
			GuideId = guide.Id,
			GuideName = $"{guide.FirstName} {guide.LastName}",
			GuideEmail = guide.Email,
			Office = guide.Office,
			Schedule = guide.Schedule,
			Specialization = guide.Specialization,
		};
		return this.View(output);
	}

	[Authorize(Roles = "Student,Guide"), HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Create([FromForm] MemoirViewModel input) {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "Tu cuenta está desactivada.";
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		ApplicationUser? guide = null!;
		if (this.User.IsInRole("Student")) {
			guide = (await this._userManager.FindByIdAsync(input.GuideId!))!;
			if (guide is null) {
				this.ViewData["ErrorMessage"] = "Error al obtener al profesor guía.";
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
			Owner = user,
			Guide = guide,
			Assistants = assistants,
			CreatedAt = DateTimeOffset.Now,
			UpdatedAt = DateTimeOffset.Now,
		};
		if (this.User.IsInRole("Student")) {
			memoir.Memorist = await this._userManager.GetUserAsync(this.User);
			memoir.Phase = Phase.DraftByStudent;
		} else if (this.User.IsInRole("Guide")) {
			memoir.Requirements = input.Requirements;
			memoir.Phase = Phase.DraftByGuide;
		}
		_ = await this._dbContext.Memoirs!.AddAsync(memoir);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "Tu propouesta ha sido registrada correctamente.";
		return this.RedirectToAction("Index", "Proposal", new { area = "University" });
	}

	[Authorize(Roles = "Student,Guide")]
	public async Task<IActionResult> Edit(string id) {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "Tu cuenta está desactivada.";
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		Memoir? memoir = null!;
		if (this.User.IsInRole("Student")) {
			memoir = await this._dbContext.Memoirs!
				.Include(m => m.Owner)
				.Where(m => m.Owner!.Id == this._userManager.GetUserId(this.User)
					&& (m.Phase != Phase.SentToCommittee || m.Phase != Phase.RejectedByCommittee
						|| m.Phase != Phase.ApprovedByCommittee || m.Phase != Phase.InProgress
						|| m.Phase != Phase.Abandoned || m.Phase != Phase.Completed))
				.Include(m => m.Guide)
				.Include(m => m.Assistants).AsNoTracking()
				.FirstOrDefaultAsync(m => m.Id == id && (m.Phase == Phase.DraftByStudent || m.Phase == Phase.RejectedByGuide));
		} else if (this.User.IsInRole("Guide")) {
			memoir = await this._dbContext.Memoirs!
				.Include(m => m.Owner)
				.Where(m => m.Owner!.Id == this._userManager.GetUserId(this.User)
					&& (m.Phase != Phase.SentToCommittee || m.Phase != Phase.RejectedByCommittee
						|| m.Phase != Phase.ApprovedByCommittee || m.Phase != Phase.InProgress
						|| m.Phase != Phase.Abandoned || m.Phase != Phase.Completed))
				.Include(m => m.Guide)
				.Include(m => m.Assistants).AsNoTracking()
				.FirstOrDefaultAsync(m => m.Id == id && (m.Phase == Phase.DraftByGuide || m.Phase == Phase.RejectedByGuide));
		}
		if (memoir is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta";
			return this.RedirectToAction("Index", "Proposal", new { area = "University" });
		}
		if (memoir.Guide!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "El profesor guía está desactivado.";
			return this.RedirectToAction("Index", "Proposal", new { area = "University" });
		}
		await this.PopulateAssistants(memoir.Guide!);
		var output = new MemoirViewModel {
			Id = memoir.Id,
			Title = memoir.Title,
			Description = memoir.Description,
			Assistants = memoir.Assistants!.Select(a => a!.Id).ToList(),
			CreatedAt = memoir.CreatedAt,
			UpdatedAt = memoir.UpdatedAt
		};
		if (this.User.IsInRole("Student")) {
			output.GuideId = memoir.Guide!.Id;
			output.GuideName = $"{memoir.Guide!.FirstName} {memoir.Guide!.LastName}";
			output.GuideEmail = memoir.Guide!.Email;
			output.Office = memoir.Guide!.Office;
			output.Schedule = memoir.Guide!.Schedule;
			output.Specialization = memoir.Guide!.Specialization;
		} else if (this.User.IsInRole("Guide")) {
			output.Requirements = memoir.Requirements;
		}
		return this.View(output);
	}

	[Authorize(Roles = "Student,Guide"), HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit([FromForm] MemoirViewModel input) {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "Tu cuenta está desactivada.";
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		Memoir? memoir = null!;
		if (this.User.IsInRole("Student")) {
			memoir = await this._dbContext.Memoirs!
				.Include(m => m.Owner)
				.Where(m => m.Owner!.Id == this._userManager.GetUserId(this.User)
					&& (m.Phase != Phase.SentToCommittee || m.Phase != Phase.RejectedByCommittee
						|| m.Phase != Phase.ApprovedByCommittee || m.Phase != Phase.InProgress
						|| m.Phase != Phase.Abandoned || m.Phase != Phase.Completed))
				.Include(m => m.Guide)
				.Include(m => m.Assistants)
				.FirstOrDefaultAsync(m => m.Id == input.Id && (m.Phase == Phase.DraftByStudent || m.Phase == Phase.RejectedByGuide));
		} else if (this.User.IsInRole("Guide")) {
			memoir = await this._dbContext.Memoirs!
				.Include(m => m.Owner)
				.Where(m => m.Owner!.Id == this._userManager.GetUserId(this.User)
					&& (m.Phase != Phase.SentToCommittee || m.Phase != Phase.RejectedByCommittee
						|| m.Phase != Phase.ApprovedByCommittee || m.Phase != Phase.InProgress
						|| m.Phase != Phase.Abandoned || m.Phase != Phase.Completed))
				.Include(m => m.Assistants)
				.FirstOrDefaultAsync(m => m.Id == input.Id && (m.Phase == Phase.DraftByGuide || m.Phase == Phase.RejectedByGuide));
		}
		if (memoir is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta";
			return this.RedirectToAction("Index", "Proposal", new { area = "University" });
		}
		if (memoir.Guide!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "El profesor guía está desactivado.";
			return this.RedirectToAction("Index", "Proposal", new { area = "University" });
		}
		var assistants = input.Assistants!.Select(async a => await this._userManager.FindByIdAsync(a!)).Select(a => a.Result).Where(u => !u!.IsDeactivated).ToList();
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
			if (memoir.Phase == Phase.RejectedByGuide) {
				memoir.Phase = Phase.SentToGuide;
			}
			memoir.UpdatedAt = DateTimeOffset.Now;
			_ = this._dbContext.Memoirs!.Update(memoir);
			_ = await this._dbContext.SaveChangesAsync();
		}
		await this.PopulateAssistants(memoir.Guide!);
		var output = new MemoirViewModel {
			Id = memoir.Id,
			Title = memoir.Title,
			Description = memoir.Description,
			Assistants = memoir.Assistants!.Select(a => a!.Id).ToList(),
			CreatedAt = memoir.CreatedAt,
			UpdatedAt = memoir.UpdatedAt
		};
		if (this.User.IsInRole("Student")) {
			output.GuideId = memoir.Guide!.Id;
			output.GuideName = $"{memoir.Guide!.FirstName} {memoir.Guide!.LastName}";
			output.GuideEmail = memoir.Guide!.Email;
			output.Office = memoir.Guide!.Office;
			output.Schedule = memoir.Guide!.Schedule;
			output.Specialization = memoir.Guide!.Specialization;
		} else if (this.User.IsInRole("Guide")) {
			output.Requirements = memoir.Requirements;
		}
		this.ViewBag.SuccessMessage = "Tu propuesta ha sido editada correctamente.";
		return this.View(output);
	}

	[Authorize(Roles = "Student,Guide")]
	public async Task<IActionResult> Delete(string id) {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "Tu cuenta está desactivada.";
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		Memoir? memoir = null!;
		if (this.User.IsInRole("Student")) {
			memoir = await this._dbContext.Memoirs!
				.Include(m => m.Owner).AsNoTracking()
				.FirstOrDefaultAsync(m => m.Owner!.Id == this._userManager.GetUserId(this.User)
					&& m.Id == id
					&& m.Phase == Phase.DraftByStudent);
		} else if (this.User.IsInRole("Guide")) {
			memoir = await this._dbContext.Memoirs!
				.Include(m => m.Owner).AsNoTracking()
				.FirstOrDefaultAsync(m => m.Owner!.Id == this._userManager.GetUserId(this.User)
					&& m.Id == id
					&& m.Phase == Phase.DraftByGuide);
		}
		if (memoir is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta";
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
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "Tu cuenta está desactivada.";
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		Memoir? memoir = null!;
		if (this.User.IsInRole("Student")) {
			memoir = await this._dbContext.Memoirs!
				.Include(m => m.Owner)
				.FirstOrDefaultAsync(m => m.Owner!.Id == this._userManager.GetUserId(this.User)
					&& m.Id == input.Id
					&& m.Phase == Phase.DraftByStudent);
		} else if (this.User.IsInRole("Guide")) {
			memoir = await this._dbContext.Memoirs!
				.Include(m => m.Owner)
				.FirstOrDefaultAsync(m => m.Owner!.Id == this._userManager.GetUserId(this.User)
					&& m.Id == input.Id
					&& m.Phase == Phase.DraftByGuide);
		}
		if (memoir is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta";
			return this.RedirectToAction("Index", "Proposal", new { area = "University" });
		}
		_ = this._dbContext.Memoirs!.Remove(memoir);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "Tu propuesta ha sido eliminada correctamente.";
		return this.RedirectToAction("Index", "Proposal", new { area = "University" });
	}

	[Authorize(Roles = "Student,Guide")]
	public async Task<IActionResult> Send(string id) {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "Tu cuenta está desactivada.";
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		Memoir? memoir = null!;
		if (this.User.IsInRole("Student")) {
			memoir = await this._dbContext.Memoirs!
				.Include(m => m.Owner)
				.Where(m => m.Owner!.Id == this._userManager.GetUserId(this.User)
					&& (m.Phase != Phase.SentToCommittee || m.Phase != Phase.RejectedByCommittee
						|| m.Phase != Phase.ApprovedByCommittee || m.Phase != Phase.InProgress
						|| m.Phase != Phase.Abandoned || m.Phase != Phase.Completed))
				.Include(m => m.Guide).AsNoTracking()
				.FirstOrDefaultAsync(m => m.Id == id && m.Phase == Phase.DraftByStudent);
		} else if (this.User.IsInRole("Guide")) {
			memoir = await this._dbContext.Memoirs!
				.Include(m => m.Owner)
				.Where(m => m.Owner!.Id == this._userManager.GetUserId(this.User)
					&& (m.Phase != Phase.SentToCommittee || m.Phase != Phase.RejectedByCommittee
						|| m.Phase != Phase.ApprovedByCommittee || m.Phase != Phase.InProgress
						|| m.Phase != Phase.Abandoned || m.Phase != Phase.Completed)).AsNoTracking()
				.Include(m => m.Guide).AsNoTracking()
				.FirstOrDefaultAsync(m => m.Id == id && m.Phase == Phase.DraftByGuide);
		}
		if (memoir is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta";
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
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "Tu cuenta está desactivada.";
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		Memoir? memoir = null!;
		if (this.User.IsInRole("Student")) {
			memoir = await this._dbContext.Memoirs!
				.Include(m => m.Owner)
				.Where(m => m.Owner!.Id == this._userManager.GetUserId(this.User)
					&& (m.Phase != Phase.SentToCommittee || m.Phase != Phase.RejectedByCommittee
						|| m.Phase != Phase.ApprovedByCommittee || m.Phase != Phase.InProgress
						|| m.Phase != Phase.Abandoned || m.Phase != Phase.Completed))
				.Include(m => m.Guide)
				.FirstOrDefaultAsync(m => m.Id == input.Id && m.Phase == Phase.DraftByStudent);
		} else if (this.User.IsInRole("Guide")) {
			memoir = await this._dbContext.Memoirs!
				.Include(m => m.Owner)
				.Where(m => m.Owner!.Id == this._userManager.GetUserId(this.User)
					&& (m.Phase != Phase.SentToCommittee || m.Phase != Phase.RejectedByCommittee
						|| m.Phase != Phase.ApprovedByCommittee || m.Phase != Phase.InProgress
						|| m.Phase != Phase.Abandoned || m.Phase != Phase.Completed))
				.Include(m => m.Guide)
				.FirstOrDefaultAsync(m => m.Id == input.Id && m.Phase == Phase.DraftByGuide);
		}
		if (memoir is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta";
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
		memoir.WhoRejected = null;
		memoir.Reason = string.Empty;
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
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "Tu cuenta está desactivada.";
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		Memoir? memoir = null!;
		if (this.User.IsInRole("Student")) {
			memoir = await this._dbContext.Memoirs!
				.Where(m => m.Phase != Phase.SentToCommittee
					|| m.Phase != Phase.RejectedByCommittee
					|| m.Phase != Phase.ApprovedByCommittee
					|| m.Phase != Phase.InProgress
					|| m.Phase != Phase.Abandoned
					|| m.Phase != Phase.Completed)
				.Include(m => m.Owner)
				.Include(m => m.Guide)
				.Include(m => m.WhoRejected)
				.Include(m => m.Assistants).AsNoTracking()
				.FirstOrDefaultAsync(m => m.Id == id);
		} else if (this.User.IsInRole("Guide")) {
			memoir = await this._dbContext.Memoirs!
				.Where(m => m.Phase != Phase.SentToCommittee
					|| m.Phase != Phase.RejectedByCommittee
					|| m.Phase != Phase.ApprovedByCommittee
					|| m.Phase != Phase.InProgress
					|| m.Phase != Phase.Abandoned
					|| m.Phase != Phase.Completed)
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
			Assistants = memoir.Assistants!.Select(a => $"{a!.FirstName} {a.LastName}").ToList(),
			CreatedAt = memoir.CreatedAt,
			UpdatedAt = memoir.UpdatedAt,
		};
		if (this.User.IsInRole("Student")) {
			output.GuideId = memoir.Guide!.Id;
			output.GuideName = $"{memoir.Guide.FirstName} {memoir.Guide.LastName}";
			output.GuideEmail = memoir.Guide.Email;
			output.Office = memoir.Guide.Office;
			output.Schedule = memoir.Guide.Schedule;
			output.Specialization = memoir.Guide.Specialization;
			output.WhoRejected = memoir.WhoRejected is null ? string.Empty : $"{memoir.WhoRejected.FirstName} {memoir.WhoRejected.LastName}";
			output.Reason = memoir.Reason;
		} else if (this.User.IsInRole("Guide")) {
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

	[Authorize(Roles = "Student")]
	public async Task<IActionResult> Apply(string id) {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "Tu cuenta está desactivada.";
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var memoir = await this._dbContext.Memoirs!
			.Where(m => m.Phase != Phase.SentToCommittee
				|| m.Phase != Phase.RejectedByCommittee
				|| m.Phase != Phase.ApprovedByCommittee
				|| m.Phase != Phase.InProgress
				|| m.Phase != Phase.Abandoned
				|| m.Phase != Phase.Completed)
			.Include(p => p.Guide)
			.FirstOrDefaultAsync(m => m.Id == id
				&& m.Phase == Phase.PublishedByGuide);
		if (memoir is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta";
			return this.RedirectToAction("Applications", "Proposal", new { area = "University" });
		}
		var output = new MemoirViewModel {
			Id = id,
			Title = memoir.Title,
			GuideName = $"{memoir.Guide!.FirstName} {memoir.Guide.LastName}"
		};
		return this.View(output);
	}

	[Authorize(Roles = "Student"), HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Apply([FromForm] MemoirViewModel input) {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "Tu cuenta está desactivada.";
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var memoir = await this._dbContext.Memoirs!
			.Where(m => m.Phase != Phase.SentToCommittee
				|| m.Phase != Phase.RejectedByCommittee
				|| m.Phase != Phase.ApprovedByCommittee
				|| m.Phase != Phase.InProgress
				|| m.Phase != Phase.Abandoned
				|| m.Phase != Phase.Completed)
			.Include(p => p.Guide)
			.FirstOrDefaultAsync(m => m.Id == input.Id
				&& m.Phase == Phase.PublishedByGuide);
		if (memoir is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta";
			return this.RedirectToAction("Applications", "Proposal", new { area = "University" });
		}
		memoir.Candidates!.Add(user);
		memoir.UpdatedAt = DateTimeOffset.Now;
		_ = this._dbContext.Memoirs!.Update(memoir);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "Has postulado a la propuesta correctamente.";
		return this.RedirectToAction("Applications", "Proposal", new { area = "University" });
	}

	[Authorize(Roles = "Guide")]
	public async Task<IActionResult> Reject(string id) {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "Tu cuenta está desactivada.";
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var memoir = await this._dbContext.Memoirs!
			.Include(m => m.Guide)
			.Where(m => m.Guide!.Id == user.Id
				&& (m.Phase != Phase.SentToCommittee
					|| m.Phase != Phase.RejectedByCommittee
					|| m.Phase != Phase.ApprovedByCommittee
					|| m.Phase != Phase.InProgress
					|| m.Phase != Phase.Abandoned
					|| m.Phase != Phase.Completed))
			.Include(m => m.Memorist)
			.FirstOrDefaultAsync(m => m.Id == id
				&& m.Phase == Phase.SentToGuide);
		if (memoir is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta";
			return this.RedirectToAction("List", "Proposal", new { area = "University" });
		}
		var output = new MemoirViewModel {
			Id = id,
			Title = memoir.Title,
			MemoristName = $"{memoir.Memorist!.FirstName} {memoir.Memorist.LastName}"
		};
		return this.View(output);
	}

	[Authorize(Roles = "Guide"), HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Reject([FromForm] MemoirViewModel input) {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "Tu cuenta está desactivada.";
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var memoir = await this._dbContext.Memoirs!
			.Include(m => m.Guide)
			.Where(m => m.Guide!.Id == user.Id
				&& (m.Phase != Phase.SentToCommittee
					|| m.Phase != Phase.RejectedByCommittee
					|| m.Phase != Phase.ApprovedByCommittee
					|| m.Phase != Phase.InProgress
					|| m.Phase != Phase.Abandoned
					|| m.Phase != Phase.Completed))
			.Include(m => m.Memorist)
			.FirstOrDefaultAsync(m => m.Id == input.Id
				&& m.Phase == Phase.SentToGuide);
		if (memoir is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta";
			return this.RedirectToAction("List", "Proposal", new { area = "University" });
		}
		memoir.Phase = Phase.RejectedByGuide;
		memoir.WhoRejected = user;
		memoir.Reason = input.Reason;
		memoir.UpdatedAt = DateTimeOffset.Now;
		_ = this._dbContext.Memoirs!.Update(memoir);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "La propuesta ha sido rechazada correctamente.";
		return this.RedirectToAction("List", "Proposal", new { area = "University" });
	}

	[Authorize(Roles = "Guide")]
	public async Task<IActionResult> Approve(string id) {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "Tu cuenta está desactivada.";
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var memoir = await this._dbContext.Memoirs!
			.Include(m => m.Guide)
			.Where(m => m.Guide!.Id == user.Id
				&& (m.Phase != Phase.SentToCommittee
					|| m.Phase != Phase.RejectedByCommittee
					|| m.Phase != Phase.ApprovedByCommittee
					|| m.Phase != Phase.InProgress
					|| m.Phase != Phase.Abandoned
					|| m.Phase != Phase.Completed))
			.Include(m => m.Memorist)
			.FirstOrDefaultAsync(m => m.Id == id
				&& m.Phase == Phase.SentToGuide);
		if (memoir is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta";
			return this.RedirectToAction("List", "Proposal", new { area = "University" });
		}
		var output = new MemoirViewModel {
			Id = id,
			Title = memoir.Title,
			MemoristName = $"{memoir.Memorist!.FirstName} {memoir.Memorist.LastName}"
		};
		return this.View(output);
	}

	[Authorize(Roles = "Guide"), HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Approve([FromForm] MemoirViewModel input) {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "Tu cuenta está desactivada.";
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var memoir = await this._dbContext.Memoirs!
			.Include(m => m.Guide)
			.Where(m => m.Guide!.Id == user.Id
				&& (m.Phase != Phase.SentToCommittee
					|| m.Phase != Phase.RejectedByCommittee
					|| m.Phase != Phase.ApprovedByCommittee
					|| m.Phase != Phase.InProgress
					|| m.Phase != Phase.Abandoned
					|| m.Phase != Phase.Completed))
			.Include(m => m.Memorist)
			.FirstOrDefaultAsync(m => m.Id == input.Id
				&& m.Phase == Phase.SentToGuide);
		if (memoir is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta";
			return this.RedirectToAction("List", "Proposal", new { area = "University" });
		}
		memoir.Phase = Phase.ApprovedByGuide;
		memoir.UpdatedAt = DateTimeOffset.Now;
		_ = this._dbContext.Memoirs!.Update(memoir);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "La propuesta ha sido aprobada correctamente.";
		return this.RedirectToAction("List", "Proposal", new { area = "University" });
	}

	[Authorize(Roles = "Guide")]
	public async Task<IActionResult> Select(string memoirId, string memoristId) {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "Tu cuenta está desactivada.";
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var student = await this._userManager.FindByIdAsync(memoristId);
		if (user is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al estudiante";
			return this.RedirectToAction("Students", "Proposal", new { area = "University" });
		}
		var memoir = await this._dbContext.Memoirs!
			.Where(m => m.Owner == user
				&& (m.Phase != Phase.SentToCommittee
					|| m.Phase != Phase.RejectedByCommittee
					|| m.Phase != Phase.ApprovedByCommittee
					|| m.Phase != Phase.InProgress
					|| m.Phase != Phase.Abandoned
					|| m.Phase != Phase.Completed))
			.Include(m => m.Candidates)
			.Where(m => m.Candidates!.Any(s => s!.Id == memoristId)).AsNoTracking()
			.FirstOrDefaultAsync(m => m.Id == memoirId
				&& m.Phase == Phase.PublishedByGuide);
		if (memoir is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction("Students", "Proposal", new { area = "University" });
		}
		var output = new MemoirViewModel {
			Id = memoirId,
			MemoristId = memoristId,
			MemoristName = $"{student!.FirstName} {student.LastName}",
			MemoristEmail = student.Email,
			UniversityId = student.UniversityId,
			RemainingCourses = student.RemainingCourses,
			IsDoingThePractice = student.IsDoingThePractice,
			IsWorking = student.IsWorking
		};
		return this.View(output);
	}

	[Authorize(Roles = "Guide"), HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Select([FromForm] MemoirViewModel input) {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "Tu cuenta está desactivada.";
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var memoir = await this._dbContext.Memoirs!
			.Where(m => m.Owner == user
				&& (m.Phase != Phase.SentToCommittee
					|| m.Phase != Phase.RejectedByCommittee
					|| m.Phase != Phase.ApprovedByCommittee
					|| m.Phase != Phase.InProgress
					|| m.Phase != Phase.Abandoned
					|| m.Phase != Phase.Completed))
			.Include(m => m.Candidates)
			.Where(m => m.Candidates!.Any(s => s!.Id == input.MemoristId)).AsNoTracking()
			.FirstOrDefaultAsync(m => m.Id == input.Id
				&& m.Phase == Phase.PublishedByGuide);
		if (memoir is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction("Index", "Proposal", new { area = "University" });
		}
		if (!memoir.Candidates!.Any(s => s!.Id == input.MemoristId)) {
			this.TempData["ErrorMessage"] = "El estudiante no está en la lista de candidatos.";
			return this.RedirectToAction("Index", "Proposal", new { area = "University" });
		}
		var student = await this._userManager.FindByIdAsync(input!.MemoristId!);
		if (student is null) {
			this.TempData["ErrorMessage"] = "Error al obtener al estudiante.";
			return this.RedirectToAction("Index", "Proposal", new { area = "University" });
		}
		_ = memoir.Candidates!.Remove(student);
		memoir.Phase = Phase.ReadyByGuide;
		memoir.Memorist = student;
		memoir.UpdatedAt = DateTimeOffset.Now;
		_ = this._dbContext.Memoirs!.Update(memoir);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "El estudiante ha sido seleccionado a la propuesta correctamente.";
		return this.RedirectToAction("Index", "Proposal", new { area = "University" });
	}

	[Authorize(Roles = "Student,Guide")]
	public async Task<IActionResult> Convert(string id) {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "Tu cuenta está desactivada.";
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		Memoir? memoir = null!;
		if (this.User.IsInRole("Student")) {
			memoir = await this._dbContext.Memoirs!
				.Include(m => m.Memorist)
				.Where(m => m.Memorist!.Id == user.Id
					&& (m.Phase != Phase.SentToCommittee
						|| m.Phase != Phase.RejectedByCommittee
						|| m.Phase != Phase.ApprovedByCommittee
						|| m.Phase != Phase.InProgress
						|| m.Phase != Phase.Abandoned
						|| m.Phase != Phase.Completed))
				.FirstOrDefaultAsync(m => m.Id == id
					&& m.Phase == Phase.ApprovedByGuide);
		} else if (this.User.IsInRole("Teacher")) {
			memoir = await this._dbContext.Memoirs!
				.Include(m => m.Guide)
				.Where(m => m.Guide!.Id == user.Id
					&& (m.Phase != Phase.SentToCommittee
						|| m.Phase != Phase.RejectedByCommittee
						|| m.Phase != Phase.ApprovedByCommittee
						|| m.Phase != Phase.InProgress
						|| m.Phase != Phase.Abandoned
						|| m.Phase != Phase.Completed))
				.FirstOrDefaultAsync(m => m.Id == id
					&& m.Phase == Phase.ReadyByGuide);
		}
		if (memoir is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta";
			return this.RedirectToAction("List", "Proposal", new { area = "University" });
		}
		var output = new MemoirViewModel {
			Id = id,
			Title = memoir.Title
		};
		return this.View(output);
	}

	[Authorize(Roles = "Student,Guide"), HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Convert([FromForm] MemoirViewModel input) {
		var user = await this._userManager.GetUserAsync(this.User);
		if (user!.IsDeactivated) {
			this.TempData["ErrorMessage"] = "Tu cuenta está desactivada.";
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		Memoir? memoir = null!;
		if (this.User.IsInRole("Student")) {
			memoir = await this._dbContext.Memoirs!
				.Include(m => m.Memorist)
				.Where(m => m.Memorist!.Id == user.Id
					&& (m.Phase != Phase.SentToCommittee
						|| m.Phase != Phase.RejectedByCommittee
						|| m.Phase != Phase.ApprovedByCommittee
						|| m.Phase != Phase.InProgress
						|| m.Phase != Phase.Abandoned
						|| m.Phase != Phase.Completed))
				.FirstOrDefaultAsync(m => m.Id == input.Id
					&& m.Phase == Phase.ApprovedByGuide);
		} else if (this.User.IsInRole("Teacher")) {
			memoir = await this._dbContext.Memoirs!
				.Include(m => m.Guide)
				.Where(m => m.Guide!.Id == user.Id
					&& (m.Phase != Phase.SentToCommittee
						|| m.Phase != Phase.RejectedByCommittee
						|| m.Phase != Phase.ApprovedByCommittee
						|| m.Phase != Phase.InProgress
						|| m.Phase != Phase.Abandoned
						|| m.Phase != Phase.Completed))
				.FirstOrDefaultAsync(m => m.Id == input.Id
					&& m.Phase == Phase.ReadyByGuide);
		}
		if (memoir is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta";
			return this.RedirectToAction("List", "Proposal", new { area = "University" });
		}
		memoir.Phase = Phase.SentToCommittee;
		memoir.UpdatedAt = DateTimeOffset.Now;
		_ = this._dbContext.Memoirs!.Update(memoir);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "Tu propuesta ha sido enviada correctamente.";
		return this.RedirectToAction("Index", "Request", new { area = "University" });
	}
}
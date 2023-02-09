using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Utal.Icc.Sgm.Controllers;
using Utal.Icc.Sgm.Data;

using Utal.Icc.Sgm.Models;
using Utal.Icc.Sgm.ViewModels;

using static Utal.Icc.Sgm.Models.ApplicationUser;
using static Utal.Icc.Sgm.Models.GuideTeacherProposal;

namespace Utal.Icc.Sgm.Areas.Student.Controllers;

[Area(nameof(Student)), Authorize(Roles = nameof(Roles.Student))]
public class GuideTeacherProposalController : ApplicationController {
	public GuideTeacherProposalController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore, SignInManager<ApplicationUser> signInManager) : base(dbContext, userManager, userStore, signInManager) { }

	protected IEnumerable<GuideTeacherProposalViewModel> Filter(string searchString, IOrderedEnumerable<GuideTeacherProposalViewModel> viewModels, params string[] parameters) {
		var result = new List<GuideTeacherProposalViewModel>();
		foreach (var parameter in parameters) {
			var partials = viewModels
					.Where(vm => (vm.GetType().GetProperty(parameter)!.GetValue(vm, null) as string)!.Contains(searchString));
			foreach (var partial in partials) {
				if (!result.Any(vm => vm.Id == partial.Id)) {
					result.Add(partial);
				}
			}
		}
		return result.AsEnumerable();
	}

	protected IOrderedEnumerable<GuideTeacherProposalViewModel> Sort(string sortOrder, IEnumerable<GuideTeacherProposalViewModel> viewModels, params string[] parameters) {
		foreach (var parameter in parameters) {
			if (parameter == sortOrder) {
				return viewModels.OrderBy(vm => vm.GetType().GetProperty(parameter)!.GetValue(vm, null));
			} else if ($"{parameter}Desc" == sortOrder) {
				return viewModels.OrderByDescending(vm => vm.GetType().GetProperty(parameter)!.GetValue(vm, null));
			}
		}
		return viewModels.OrderBy(vm => vm.GetType().GetProperty(parameters[0]));
	}

	public async Task<IActionResult> GuideTeacherProposal(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var parameters = new[] { nameof(GuideTeacherProposalViewModel.Title), nameof(GuideTeacherProposalViewModel.GuideTeacherName) };
		base.SetSortParameters(sortOrder, parameters);
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		var proposals = this._dbContext.GuideTeacherProposals!.AsNoTracking()
			.Where(p => p.StudentsWhoAreInterestedInThisGuideTeacherProposal!.Contains(user) && (p.ProposalStatus == Status.Published || p.ProposalStatus == Status.Ready))
			.Include(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal).AsNoTracking()
			.Select(p => new GuideTeacherProposalViewModel {
				Id = p.Id,
				Title = p.Title,
				GuideTeacherName = $"{p.GuideTeacherOwnerOfTheGuideTeacherProposal!.FirstName} {p.GuideTeacherOwnerOfTheGuideTeacherProposal!.LastName}",
				ProposalStatus = p.ProposalStatus.ToString(),
			}).AsEnumerable();
		var ordered = this.Sort(sortOrder, proposals, parameters);
		var output = !searchString.IsNullOrEmpty() ? this.Filter(searchString, ordered!, parameters) : ordered;
		return this.View(PaginatedList<GuideTeacherProposalViewModel>.Create(output!.AsQueryable(), pageNumber ?? 1, 6));
	}

	public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var parameters = new[] { nameof(GuideTeacherProposalViewModel.Title), nameof(GuideTeacherProposalViewModel.GuideTeacherName) };
		base.SetSortParameters(sortOrder, parameters);
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		var proposals = this._dbContext.GuideTeacherProposals!.AsNoTracking()
			.Where(p => p.ProposalStatus == Status.Published)
			.Include(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal).AsNoTracking()
			.Select(p => new GuideTeacherProposalViewModel {
				Id = p.Id,
				Title = p.Title,
				GuideTeacherName = $"{p.GuideTeacherOwnerOfTheGuideTeacherProposal!.FirstName} {p.GuideTeacherOwnerOfTheGuideTeacherProposal!.LastName}",
				ProposalStatus = p.ProposalStatus.ToString(),
			}).AsEnumerable();
		var ordered = this.Sort(sortOrder, proposals, parameters);
		var output = !searchString.IsNullOrEmpty() ? this.Filter(searchString, ordered!, parameters) : ordered;
		return this.View(PaginatedList<GuideTeacherProposalViewModel>.Create(output!.AsQueryable(), pageNumber ?? 1, 6));
	}

	public new async Task<IActionResult> View(string id) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.GuideTeacherProposals!.AsNoTracking()
			.Where(p => p.ProposalStatus == Status.Published)
			.Include(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal).AsNoTracking()
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
			GuideTeacherName = $"{proposal.GuideTeacherOwnerOfTheGuideTeacherProposal!.FirstName} {proposal.GuideTeacherOwnerOfTheGuideTeacherProposal!.LastName}",
			GuideTeacherEmail = proposal.GuideTeacherOwnerOfTheGuideTeacherProposal.Email,
			GuideTeacherOffice = proposal.GuideTeacherOwnerOfTheGuideTeacherProposal.TeacherOffice,
			GuideTeacherSchedule = proposal.GuideTeacherOwnerOfTheGuideTeacherProposal.TeacherSchedule,
			GuideTeacherSpecialization = proposal.GuideTeacherOwnerOfTheGuideTeacherProposal.TeacherSpecialization,
			AssistantTeachers = proposal.AssistantTeachersOfTheGuideTeacherProposal!.Select(at => $"{at!.FirstName} {at!.LastName}").ToList(),
			CreatedAt = proposal.CreatedAt,
			UpdatedAt = proposal.UpdatedAt,
		};
		return this.View(output);
	}

	public async Task<IActionResult> Apply(string id) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.GuideTeacherProposals!.AsNoTracking()
			.Where(p => p.ProposalStatus == Status.Published)
			.Include(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal).AsNoTracking()
			.FirstOrDefaultAsync(p => p.Id == id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });

		}
		var output = new GuideTeacherProposalViewModel {
			Id = id,
			Title = proposal.Title,
			GuideTeacherName = $"{proposal.GuideTeacherOwnerOfTheGuideTeacherProposal!.FirstName} {proposal.GuideTeacherOwnerOfTheGuideTeacherProposal.LastName}"
		};
		return this.View(output);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Apply([FromForm] GuideTeacherProposalViewModel input) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.GuideTeacherProposals!.AsNoTracking()
			.Where(p => p.ProposalStatus == Status.Published)
			.Include(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal).AsNoTracking()
			.FirstOrDefaultAsync(p => p.Id == input.Id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		proposal.StudentsWhoAreInterestedInThisGuideTeacherProposal!.Add(user);
		proposal.UpdatedAt = DateTimeOffset.Now;
		_ = this._dbContext.GuideTeacherProposals!.Update(proposal);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "Has postulado a la propuesta correctamente.";
		return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
	}
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Utal.Icc.Sgm.Controllers;
using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;
using Utal.Icc.Sgm.ViewModels;

using static Utal.Icc.Sgm.Models.ApplicationUser;

namespace Utal.Icc.Sgm.Areas.Student.Controllers;

[Area(nameof(Student)), Authorize(Roles = nameof(Roles.Student))]
public class StudentProposalController : ApplicationController {
	public StudentProposalController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore, SignInManager<ApplicationUser> signInManager) : base(dbContext, userManager, userStore, signInManager) { }

	protected async Task Populate(ApplicationUser guideTeacher) {
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
	
	protected IEnumerable<StudentProposalViewModel> Filter(string searchString, IOrderedEnumerable<StudentProposalViewModel> viewModels, params string[] parameters) {
		var result = new List<StudentProposalViewModel>();
		foreach (var parameter in parameters) {
			var partials = viewModels
					.Where(vm => (vm.GetType().GetProperty(parameter)!.GetValue(vm) as string)!.Contains(searchString));
			foreach (var partial in partials) {
				if (!result.Any(vm => vm.Id == partial.Id)) {
					result.Add(partial);
				}
			}
		}
		return result.AsEnumerable();
	}

	protected IOrderedEnumerable<StudentProposalViewModel> Sort(string sortOrder, IEnumerable<StudentProposalViewModel> viewModels, params string[] parameters) {
		foreach (var parameter in parameters) {
			if (parameter == sortOrder) {
				return viewModels.OrderBy(vm => vm.GetType().GetProperty(parameter)!.GetValue(vm, null));
			} else if ($"{parameter}Desc" == sortOrder) {
				return viewModels.OrderByDescending(vm => vm.GetType().GetProperty(parameter)!.GetValue(vm, null));
			}
		}
		return viewModels.OrderBy(vm => vm.GetType().GetProperty(parameters[0]));
	}

	protected IEnumerable<ApplicationUserViewModel> Filter(string searchString, IOrderedEnumerable<ApplicationUserViewModel> viewModels, params string[] parameters) {
		var result = new List<ApplicationUserViewModel>();
		foreach (var parameter in parameters) {
			var partials = viewModels
					.Where(vm => (vm.GetType().GetProperty(parameter)!.GetValue(vm) as string)!.Contains(searchString));
			foreach (var partial in partials) {
				if (!result.Any(vm => vm.Id == partial.Id)) {
					result.Add(partial);
				}
			}
		}
		return result.AsEnumerable();
	}

	protected IOrderedEnumerable<ApplicationUserViewModel> Sort(string sortOrder, IEnumerable<ApplicationUserViewModel> viewModels, params string[] parameters) {
		foreach (var parameter in parameters) {
			if (parameter == sortOrder) {
				return viewModels.OrderBy(vm => vm.GetType().GetProperty(parameter)!.GetValue(vm, null));
			} else if ($"{parameter}Desc" == sortOrder) {
				return viewModels.OrderByDescending(vm => vm.GetType().GetProperty(parameter)!.GetValue(vm, null));
			}
		}
		return viewModels.OrderBy(vm => vm.GetType().GetProperty(parameters[0]));
	}

	public async Task<IActionResult> GuideTeacher(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		if (await base.CheckSession() is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var parameters = new[] { nameof(ApplicationUserViewModel.FirstName), nameof(ApplicationUserViewModel.LastName), nameof(ApplicationUserViewModel.Rut), nameof(ApplicationUserViewModel.Email), nameof(ApplicationUserViewModel.TeacherOffice), nameof(ApplicationUserViewModel.TeacherSchedule), nameof(ApplicationUserViewModel.TeacherSpecialization) };
		this.SetSortParameters(sortOrder, parameters);
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		var users = (await this._userManager.GetUsersInRoleAsync(nameof(Roles.GuideTeacher)))
			.Where(u => !u.IsDeactivated)
			.Select(
				u => new ApplicationUserViewModel {
					Id = u.Id,
					FirstName = u.FirstName,
					LastName = u.LastName,
					Rut = u.Rut,
					Email = u.Email,
					IsDeactivated = u.IsDeactivated,
					TeacherOffice = u.TeacherOffice,
					TeacherSchedule = u.TeacherSchedule,
					TeacherSpecialization = u.TeacherSpecialization
				}
		).AsEnumerable();
		var ordered = this.Sort(sortOrder, users, parameters);
		var viewModels = !searchString.IsNullOrEmpty() ? this.Filter(searchString, ordered, parameters) : ordered;
		return this.View(PaginatedList<ApplicationUserViewModel>.Create(viewModels.AsQueryable(), pageNumber ?? 1, 6));
	}

	public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		if (await base.CheckSession() is not ApplicationUser user) {
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
			.Where(p => p.StudentOwnerOfTheStudentProposal == user)
			.Include(p => p.GuideTeacherOfTheStudentProposal).AsNoTracking()
			.Select(p => new StudentProposalViewModel {
				Id = p.Id,
				Title = p.Title,
				GuideTeacherName = $"{p.GuideTeacherOfTheStudentProposal!.FirstName} {p.GuideTeacherOfTheStudentProposal!.LastName}",
				ProposalStatus = p.ProposalStatus.ToString(),
			}).AsEnumerable();
		var ordered = this.Sort(sortOrder, proposals, parameters);
		var output = !searchString.IsNullOrEmpty() ? this.Filter(searchString, ordered!, parameters) : ordered;
		return this.View(PaginatedList<StudentProposalViewModel>.Create(output!.AsQueryable(), pageNumber ?? 1, 6));
	}

	public async Task<IActionResult> Create(string id) {
		if (await base.CheckSession() is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		if (await base.CheckApplicationUser(id) is not ApplicationUser guideTeacher) {
			this.TempData["ErrorMessage"] = "Error al obtener al profesor guía.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		await this.Populate(guideTeacher);
		var output = new StudentProposalViewModel {
			GuideTeacherId = guideTeacher.Id,
			GuideTeacherName = $"{guideTeacher.FirstName} {guideTeacher.LastName}",
			GuideTeacherEmail = guideTeacher.Email,
			GuideTeacherOffice = guideTeacher.TeacherOffice,
			GuideTeacherSchedule = guideTeacher.TeacherSchedule,
			GuideTeacherSpecialization = guideTeacher.TeacherSpecialization,
		};
		return this.View(output);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Create([FromForm] StudentProposalViewModel input) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		if (await base.CheckApplicationUser(input.GuideTeacherId!) is not ApplicationUser guideTeacher) {
			this.TempData["ErrorMessage"] = "Revisa tu selección del profesor guía.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		var assistantTeachers = input.AssistantTeachers!.Select(async at => await this.CheckApplicationUser(at)).Select(at => at.Result).ToList();
		var proposal = new StudentProposal {
			Id = Guid.NewGuid().ToString(),
			Title = input.Title,
			Description = input.Description,
			StudentOwnerOfTheStudentProposal = user,
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
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.StudentProposals!.AsNoTracking()
			.Include(p => p.StudentOwnerOfTheStudentProposal).AsNoTracking()
			.Where(p => p.StudentOwnerOfTheStudentProposal == user && p.ProposalStatus == StudentProposal.Status.Draft)
			.Include(p => p.GuideTeacherOfTheStudentProposal).AsNoTracking()
			.Include(p => p.AssistantTeachersOfTheStudentProposal).AsNoTracking()
			.FirstOrDefaultAsync(p => p.Id == id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		await this.Populate(proposal.GuideTeacherOfTheStudentProposal!);
		var output = new StudentProposalViewModel {
			Id = id,
			Title = proposal!.Title,
			Description = proposal.Description,
			GuideTeacherId = proposal.GuideTeacherOfTheStudentProposal!.Id,
			GuideTeacherName = $"{proposal.GuideTeacherOfTheStudentProposal!.FirstName} {proposal.GuideTeacherOfTheStudentProposal!.LastName}",
			AssistantTeachers = proposal.AssistantTeachersOfTheStudentProposal!.Select(at => at!.Id).ToList(),
			CreatedAt = proposal.CreatedAt,
			UpdatedAt = proposal.UpdatedAt
		};
		return this.View(output);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit([FromForm] StudentProposalViewModel input) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.StudentProposals!
			.Where(p => p.StudentOwnerOfTheStudentProposal == user && p.ProposalStatus == StudentProposal.Status.Draft)
			.Include(p => p.StudentOwnerOfTheStudentProposal)
			.Include(p => p.GuideTeacherOfTheStudentProposal)
			.Include(p => p.AssistantTeachersOfTheStudentProposal)
			.FirstOrDefaultAsync(p => p.Id == input.Id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		await this.Populate(proposal.GuideTeacherOfTheStudentProposal!);
		proposal.Title = input.Title;
		proposal.Description = input.Description;
		var assistantTeachers = input.AssistantTeachers!.Select(async at => await this.CheckApplicationUser(at)).Select(at => at.Result).ToList();
		proposal.AssistantTeachersOfTheStudentProposal = assistantTeachers!;
		proposal.UpdatedAt = DateTimeOffset.Now;
		_ = this._dbContext.StudentProposals!.Update(proposal);
		_ = await this._dbContext.SaveChangesAsync();
		var output = new StudentProposalViewModel {
			Id = proposal!.Id,
			Title = proposal!.Title,
			Description = proposal.Description,
			GuideTeacherId = proposal.GuideTeacherOfTheStudentProposal!.Id,
			GuideTeacherName = $"{proposal.GuideTeacherOfTheStudentProposal!.FirstName} {proposal.GuideTeacherOfTheStudentProposal!.LastName}",
			AssistantTeachers = proposal.AssistantTeachersOfTheStudentProposal!.Select(at => at!.Id).ToList(),
			CreatedAt = proposal.CreatedAt,
			UpdatedAt = proposal.UpdatedAt
		};
		this.ViewBag.SuccessMessage = "Tu propuesta ha sido actualizada correctamente.";
		return this.View(output);
	}

	public async Task<IActionResult> Delete(string id) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.StudentProposals!.AsNoTracking()
			.Where(p => p.StudentOwnerOfTheStudentProposal == user && p.ProposalStatus == StudentProposal.Status.Draft)
			.FirstOrDefaultAsync(p => p.Id == id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		var output = new StudentProposalViewModel {
			Id = id,
			Title = proposal.Title
		};
		return this.View(output);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete([FromForm] StudentProposalViewModel input) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.StudentProposals!
			.Where(p => p.StudentOwnerOfTheStudentProposal == user && p.ProposalStatus == StudentProposal.Status.Draft)
			.FirstOrDefaultAsync(p => p.Id == input.Id);
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
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.StudentProposals!.AsNoTracking()
			.Where(p => p.StudentOwnerOfTheStudentProposal == user && p.ProposalStatus == StudentProposal.Status.Draft)
			.Include(p => p.GuideTeacherOfTheStudentProposal)
			.FirstOrDefaultAsync(p => p.Id == id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		var output = new StudentProposalViewModel {
			Id = id,
			Title = proposal.Title,
			GuideTeacherName = $"{proposal.GuideTeacherOfTheStudentProposal!.FirstName} {proposal.GuideTeacherOfTheStudentProposal!.LastName}"
		};
		return this.View(output);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Send([FromForm] StudentProposalViewModel input) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.StudentProposals!
			.Where(p => p.StudentOwnerOfTheStudentProposal == user && p.ProposalStatus == StudentProposal.Status.Draft)
			.Include(p => p.GuideTeacherOfTheStudentProposal)
			.Include(p => p.AssistantTeachersOfTheStudentProposal)
			.FirstOrDefaultAsync(p => p.Id == input.Id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		if (await base.CheckApplicationUser(proposal.GuideTeacherOfTheStudentProposal!.Id) is null) {
			this.TempData["ErrorMessage"] = "Error al obtener el profesor guía.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		foreach (var assistantTeacher in proposal.AssistantTeachersOfTheStudentProposal!) {
			if (await base.CheckApplicationUser(assistantTeacher!.Id) is null) {
				this.TempData["ErrorMessage"] = "Error al obtener el profesor co-guía.";
				return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
			}
		}
		proposal.ProposalStatus = StudentProposal.Status.SentToGuideTeacher;
		proposal.UpdatedAt = DateTimeOffset.Now;
		_ = this._dbContext.StudentProposals!.Update(proposal);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "Tu propuesta ha sido enviada correctamente.";
		return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
	}

	public async Task<IActionResult> ViewRejectionReason(string id) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.StudentProposals!.AsNoTracking()
			.Where(p => p.StudentOwnerOfTheStudentProposal == user && p.ProposalStatus == StudentProposal.Status.RejectedByGuideTeacher)
			.Include(p => p.GuideTeacherWhoRejectedThisStudentProposal)
			.FirstOrDefaultAsync(p => p.Id == id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		var output = new StudentProposalViewModel {
			Id = id,
			Title = proposal.Title,
			Description = proposal.Description,
			WhoRejected = $"{proposal.GuideTeacherWhoRejectedThisStudentProposal!.FirstName} {proposal.GuideTeacherWhoRejectedThisStudentProposal!.LastName}", // TODO It's not just the guide teacher who can reject a proposal
			RejectionReason = proposal.RejectionReason,
			CreatedAt = proposal.CreatedAt,
			UpdatedAt = proposal.UpdatedAt
		};
		return this.View(output);
	}

	public async Task<IActionResult> Convert(string id) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.StudentProposals!.AsNoTracking()
			.Where(p => p.StudentOwnerOfTheStudentProposal == user && p.ProposalStatus == StudentProposal.Status.ApprovedByGuideTeacher)
			.Include(p => p.GuideTeacherOfTheStudentProposal).AsNoTracking()
			.Include(p => p.AssistantTeachersOfTheStudentProposal).AsNoTracking()
			.FirstOrDefaultAsync(p => p.Id == id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		var output = new StudentProposalViewModel {
			Id = id,
			Title = proposal.Title,
			Description = proposal.Description,
			GuideTeacherName = $"{proposal.GuideTeacherOfTheStudentProposal!.FirstName} {proposal.GuideTeacherOfTheStudentProposal!.LastName}",
			GuideTeacherEmail = proposal.GuideTeacherOfTheStudentProposal!.Email,
			GuideTeacherOffice = proposal.GuideTeacherOfTheStudentProposal!.TeacherOffice,
			GuideTeacherSchedule = proposal.GuideTeacherOfTheStudentProposal!.TeacherSchedule,
			GuideTeacherSpecialization = proposal.GuideTeacherOfTheStudentProposal!.TeacherSpecialization,
			AssistantTeachers = proposal.AssistantTeachersOfTheStudentProposal!.Select(at => $"{at!.FirstName} {at!.LastName}").ToList(),
			CreatedAt = proposal.CreatedAt,
			UpdatedAt = proposal.UpdatedAt
		};
		return this.View(output);
	}
}
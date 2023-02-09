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
using static Utal.Icc.Sgm.Models.GuideTeacherProposal;

namespace Utal.Icc.Sgm.Areas.GuideTeacher.Controllers;

[Area(nameof(GuideTeacher)), Authorize(Roles = nameof(Roles.GuideTeacher))]
public class GuideTeacherProposalController : ApplicationController {
	public GuideTeacherProposalController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore, SignInManager<ApplicationUser> signInManager) : base(dbContext, userManager, userStore, signInManager) { }

	protected async Task PopulateAssistantTeachers(ApplicationUser guideTeacher) {
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

	protected IEnumerable<ApplicationUserViewModel> Filter(string searchString, IOrderedEnumerable<ApplicationUserViewModel> viewModels, params string[] parameters) {
		var result = new List<ApplicationUserViewModel>();
		foreach (var parameter in parameters) {
			var partials = viewModels
					.Where(vm => !(vm.GetType().GetProperty(parameter)!.GetValue(vm, null) as string)!.IsNullOrEmpty() && (vm.GetType().GetProperty(parameter)!.GetValue(vm, null) as string)!.Contains(searchString));
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

	public async Task<IActionResult> Students(string id, string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		this.ViewData["Proposal"] = await this._dbContext.GuideTeacherProposals!
			.Include(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal)
			.Where(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal == user && p.ProposalStatus == GuideTeacherProposal.Status.Published)
			.FirstOrDefaultAsync(p => p.Id == id);
		var parameters = new[] { nameof(ApplicationUserViewModel.FirstName), nameof(ApplicationUserViewModel.LastName), nameof(ApplicationUserViewModel.StudentUniversityId), nameof(ApplicationUserViewModel.Rut), nameof(ApplicationUserViewModel.Email) };
		base.SetSortParameters(sortOrder, parameters);
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		var users = this._dbContext.GuideTeacherProposals!
			.Where(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal == user && p.Id == id)
			.Include(p => p.StudentsWhoAreInterestedInThisGuideTeacherProposal)
			.SelectMany(p => p.StudentsWhoAreInterestedInThisGuideTeacherProposal!.Select(
				u => new ApplicationUserViewModel {
					Id = u!.Id,
					FirstName = u.FirstName,
					LastName = u.LastName,
					StudentUniversityId = u.StudentUniversityId,
					Rut = u.Rut,
					Email = u.Email,
					IsDeactivated = u.IsDeactivated
				}
			)).AsEnumerable();
		var ordered = this.Sort(sortOrder, users, parameters);
		var output = !searchString.IsNullOrEmpty() ? this.Filter(searchString, ordered, parameters) : ordered;
		return this.View(PaginatedList<ApplicationUserViewModel>.Create(output.AsQueryable(), pageNumber ?? 1, 6));
	}

	public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var parameters = new[] { nameof(GuideTeacherProposalViewModel.Title) };
		base.SetSortParameters(sortOrder, parameters);
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		var proposals = this._dbContext.GuideTeacherProposals!.AsNoTracking()
			.Where(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal == user)
			.Select(p => new GuideTeacherProposalViewModel {
				Id = p.Id,
				Title = p.Title,
				ProposalStatus = p.ProposalStatus.ToString(),
			}).AsEnumerable();
		var ordered = this.Sort(sortOrder, proposals, parameters);
		var output = !searchString.IsNullOrEmpty() ? this.Filter(searchString, ordered!, parameters) : ordered;
		return this.View(PaginatedList<GuideTeacherProposalViewModel>.Create(output!.AsQueryable(), pageNumber ?? 1, 6));
	}

	public async Task<IActionResult> Create() {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		await this.PopulateAssistantTeachers(user);
		var output = new GuideTeacherProposalViewModel {
			GuideTeacherId = user.Id,
			GuideTeacherName = $"{user.FirstName} {user.LastName}",
			GuideTeacherEmail = user.Email,
			GuideTeacherOffice = user.TeacherOffice,
			GuideTeacherSchedule = user.TeacherSchedule,
			GuideTeacherSpecialization = user.TeacherSpecialization,
		};
		return this.View(output);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Create([FromForm] GuideTeacherProposalViewModel input) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var assistantTeachers = input.AssistantTeachers!.Select(async at => await this.CheckApplicationUser(at)).Select(at => at.Result).ToList();
		var proposal = new GuideTeacherProposal {
			Id = Guid.NewGuid().ToString(),
			Title = input.Title,
			Description = input.Description,
			Requirements = input.Requirements,
			GuideTeacherOwnerOfTheGuideTeacherProposal = user,
			AssistantTeachersOfTheGuideTeacherProposal = assistantTeachers!,
			ProposalStatus = GuideTeacherProposal.Status.Draft,
			CreatedAt = DateTimeOffset.Now,
			UpdatedAt = DateTimeOffset.Now
		};
		_ = await this._dbContext.GuideTeacherProposals!.AddAsync(proposal);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "Tu propuesta ha sido registrada correctamente.";
		return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
	}

	public async Task<IActionResult> Edit(string id) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.GuideTeacherProposals!.AsNoTracking()
			.Include(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal).AsNoTracking()
			.Where(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal == user && p.ProposalStatus == GuideTeacherProposal.Status.Draft)
			.Include(p => p.AssistantTeachersOfTheGuideTeacherProposal).AsNoTracking()
			.FirstOrDefaultAsync(p => p.Id == id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		await this.PopulateAssistantTeachers(proposal.GuideTeacherOwnerOfTheGuideTeacherProposal!);
		var output = new GuideTeacherProposalViewModel {
			Id = id,
			Title = proposal!.Title,
			Description = proposal.Description,
			Requirements = proposal.Requirements,
			AssistantTeachers = proposal.AssistantTeachersOfTheGuideTeacherProposal!.Select(at => at!.Id).ToList(),
			CreatedAt = proposal.CreatedAt,
			UpdatedAt = proposal.UpdatedAt
		};
		return this.View(output);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit([FromForm] GuideTeacherProposalViewModel input) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.GuideTeacherProposals!
			.Include(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal)
			.Where(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal == user && p.ProposalStatus == GuideTeacherProposal.Status.Draft)
			.Include(p => p.AssistantTeachersOfTheGuideTeacherProposal)
			.FirstOrDefaultAsync(p => p.Id == input.Id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		await this.PopulateAssistantTeachers(proposal.GuideTeacherOwnerOfTheGuideTeacherProposal!);
		proposal.Title = input.Title;
		proposal.Description = input.Description;
		proposal.Requirements = input.Requirements;
		var assistantTeachers = input.AssistantTeachers!.Select(async at => await this.CheckApplicationUser(at)).Select(at => at.Result).ToList();
		proposal.AssistantTeachersOfTheGuideTeacherProposal = assistantTeachers!;
		proposal.UpdatedAt = DateTimeOffset.Now;
		_ = this._dbContext.GuideTeacherProposals!.Update(proposal);
		_ = await this._dbContext.SaveChangesAsync();
		var output = new GuideTeacherProposalViewModel {
			Id = proposal!.Id,
			Title = proposal!.Title,
			Description = proposal.Description,
			Requirements = proposal.Requirements,
			AssistantTeachers = proposal.AssistantTeachersOfTheGuideTeacherProposal!.Select(at => at!.Id).ToList(),
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
		var proposal = await this._dbContext.GuideTeacherProposals!.AsNoTracking()
			.Where(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal == user && p.ProposalStatus == GuideTeacherProposal.Status.Draft)
			.FirstOrDefaultAsync(p => p.Id == id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		var output = new GuideTeacherProposalViewModel {
			Id = id,
			Title = proposal.Title
		};
		return this.View(output);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete([FromForm] GuideTeacherProposalViewModel input) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.GuideTeacherProposals!
			.Where(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal == user && p.ProposalStatus == GuideTeacherProposal.Status.Draft)
			.FirstOrDefaultAsync(p => p.Id == input.Id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		_ = this._dbContext.GuideTeacherProposals!.Remove(proposal);
		_ = this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "Tu propuesta ha sido eliminada correctamente.";
		return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
	}

	public async Task<IActionResult> Publish(string id) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.GuideTeacherProposals!.AsNoTracking()
			.Where(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal == user && p.ProposalStatus == GuideTeacherProposal.Status.Draft)
			.FirstOrDefaultAsync(p => p.Id == id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		var output = new GuideTeacherProposalViewModel {
			Id = id,
			Title = proposal.Title,
		};
		return this.View(output);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Publish([FromForm] GuideTeacherProposalViewModel input) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.GuideTeacherProposals!
			.Where(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal == user && p.ProposalStatus == GuideTeacherProposal.Status.Draft)
			.Include(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal)
			.Include(p => p.AssistantTeachersOfTheGuideTeacherProposal)
			.FirstOrDefaultAsync(p => p.Id == input.Id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		if (await base.CheckApplicationUser(proposal.GuideTeacherOwnerOfTheGuideTeacherProposal!.Id) is null) {
			this.TempData["ErrorMessage"] = "Error al obtener el profesor guía.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		foreach (var assistantTeacher in proposal.AssistantTeachersOfTheGuideTeacherProposal!) {
			if (await base.CheckApplicationUser(assistantTeacher!.Id) is null) {
				this.TempData["ErrorMessage"] = "Error al obtener el profesor co-guía.";
				return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
			}
		}
		proposal.ProposalStatus = GuideTeacherProposal.Status.Published;
		proposal.UpdatedAt = DateTimeOffset.Now;
		_ = this._dbContext.GuideTeacherProposals!.Update(proposal);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "Tu propuesta ha sido enviada correctamente.";
		return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
	}

	public async Task<IActionResult> Select(string proposalId, string studentId) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		if (await base.CheckApplicationUser(studentId) is not ApplicationUser student) {
			this.TempData["ErrorMessage"] = "Error al obtener el estudiante.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		var proposal = await this._dbContext.GuideTeacherProposals!.AsNoTracking()
			.Where(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal == user && p.ProposalStatus == GuideTeacherProposal.Status.Published)
			.Include(p => p.StudentsWhoAreInterestedInThisGuideTeacherProposal).AsNoTracking()
			.Where(p => p.StudentsWhoAreInterestedInThisGuideTeacherProposal!.Any(s => s!.Id == studentId))
			.FirstOrDefaultAsync(p => p.Id == proposalId);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		var output = new GuideTeacherProposalViewModel {
			Id = proposalId,
			StudentName = $"{student.FirstName} {student.LastName}",
			StudentId = studentId,
			StudentEmail = student.Email,
			StudentRemainingCourses = student.StudentRemainingCourses,
			StudentIsDoingThePractice = student.StudentIsDoingThePractice,
			StudentIsWorking = student.StudentIsWorking
		};
		return this.View(output);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Select([FromForm] GuideTeacherProposalViewModel input) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		if (await base.CheckApplicationUser(input.StudentId!) is not ApplicationUser student) {
			this.TempData["ErrorMessage"] = "Error al obtener el estudiante.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		var proposal = await this._dbContext.GuideTeacherProposals!
			.Where(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal == user && p.ProposalStatus == GuideTeacherProposal.Status.Published)
			.Include(p => p.StudentsWhoAreInterestedInThisGuideTeacherProposal)
			.Where(p => p.StudentsWhoAreInterestedInThisGuideTeacherProposal!.Any(s => s!.Id == input.StudentId!))
			.FirstOrDefaultAsync(p => p.Id == input.Id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		if (proposal.StudentsWhoAreInterestedInThisGuideTeacherProposal!.Any(s => s!.Id == student.Id)) {
			_ = proposal.StudentsWhoAreInterestedInThisGuideTeacherProposal!.Remove(student);
		}
		proposal.ProposalStatus = GuideTeacherProposal.Status.Ready;
		proposal.StudentWhoIsAssignedToThisGuideTeacherProposal = student;
		proposal.UpdatedAt = DateTimeOffset.Now;
		_ = this._dbContext.GuideTeacherProposals!.Update(proposal);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "El estudiante ha sido seleccionado a la propuesta correctamente.";
		return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
	}

	public async Task<IActionResult> Convert(string id) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.GuideTeacherProposals!.AsNoTracking()
			.Where(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal == user && p.ProposalStatus == GuideTeacherProposal.Status.Ready)
			.Include(p => p.StudentWhoIsAssignedToThisGuideTeacherProposal).AsNoTracking()
			.Include(p => p.AssistantTeachersOfTheGuideTeacherProposal).AsNoTracking()
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
			StudentName = $"{proposal.StudentWhoIsAssignedToThisGuideTeacherProposal!.FirstName} {proposal.StudentWhoIsAssignedToThisGuideTeacherProposal!.LastName}",
			StudentEmail = proposal.StudentWhoIsAssignedToThisGuideTeacherProposal!.Email,
			StudentRemainingCourses = proposal.StudentWhoIsAssignedToThisGuideTeacherProposal!.StudentRemainingCourses,
			StudentIsDoingThePractice = proposal.StudentWhoIsAssignedToThisGuideTeacherProposal!.StudentIsDoingThePractice,
			StudentIsWorking = proposal.StudentWhoIsAssignedToThisGuideTeacherProposal!.StudentIsWorking,
			AssistantTeachers = proposal.AssistantTeachersOfTheGuideTeacherProposal!.Select(at => $"{at!.FirstName} {at!.LastName}").ToList(),
			CreatedAt = proposal.CreatedAt,
			UpdatedAt = proposal.UpdatedAt
		};
		return this.View(output);
	}
	public new async Task<IActionResult> View(string id) {
		if (await base.CheckSession() is not ApplicationUser user) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var proposal = await this._dbContext.GuideTeacherProposals!
			.Include(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal)
			.Where(p => p.GuideTeacherOwnerOfTheGuideTeacherProposal == user && p.ProposalStatus == GuideTeacherProposal.Status.Published)
			.Include(p => p.AssistantTeachersOfTheGuideTeacherProposal)
			.FirstOrDefaultAsync(p => p.Id == id);
		if (proposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		var output = new GuideTeacherProposalViewModel {
			Id = id,
			Title = proposal.Title,
			Description = proposal.Description,
			Requirements = proposal.Requirements,
			AssistantTeachers = proposal.AssistantTeachersOfTheGuideTeacherProposal!.Select(at => $"{at!.FirstName} {at!.LastName}").ToList(),
			CreatedAt = proposal.CreatedAt,
			UpdatedAt = proposal.UpdatedAt,
		};
		return this.View(output);
	}
}
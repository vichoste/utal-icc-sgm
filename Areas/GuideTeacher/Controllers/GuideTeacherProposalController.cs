using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Utal.Icc.Sgm.Areas.GuideTeacher.Views.GuideTeacherProposal;
using Utal.Icc.Sgm.Controllers;
using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;

using static Utal.Icc.Sgm.Models.ApplicationUser;

namespace Utal.Icc.Sgm.Areas.GuideTeacher.Controllers;

[Area(nameof(Student)), Authorize(Roles = nameof(Roles.Student))]
public class GuideTeacherProposalController : Controller {
	private readonly ApplicationDbContext _dbContext;
	private readonly UserManager<ApplicationUser> _userManager;

	public GuideTeacherProposalController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager) {
		this._dbContext = dbContext;
		this._userManager = userManager;
	}

	protected async Task<ApplicationUser> CheckGuideTeacherSession() {
		var student = await this._userManager.GetUserAsync(this.User);
		return student is null || student.IsDeactivated ? null! : student;
	}

	protected async Task<ApplicationUser> CheckApplicationUser(string applicationUserId) {
		var applicationUser = await this._userManager.FindByIdAsync(applicationUserId);
		return applicationUser is null || applicationUser.IsDeactivated ? null! : applicationUser;
	}

	protected void SetSortParameters(string sortOrder, params string[] parameters) {
		foreach (var parameter in parameters) {
			this.ViewData[$"{parameter}SortParam"] = sortOrder == parameter ? $"{parameter}Desc" : parameter;
		}
		this.ViewData["CurrentSort"] = sortOrder;
	}

	protected IOrderedEnumerable<IndexViewModel> OrderProposals(string sortOrder, IEnumerable<IndexViewModel> indexViewModels, params string[] parameters) {
		foreach (var parameter in parameters) {
			if (parameter == sortOrder) {
				return indexViewModels.OrderBy(gtp => gtp.GetType().GetProperty(parameter)!.GetValue(gtp, null));
			} else if ($"{parameter}Desc" == sortOrder) {
				return indexViewModels.OrderByDescending(gtp => gtp.GetType().GetProperty(parameter)!.GetValue(gtp, null));
			}
		}
		return indexViewModels.OrderBy(gtp => gtp.GetType().GetProperty(parameters[0]));
	}

	protected IEnumerable<IndexViewModel> FilterProposals(string searchString, IEnumerable<IndexViewModel> indexViewModels, params string[] parameters) {
		var result = new List<IndexViewModel>();
		foreach (var parameter in parameters) {
			var partials = indexViewModels
					.Where(gtp => (gtp.GetType().GetProperty(parameter)!.GetValue(gtp) as string)!.Contains(searchString));
			foreach (var partial in partials) {
				if (!result.Any(ivm => ivm.Id == partial.Id)) {
					result.Add(partial);
				}
			}
		}
		return result.AsEnumerable();
	}

	protected async Task PopulateTeachers() {
		var assistantTeachers1 = (
			await this._userManager.GetUsersInRoleAsync(nameof(Roles.AssistantTeacher)))
				.Where(gt => !gt.IsDeactivated)
				.OrderBy(at => at.LastName)
				.ToList();
		var assistantTeachers2 = (
			await this._userManager.GetUsersInRoleAsync(nameof(Roles.AssistantTeacher)))
				.Where(gt => !gt.IsDeactivated)
				.OrderBy(at => at.LastName)
				.ToList();
		var assistantTeachers3 = (
			await this._userManager.GetUsersInRoleAsync(nameof(Roles.AssistantTeacher)))
				.Where(gt => !gt.IsDeactivated)
				.OrderBy(at => at.LastName)
			.ToList();
		this.ViewData[$"{nameof(Roles.AssistantTeacher)}s1"] = assistantTeachers1.Select(at => new SelectListItem {
			Text = $"{at.FirstName} {at.LastName}",
			Value = at.Id
		});
		this.ViewData[$"{nameof(Roles.AssistantTeacher)}s2"] = assistantTeachers2.Select(at => new SelectListItem {
			Text = $"{at.FirstName} {at.LastName}",
			Value = at.Id
		});
		this.ViewData[$"{nameof(Roles.AssistantTeacher)}s3"] = assistantTeachers3.Select(at => new SelectListItem {
			Text = $"{at.FirstName} {at.LastName}",
			Value = at.Id
		});
	}

	public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		if (await this.CheckGuideTeacherSession() is not ApplicationUser guideTeacher) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var parameters = new[] { nameof(GuideTeacherProposal.Title), nameof(Roles.GuideTeacher) };
		this.SetSortParameters(sortOrder, parameters);
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		var guideTeacherProposals = this._dbContext.GuideTeacherProposals!.AsNoTracking()
			.Where(gtp => gtp.GuideTeacherOwnerOfTheGuideTeacherProposal == guideTeacher)
			.Select(gtp => new IndexViewModel {
				Id = gtp.Id,
				Title = gtp.Title,
				ProposalStatus = gtp.ProposalStatus.ToString(),
			}).AsEnumerable();
		var orderedProposals = this.OrderProposals(sortOrder, guideTeacherProposals, parameters);
		var filteredAndOrderedProposals = !searchString.IsNullOrEmpty() ?
			this.FilterProposals(searchString, orderedProposals, parameters)
			: orderedProposals;
		return this.View(PaginatedList<IndexViewModel>.Create(filteredAndOrderedProposals.AsQueryable(), pageNumber ?? 1, 6));
	}

	public async Task<IActionResult> Create() {
		if (await this.CheckGuideTeacherSession() is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		await this.PopulateTeachers();
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
		if (await this.CheckGuideTeacherSession() is not ApplicationUser guideTeacher) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		await this.PopulateTeachers();
		var assistantTeacher1 = !model.AssistantTeacher1.IsNullOrEmpty() ? await this.CheckApplicationUser(model.AssistantTeacher1!) : null;
		var assistantTeacher2 = !model.AssistantTeacher2.IsNullOrEmpty() ? await this.CheckApplicationUser(model.AssistantTeacher2!) : null;
		var assistantTeacher3 = !model.AssistantTeacher3.IsNullOrEmpty() ? await this.CheckApplicationUser(model.AssistantTeacher3!) : null;
		var teacherCheck = (assistantTeacher1, assistantTeacher2, assistantTeacher3) switch {
			(ApplicationUser at1, ApplicationUser at2, ApplicationUser at3) => at1 != at2 && at1 != at3 && at2 != at3,
			(ApplicationUser at1, ApplicationUser at2, null) => at1 != at2,
			_ => true
		};
		if (!teacherCheck) {
			this.ViewBag.WarningMessage = "Revisa tu selección de profesores.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		var guideTeacherProposal = new GuideTeacherProposal {
			Id = Guid.NewGuid().ToString(),
			Title = model.Title,
			Description = model.Description,
			GuideTeacherOwnerOfTheGuideTeacherProposal = guideTeacher,
			AssistantTeacher1OfTheGuideTeacherProposal = assistantTeacher1,
			AssistantTeacher2OfTheGuideTeacherProposal = assistantTeacher2,
			AssistantTeacher3OfTheGuideTeacherProposal = assistantTeacher3,
			ProposalStatus = GuideTeacherProposal.Status.Draft,
			CreatedAt = DateTimeOffset.Now,
			UpdatedAt = DateTimeOffset.Now
		};
		_ = await this._dbContext.GuideTeacherProposals!.AddAsync(guideTeacherProposal);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "Tu propuesta ha sido registrada correctamente.";
		return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
	}

	public async Task<IActionResult> Edit(string id) {
		if (await this.CheckGuideTeacherSession() is not ApplicationUser guideTeacher) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		await this.PopulateTeachers();
		var guideTeacherProposal = await this._dbContext.GuideTeacherProposals!.AsNoTracking()
			.Where(gtp => gtp.GuideTeacherOwnerOfTheGuideTeacherProposal == guideTeacher && gtp.ProposalStatus == GuideTeacherProposal.Status.Draft)
			.Include(gtp => gtp.AssistantTeacher1OfTheGuideTeacherProposal).AsNoTracking()
			.Include(gtp => gtp.AssistantTeacher2OfTheGuideTeacherProposal).AsNoTracking()
			.Include(gtp => gtp.AssistantTeacher3OfTheGuideTeacherProposal).AsNoTracking()
			.FirstOrDefaultAsync(gtp => gtp.Id == id);
		if (guideTeacherProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		var editViewModel = new EditViewModel {
			Id = id,
			Title = guideTeacherProposal!.Title,
			Description = guideTeacherProposal.Description,
			AssistantTeacher1 = guideTeacherProposal.AssistantTeacher1OfTheGuideTeacherProposal?.Id,
			AssistantTeacher2 = guideTeacherProposal.AssistantTeacher2OfTheGuideTeacherProposal?.Id,
			AssistantTeacher3 = guideTeacherProposal.AssistantTeacher3OfTheGuideTeacherProposal?.Id,
			CreatedAt = guideTeacherProposal.CreatedAt,
			UpdatedAt = guideTeacherProposal.UpdatedAt
		};
		return this.View(editViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit([FromForm] EditViewModel model) {
		if (!this.ModelState.IsValid) {
			this.ViewBag.WarningMessage = "Revisa que los campos estén correctos.";
			return this.View(new EditViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		if (await this.CheckGuideTeacherSession() is not ApplicationUser guideTeacher) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		await this.PopulateTeachers();
		var guideTeacherProposal = await this._dbContext.GuideTeacherProposals!.AsNoTracking()
			.Where(gtp => gtp.GuideTeacherOwnerOfTheGuideTeacherProposal == guideTeacher && gtp.ProposalStatus == GuideTeacherProposal.Status.Draft)
			.Include(gtp => gtp.AssistantTeacher1OfTheGuideTeacherProposal).AsNoTracking()
			.Include(gtp => gtp.AssistantTeacher2OfTheGuideTeacherProposal).AsNoTracking()
			.Include(gtp => gtp.AssistantTeacher3OfTheGuideTeacherProposal).AsNoTracking()
			.FirstOrDefaultAsync(gtp => gtp.Id == model.Id);
		if (guideTeacherProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		if (guideTeacherProposal.ProposalStatus != GuideTeacherProposal.Status.Draft) {
			this.ViewBag.ErrorMessage = "La propuesta no puede ser editada ya que no es un borrador.";
			return this.View(new EditViewModel {
				Id = model.Id,
				Title = model.Title,
				Description = model.Description,
				CreatedAt = guideTeacherProposal.CreatedAt,
				UpdatedAt = guideTeacherProposal.UpdatedAt
			});
		}
		var assistantTeacher1 = !model.AssistantTeacher1.IsNullOrEmpty() ? await this.CheckApplicationUser(model.AssistantTeacher1!) : null;
		var assistantTeacher2 = !model.AssistantTeacher2.IsNullOrEmpty() ? await this.CheckApplicationUser(model.AssistantTeacher2!) : null;
		var assistantTeacher3 = !model.AssistantTeacher3.IsNullOrEmpty() ? await this.CheckApplicationUser(model.AssistantTeacher3!) : null;
		var teacherCheck = (assistantTeacher1, assistantTeacher2, assistantTeacher3) switch {
			(ApplicationUser at1, ApplicationUser at2, ApplicationUser at3) => at1 != at2 && at1 != at3 && at2 != at3,
			(ApplicationUser at1, ApplicationUser at2, null) => at1 != at2,
			_ => true
		};
		if (!teacherCheck) {
			this.ViewBag.WarningMessage = "Revisa tu selección de profesores.";
			return this.View(new EditViewModel {
				Id = model.Id,
				Title = model.Title,
				Description = model.Description,
				CreatedAt = guideTeacherProposal.CreatedAt,
				UpdatedAt = guideTeacherProposal.UpdatedAt
			});
		}
		guideTeacherProposal.Title = model.Title;
		guideTeacherProposal.Description = model.Description;
		guideTeacherProposal.AssistantTeacher1OfTheGuideTeacherProposal = assistantTeacher1;
		guideTeacherProposal.AssistantTeacher2OfTheGuideTeacherProposal = assistantTeacher2;
		guideTeacherProposal.AssistantTeacher3OfTheGuideTeacherProposal = assistantTeacher3;
		guideTeacherProposal.UpdatedAt = DateTimeOffset.Now;
		_ = this._dbContext.GuideTeacherProposals!.Update(guideTeacherProposal);
		_ = await this._dbContext.SaveChangesAsync();
		var editViewModel = new EditViewModel {
			Id = guideTeacherProposal.Id,
			Title = guideTeacherProposal!.Title,
			Description = guideTeacherProposal.Description,
			AssistantTeacher1 = guideTeacherProposal.AssistantTeacher1OfTheGuideTeacherProposal?.Id,
			AssistantTeacher2 = guideTeacherProposal.AssistantTeacher2OfTheGuideTeacherProposal?.Id,
			AssistantTeacher3 = guideTeacherProposal.AssistantTeacher3OfTheGuideTeacherProposal?.Id,
			CreatedAt = guideTeacherProposal.CreatedAt,
			UpdatedAt = guideTeacherProposal.UpdatedAt
		};
		this.ViewBag.SuccessMessage = "Tu propuesta ha sido actualizada correctamente.";
		return this.View(editViewModel);
	}

	public async Task<IActionResult> Delete(string id) {
		if (await this.CheckGuideTeacherSession() is not ApplicationUser guideTeacher) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var guideTeacherProposal = await this._dbContext.GuideTeacherProposals!.AsNoTracking()
			.Where(gtp => gtp.GuideTeacherOwnerOfTheGuideTeacherProposal == guideTeacher && gtp.ProposalStatus == GuideTeacherProposal.Status.Draft)
			.FirstOrDefaultAsync(gtp => gtp.Id == id);
		if (guideTeacherProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		var deleteViewModel = new DeleteViewModel {
			Id = id,
			Title = guideTeacherProposal.Title
		};
		return this.View(deleteViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete([FromForm] DeleteViewModel model) {
		if (await this.CheckGuideTeacherSession() is not ApplicationUser guideTeacher) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var guideTeacherProposal = await this._dbContext.GuideTeacherProposals!.AsNoTracking()
			.Where(gtp => gtp.GuideTeacherOwnerOfTheGuideTeacherProposal == guideTeacher && gtp.ProposalStatus == GuideTeacherProposal.Status.Draft)
			.FirstOrDefaultAsync(gtp => gtp.Id == model.Id);
		if (guideTeacherProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		_ = this._dbContext.GuideTeacherProposals!.Remove(guideTeacherProposal);
		_ = this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "Tu propuesta ha sido eliminada correctamente.";
		return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
	}

	public async Task<IActionResult> Publish(string id) {
		if (await this.CheckGuideTeacherSession() is not ApplicationUser guideTeacher) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var guideTeacherProposal = await this._dbContext.GuideTeacherProposals!.AsNoTracking()
			.Where(gtp => gtp.GuideTeacherOwnerOfTheGuideTeacherProposal == guideTeacher && gtp.ProposalStatus == GuideTeacherProposal.Status.Draft)
			.FirstOrDefaultAsync(gtp => gtp.Id == id);
		if (guideTeacherProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		var PublishViewModel = new PublishViewModel {
			Id = id,
			Title = guideTeacherProposal.Title
		};
		return this.View(PublishViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Publish([FromForm] PublishViewModel model) {
		if (await this.CheckGuideTeacherSession() is not ApplicationUser guideTeacher) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var guideTeacherProposal = await this._dbContext.GuideTeacherProposals!
			.Where(gtp => gtp.GuideTeacherOwnerOfTheGuideTeacherProposal == guideTeacher && gtp.ProposalStatus == GuideTeacherProposal.Status.Draft)
			.Include(gtp => gtp.AssistantTeacher1OfTheGuideTeacherProposal)
			.Include(gtp => gtp.AssistantTeacher2OfTheGuideTeacherProposal)
			.Include(gtp => gtp.AssistantTeacher3OfTheGuideTeacherProposal)
			.FirstOrDefaultAsync(gtp => gtp.Id == model.Id);
		if (guideTeacherProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		var teacherCheck = (guideTeacherProposal.AssistantTeacher1OfTheGuideTeacherProposal, guideTeacherProposal.AssistantTeacher2OfTheGuideTeacherProposal, guideTeacherProposal.AssistantTeacher3OfTheGuideTeacherProposal) switch {
			(ApplicationUser teacher, _, _) when teacher.IsDeactivated => "El profesor co-guía 1 está desactivado.",
			(_, ApplicationUser teacher, _) when teacher.IsDeactivated => "El profesor co-guía 2 está desactivado.",
			(_, _, ApplicationUser teacher) when teacher.IsDeactivated => "El profesor co-guía 3 está desactivado.",
			_ => null
		};
		if (teacherCheck is not null) {
			this.TempData["ErrorMessage"] = teacherCheck;
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		guideTeacherProposal.ProposalStatus = GuideTeacherProposal.Status.Published;
		guideTeacherProposal.UpdatedAt = DateTimeOffset.Now;
		_ = this._dbContext.GuideTeacherProposals!.Update(guideTeacherProposal);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "Tu propuesta ha sido enviada correctamente.";
		return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
	}

	public async Task<IActionResult> Convert(string id) {
		if (await this.CheckGuideTeacherSession() is not ApplicationUser guideTeacher) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var guideTeacherProposal = await this._dbContext.GuideTeacherProposals!.AsNoTracking()
			.Where(gtp => gtp.GuideTeacherOwnerOfTheGuideTeacherProposal == guideTeacher && gtp.ProposalStatus == GuideTeacherProposal.Status.Ready)
			.Include(gtp => gtp.StudentWhichIsAssignedToThisGuideTeacherProposal).AsNoTracking()
			.Include(gtp => gtp.AssistantTeacher1OfTheGuideTeacherProposal).AsNoTracking()
			.Include(gtp => gtp.AssistantTeacher2OfTheGuideTeacherProposal).AsNoTracking()
			.Include(gtp => gtp.AssistantTeacher3OfTheGuideTeacherProposal).AsNoTracking()
			.FirstOrDefaultAsync(gtp => gtp.Id == id);
		if (guideTeacherProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(GuideTeacher) });
		}
		var convertViewModel = new ConvertViewModel {
			Id = id,
			Title = guideTeacherProposal.Title,
			Description = guideTeacherProposal.Description,
			StudentName = $"{guideTeacherProposal.StudentWhichIsAssignedToThisGuideTeacherProposal!.FirstName} {guideTeacherProposal.StudentWhichIsAssignedToThisGuideTeacherProposal!.LastName}",
			StudentEmail = guideTeacherProposal.StudentWhichIsAssignedToThisGuideTeacherProposal!.Email,
			StudentUniversityId = guideTeacherProposal.StudentWhichIsAssignedToThisGuideTeacherProposal!.StudentUniversityId,
			StudentRemainingCourses = guideTeacherProposal.StudentWhichIsAssignedToThisGuideTeacherProposal!.StudentRemainingCourses,
			StudentIsDoingThePractice = guideTeacherProposal.StudentWhichIsAssignedToThisGuideTeacherProposal!.StudentIsDoingThePractice,
			StudentIsWorking = guideTeacherProposal.StudentWhichIsAssignedToThisGuideTeacherProposal!.StudentIsWorking,
			AssistantTeacher1 = guideTeacherProposal.AssistantTeacher1OfTheGuideTeacherProposal is not null ? $"{guideTeacherProposal.AssistantTeacher1OfTheGuideTeacherProposal!.FirstName} {guideTeacherProposal.AssistantTeacher1OfTheGuideTeacherProposal!.LastName}" : "No asignado",
			AssistantTeacher2 = guideTeacherProposal.AssistantTeacher2OfTheGuideTeacherProposal is not null ? $"{guideTeacherProposal.AssistantTeacher2OfTheGuideTeacherProposal!.FirstName} {guideTeacherProposal.AssistantTeacher2OfTheGuideTeacherProposal!.LastName}" : "No asignado",
			AssistantTeacher3 = guideTeacherProposal.AssistantTeacher3OfTheGuideTeacherProposal is not null ? $"{guideTeacherProposal.AssistantTeacher3OfTheGuideTeacherProposal!.FirstName} {guideTeacherProposal.AssistantTeacher3OfTheGuideTeacherProposal!.LastName}" : "No asignado",
			CreatedAt = guideTeacherProposal.CreatedAt,
			UpdatedAt = guideTeacherProposal.UpdatedAt
		};
		return this.View(convertViewModel);
	}
}
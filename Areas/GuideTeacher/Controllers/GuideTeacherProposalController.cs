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
				return indexViewModels.OrderBy(gtp => sp.GetType().GetProperty(parameter)!.GetValue(sp, null));
			} else if ($"{parameter}Desc" == sortOrder) {
				return indexViewModels.OrderByDescending(gtp => sp.GetType().GetProperty(parameter)!.GetValue(sp, null));
			}
		}
		return indexViewModels.OrderBy(gtp => sp.GetType().GetProperty(parameters[0]));
	}

	protected IEnumerable<IndexViewModel> FilterProposals(string searchString, IEnumerable<IndexViewModel> indexViewModels, params string[] parameters) {
		var result = new List<IndexViewModel>();
		foreach (var parameter in parameters) {
			var partials = indexViewModels
					.Where(gtp => (sp.GetType().GetProperty(parameter)!.GetValue(sp) as string)!.Contains(searchString));
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
		var studentProposals = this._dbContext.GuideTeacherProposals!.AsNoTracking()
			.Where(gtp => gtp.GuideTeacherOwnerOfTheGuideTeacherProposal == guideTeacher)
			.Select(gtp => new IndexViewModel {
				Id = gtp.Id,
				Title = sp.Title,
				GuideTeacher = $"{sp.GuideTeacherOfTheGuideTeacherProposal!.FirstName} {sp.GuideTeacherOfTheGuideTeacherProposal!.LastName}",
				ProposalStatus = sp.ProposalStatus.ToString(),
			}).AsEnumerable();
		var orderedProposals = this.OrderProposals(sortOrder, studentProposals, parameters);
		var filteredAndOrderedProposals = !searchString.IsNullOrEmpty() ?
			this.FilterProposals(searchString, orderedProposals, parameters)
			: orderedProposals;
		return this.View(PaginatedList<IndexViewModel>.Create((await this._userManager.GetUserAsync(this.User))!.Id, filteredAndOrderedProposals.AsQueryable(), pageNumber ?? 1, 6));
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
		if (await this.CheckApplicationUser(model.GuideTeacher!) is not ApplicationUser guideTeacher) {
			this.ViewBag.WarningMessage = "Revisa tu selección del profesor guía.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		var assistantTeacher1 = !model.AssistantTeacher1.IsNullOrEmpty() ? await this.CheckApplicationUser(model.AssistantTeacher1!) : null;
		var assistantTeacher2 = !model.AssistantTeacher2.IsNullOrEmpty() ? await this.CheckApplicationUser(model.AssistantTeacher2!) : null;
		var assistantTeacher3 = !model.AssistantTeacher3.IsNullOrEmpty() ? await this.CheckApplicationUser(model.AssistantTeacher3!) : null;
		var teacherCheck = (guideTeacher, assistantTeacher1, assistantTeacher2, assistantTeacher3) switch {
			(ApplicationUser gt, ApplicationUser at1, ApplicationUser at2, ApplicationUser at3) => gt != at1 && gt != at2 && gt != at3 && at1 != at2 && at1 != at3 && at2 != at3,
			(ApplicationUser gt, ApplicationUser at1, ApplicationUser at2, null) => gt != at1 && gt != at2 && at1 != at2,
			(ApplicationUser gt, ApplicationUser at1, null, null) => gt != at1,
			(ApplicationUser, null, null, null) => true,
			_ => false
		};
		if (!teacherCheck) {
			this.ViewBag.WarningMessage = "Revisa tu selección de profesores.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		var studentProposal = new GuideTeacherProposal {
			Id = Guid.NewGuid().ToString(),
			Title = model.Title,
			Description = model.Description,
			StudentOwnerOfTheGuideTeacherProposal = student,
			GuideTeacherOfTheGuideTeacherProposal = guideTeacher,
			AssistantTeacher1OfTheGuideTeacherProposal = assistantTeacher1,
			AssistantTeacher2OfTheGuideTeacherProposal = assistantTeacher2,
			AssistantTeacher3OfTheGuideTeacherProposal = assistantTeacher3,
			ProposalStatus = GuideTeacherProposal.Status.Draft,
			CreatedAt = DateTimeOffset.Now,
			UpdatedAt = DateTimeOffset.Now
		};
		_ = await this._dbContext.GuideTeacherProposals!.AddAsync(studentProposal);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "Tu propuesta ha sido registrada correctamente.";
		return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
	}

	public async Task<IActionResult> Edit(string id) {
		if (await this.CheckGuideTeacherSession() is not ApplicationUser guideTeacher) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		await this.PopulateTeachers();
		var studentProposal = await this._dbContext.GuideTeacherProposals!.AsNoTracking()
			.Include(gtp => sp.StudentOwnerOfTheGuideTeacherProposal).AsNoTracking()
			.Where(gtp => sp.StudentOwnerOfTheGuideTeacherProposal == guideTeacher && sp.ProposalStatus == GuideTeacherProposal.Status.Draft)
			.Include(gtp => sp.GuideTeacherOfTheGuideTeacherProposal).AsNoTracking()
			.Include(gtp => sp.AssistantTeacher1OfTheGuideTeacherProposal).AsNoTracking()
			.Include(gtp => sp.AssistantTeacher2OfTheGuideTeacherProposal).AsNoTracking()
			.Include(gtp => sp.AssistantTeacher3OfTheGuideTeacherProposal).AsNoTracking()
			.FirstOrDefaultAsync(gtp => sp.Id == id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		var editViewModel = new EditViewModel {
			Id = id,
			Title = studentProposal!.Title,
			Description = studentProposal.Description,
			GuideTeacher = studentProposal.GuideTeacherOfTheGuideTeacherProposal!.Id,
			AssistantTeacher1 = studentProposal.AssistantTeacher1OfTheGuideTeacherProposal?.Id,
			AssistantTeacher2 = studentProposal.AssistantTeacher2OfTheGuideTeacherProposal?.Id,
			AssistantTeacher3 = studentProposal.AssistantTeacher3OfTheGuideTeacherProposal?.Id,
			CreatedAt = studentProposal.CreatedAt,
			UpdatedAt = studentProposal.UpdatedAt
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
		var studentProposal = await this._dbContext.GuideTeacherProposals!
			.Include(gtp => sp.StudentOwnerOfTheGuideTeacherProposal)
			.Include(gtp => sp.GuideTeacherOfTheGuideTeacherProposal)
			.Include(gtp => sp.AssistantTeacher1OfTheGuideTeacherProposal)
			.Include(gtp => sp.AssistantTeacher2OfTheGuideTeacherProposal)
			.Include(gtp => sp.AssistantTeacher3OfTheGuideTeacherProposal)
			.FirstOrDefaultAsync(gtp => sp.Id == model.Id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		if (await this.CheckApplicationUser(model.GuideTeacher!) is not ApplicationUser guideTeacher) {
			this.ViewBag.WarningMessage = "Revisa tu selección del profesor guía.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		if (studentProposal.ProposalStatus != GuideTeacherProposal.Status.Draft) {
			this.ViewBag.ErrorMessage = "La propuesta no puede ser editada ya que no es un borrador.";
			return this.View(new EditViewModel {
				Id = model.Id,
				Title = model.Title,
				Description = model.Description,
				CreatedAt = studentProposal.CreatedAt,
				UpdatedAt = studentProposal.UpdatedAt
			});
		}
		var assistantTeacher1 = !model.AssistantTeacher1.IsNullOrEmpty() ? await this.CheckApplicationUser(model.AssistantTeacher1!) : null;
		var assistantTeacher2 = !model.AssistantTeacher2.IsNullOrEmpty() ? await this.CheckApplicationUser(model.AssistantTeacher2!) : null;
		var assistantTeacher3 = !model.AssistantTeacher3.IsNullOrEmpty() ? await this.CheckApplicationUser(model.AssistantTeacher3!) : null;
		var teacherCheck = (guideTeacher, assistantTeacher1, assistantTeacher2, assistantTeacher3) switch {
			(ApplicationUser gt, ApplicationUser at1, ApplicationUser at2, ApplicationUser at3) => gt != at1 && gt != at2 && gt != at3 && at1 != at2 && at1 != at3 && at2 != at3,
			(ApplicationUser gt, ApplicationUser at1, ApplicationUser at2, null) => gt != at1 && gt != at2 && at1 != at2,
			(ApplicationUser gt, ApplicationUser at1, null, null) => gt != at1,
			(ApplicationUser, null, null, null) => true,
			_ => false
		};
		if (!teacherCheck) {
			this.ViewBag.WarningMessage = "Revisa tu selección de profesores.";
			return this.View(new EditViewModel {
				Id = model.Id,
				Title = model.Title,
				Description = model.Description,
				CreatedAt = studentProposal.CreatedAt,
				UpdatedAt = studentProposal.UpdatedAt
			});
		}
		studentProposal.Title = model.Title;
		studentProposal.Description = model.Description;
		studentProposal.GuideTeacherOfTheGuideTeacherProposal = guideTeacher;
		studentProposal.AssistantTeacher1OfTheGuideTeacherProposal = assistantTeacher1;
		studentProposal.AssistantTeacher2OfTheGuideTeacherProposal = assistantTeacher2;
		studentProposal.AssistantTeacher3OfTheGuideTeacherProposal = assistantTeacher3;
		studentProposal.UpdatedAt = DateTimeOffset.Now;
		_ = this._dbContext.GuideTeacherProposals!.Update(studentProposal);
		_ = await this._dbContext.SaveChangesAsync();
		var editViewModel = new EditViewModel {
			Id = studentProposal.Id,
			Title = studentProposal!.Title,
			Description = studentProposal.Description,
			GuideTeacher = studentProposal.GuideTeacherOfTheGuideTeacherProposal!.Id,
			AssistantTeacher1 = studentProposal.AssistantTeacher1OfTheGuideTeacherProposal?.Id,
			AssistantTeacher2 = studentProposal.AssistantTeacher2OfTheGuideTeacherProposal?.Id,
			AssistantTeacher3 = studentProposal.AssistantTeacher3OfTheGuideTeacherProposal?.Id,
			CreatedAt = studentProposal.CreatedAt,
			UpdatedAt = studentProposal.UpdatedAt
		};
		this.ViewBag.SuccessMessage = "Tu propuesta ha sido actualizada correctamente.";
		return this.View(editViewModel);
	}

	public async Task<IActionResult> Delete(string id) {
		if (await this.CheckGuideTeacherSession() is not ApplicationUser guideTeacher) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var studentProposal = await this._dbContext.GuideTeacherProposals!.AsNoTracking()
			.Where(gtp => sp.StudentOwnerOfTheGuideTeacherProposal == guideTeacher && sp.ProposalStatus == GuideTeacherProposal.Status.Draft)
			.FirstOrDefaultAsync(gtp => sp.Id == id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		var deleteViewModel = new DeleteViewModel {
			Id = id,
			Title = studentProposal.Title
		};
		return this.View(deleteViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete([FromForm] DeleteViewModel model) {
		if (await this.CheckGuideTeacherSession() is not ApplicationUser guideTeacher) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var studentProposal = await this._dbContext.GuideTeacherProposals!
			.Where(gtp => sp.StudentOwnerOfTheGuideTeacherProposal == guideTeacher && sp.ProposalStatus == GuideTeacherProposal.Status.Draft)
			.FirstOrDefaultAsync(gtp => sp.Id == model.Id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		_ = this._dbContext.GuideTeacherProposals!.Remove(studentProposal);
		_ = this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "Tu propuesta ha sido eliminada correctamente.";
		return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
	}

	public async Task<IActionResult> Send(string id) {
		if (await this.CheckGuideTeacherSession() is not ApplicationUser guideTeacher) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var studentProposal = await this._dbContext.GuideTeacherProposals!.AsNoTracking()
			.Where(gtp => sp.StudentOwnerOfTheGuideTeacherProposal == guideTeacher && sp.ProposalStatus == GuideTeacherProposal.Status.Draft)
			.Include(gtp => sp.GuideTeacherOfTheGuideTeacherProposal)
			.FirstOrDefaultAsync(gtp => sp.Id == id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		var sendViewModel = new SendViewModel {
			Id = id,
			Title = studentProposal.Title,
			GuideTeacherName = $"{studentProposal.GuideTeacherOfTheGuideTeacherProposal!.FirstName} {studentProposal.GuideTeacherOfTheGuideTeacherProposal!.LastName}"
		};
		return this.View(sendViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Send([FromForm] SendViewModel model) {
		if (await this.CheckGuideTeacherSession() is not ApplicationUser guideTeacher) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var studentProposal = await this._dbContext.GuideTeacherProposals!
			.Where(gtp => sp.StudentOwnerOfTheGuideTeacherProposal == guideTeacher && sp.ProposalStatus == GuideTeacherProposal.Status.Draft)
			.Include(gtp => sp.GuideTeacherOfTheGuideTeacherProposal)
			.Include(gtp => sp.AssistantTeacher1OfTheGuideTeacherProposal)
			.Include(gtp => sp.AssistantTeacher2OfTheGuideTeacherProposal)
			.Include(gtp => sp.AssistantTeacher3OfTheGuideTeacherProposal)
			.FirstOrDefaultAsync(gtp => sp.Id == model.Id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		var teacherCheck = (studentProposal.GuideTeacherOfTheGuideTeacherProposal, studentProposal.AssistantTeacher1OfTheGuideTeacherProposal, studentProposal.AssistantTeacher2OfTheGuideTeacherProposal, studentProposal.AssistantTeacher3OfTheGuideTeacherProposal) switch {
			(ApplicationUser teacher, _, _, _) when teacher.IsDeactivated => "El profesor guía está desactivado.",
			(_, ApplicationUser teacher, _, _) when teacher.IsDeactivated => "El profesor co-guía 1 está desactivado.",
			(_, _, ApplicationUser teacher, _) when teacher.IsDeactivated => "El profesor co-guía 2 está desactivado.",
			(_, _, _, ApplicationUser teacher) when teacher.IsDeactivated => "El profesor co-guía 3 está desactivado.",
			_ => null
		};
		if (teacherCheck is not null) {
			this.TempData["ErrorMessage"] = teacherCheck;
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		studentProposal.ProposalStatus = GuideTeacherProposal.Status.SentToGuideTeacher;
		studentProposal.UpdatedAt = DateTimeOffset.Now;
		_ = this._dbContext.GuideTeacherProposals!.Update(studentProposal);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "Tu propuesta ha sido enviada correctamente.";
		return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
	}

	public async Task<IActionResult> ViewRejectionReason(string id) {
		if (await this.CheckGuideTeacherSession() is not ApplicationUser guideTeacher) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var studentProposal = await this._dbContext.GuideTeacherProposals!.AsNoTracking()
			.Where(gtp => sp.StudentOwnerOfTheGuideTeacherProposal == guideTeacher && sp.ProposalStatus == GuideTeacherProposal.Status.RejectedByGuideTeacher)
			.Include(gtp => sp.GuideTeacherOfTheGuideTeacherProposal)
			.FirstOrDefaultAsync(gtp => sp.Id == id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		var viewRejectionReasonViewModel = new ViewRejectionReasonViewModel {
			Id = id,
			Title = studentProposal.Title,
			Description = studentProposal.Description,
			RejectedBy = $"{studentProposal.GuideTeacherOfTheGuideTeacherProposal!.FirstName} {studentProposal.GuideTeacherOfTheGuideTeacherProposal!.LastName}",
			Reason = studentProposal.RejectionReason,
			CreatedAt = studentProposal.CreatedAt,
			UpdatedAt = studentProposal.UpdatedAt
		};
		return this.View(viewRejectionReasonViewModel);
	}

	public async Task<IActionResult> Convert(string id) {
		if (await this.CheckGuideTeacherSession() is not ApplicationUser guideTeacher) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var studentProposal = await this._dbContext.GuideTeacherProposals!.AsNoTracking()
			.Where(gtp => sp.StudentOwnerOfTheGuideTeacherProposal == guideTeacher && sp.ProposalStatus == GuideTeacherProposal.Status.ApprovedByGuideTeacher)
			.Include(gtp => sp.GuideTeacherOfTheGuideTeacherProposal).AsNoTracking()
			.Include(gtp => sp.AssistantTeacher1OfTheGuideTeacherProposal).AsNoTracking()
			.Include(gtp => sp.AssistantTeacher2OfTheGuideTeacherProposal).AsNoTracking()
			.Include(gtp => sp.AssistantTeacher3OfTheGuideTeacherProposal).AsNoTracking()
			.FirstOrDefaultAsync(gtp => sp.Id == id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(GuideTeacherProposalController.Index), nameof(GuideTeacherProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		var convertViewModel = new ConvertViewModel {
			Id = id,
			Title = studentProposal.Title,
			Description = studentProposal.Description,
			GuideTeacherName = $"{studentProposal.GuideTeacherOfTheGuideTeacherProposal!.FirstName} {studentProposal.GuideTeacherOfTheGuideTeacherProposal!.LastName}",
			GuideTeacherEmail = studentProposal.GuideTeacherOfTheGuideTeacherProposal!.Email,
			GuideTeacherOffice = studentProposal.GuideTeacherOfTheGuideTeacherProposal!.TeacherOffice,
			GuideTeacherSchedule = studentProposal.GuideTeacherOfTheGuideTeacherProposal!.TeacherSchedule,
			GuideTeacherSpecialization = studentProposal.GuideTeacherOfTheGuideTeacherProposal!.TeacherSpecialization,
			AssistantTeacher1 = studentProposal.AssistantTeacher1OfTheGuideTeacherProposal is not null ? $"{studentProposal.AssistantTeacher1OfTheGuideTeacherProposal!.FirstName} {studentProposal.AssistantTeacher1OfTheGuideTeacherProposal!.LastName}" : "No asignado",
			AssistantTeacher2 = studentProposal.AssistantTeacher2OfTheGuideTeacherProposal is not null ? $"{studentProposal.AssistantTeacher2OfTheGuideTeacherProposal!.FirstName} {studentProposal.AssistantTeacher2OfTheGuideTeacherProposal!.LastName}" : "No asignado",
			AssistantTeacher3 = studentProposal.AssistantTeacher3OfTheGuideTeacherProposal is not null ? $"{studentProposal.AssistantTeacher3OfTheGuideTeacherProposal!.FirstName} {studentProposal.AssistantTeacher3OfTheGuideTeacherProposal!.LastName}" : "No asignado",
			CreatedAt = studentProposal.CreatedAt,
			UpdatedAt = studentProposal.UpdatedAt
		};
		return this.View(convertViewModel);
	}
}
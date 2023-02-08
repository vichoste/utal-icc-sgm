using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Utal.Icc.Sgm.Areas.Student.Views.StudentProposal;
using Utal.Icc.Sgm.Controllers;
using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;

using static Utal.Icc.Sgm.Models.ApplicationUser;

namespace Utal.Icc.Sgm.Areas.Student.Controllers;

[Area(nameof(Student)), Authorize(Roles = nameof(Roles.Student))]
public class StudentProposalController : Controller {
	private readonly ApplicationDbContext _dbContext;
	private readonly UserManager<ApplicationUser> _userManager;

	public StudentProposalController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager) {
		this._dbContext = dbContext;
		this._userManager = userManager;
	}

	protected async Task<ApplicationUser> CheckStudentSession() {
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
				return indexViewModels.OrderBy(sp => sp.GetType().GetProperty(parameter)!.GetValue(sp, null));
			} else if ($"{parameter}Desc" == sortOrder) {
				return indexViewModels.OrderByDescending(sp => sp.GetType().GetProperty(parameter)!.GetValue(sp, null));
			}
		}
		return indexViewModels.OrderBy(sp => sp.GetType().GetProperty(parameters[0]));
	}

	protected IEnumerable<IndexViewModel> FilterProposals(string searchString, IEnumerable<IndexViewModel> indexViewModels, params string[] parameters) {
		var result = new List<IndexViewModel>();
		foreach (var parameter in parameters) {
			var partials = indexViewModels
					.Where(sp => (sp.GetType().GetProperty(parameter)!.GetValue(sp) as string)!.Contains(searchString));
			foreach (var partial in partials) {
				if (!result.Any(ivm => ivm.Id == partial.Id)) {
					result.Add(partial);
				}
			}
		}
		return result.AsEnumerable();
	}

		protected IOrderedEnumerable<ApplicationUser> OrderGuideTeachers(string sortOrder, IEnumerable<ApplicationUser> guideTeachers, params string[] parameters) {
		foreach (var parameter in parameters) {
			if (parameter == sortOrder) {
				return guideTeachers.OrderBy(s => s.GetType().GetProperty(parameter)!.GetValue(s, null));
			} else if ($"{parameter}Desc" == sortOrder) {
				return guideTeachers.OrderByDescending(s => s.GetType().GetProperty(parameter)!.GetValue(s, null));
			}
		}
		return guideTeachers.OrderBy(s => s.GetType().GetProperty(parameters[0]));
	}

	protected IEnumerable<GuideTeacherViewModel> FilterGuideTeachers(string searchString, IOrderedEnumerable<ApplicationUser> guideTeachers, params string[] parameters) {
		var result = new List<GuideTeacherViewModel>();
		foreach (var parameter in parameters) {
			var partials = guideTeachers
					.Where(t => (t.GetType().GetProperty(parameter)!.GetValue(t) as string)!.Contains(searchString))
					.Select(t => new GuideTeacherViewModel {
						Id = t.Id,
						FirstName = t.FirstName,
						LastName = t.LastName,
						Email = t.Email,
						TeacherOffice = t.TeacherOffice,
						TeacherSchedule = t.TeacherSchedule,
						TeacherSpecialization = t.TeacherSpecialization
					});
			foreach (var partial in partials) {
				if (!result.Any(ivm => ivm.Id == partial.Id)) {
					result.Add(partial);
				}
			}
		}
		return result.AsEnumerable();
	}

	protected async Task PopulateTeachers() {
		var guideTeachers = (
			await this._userManager.GetUsersInRoleAsync(nameof(Roles.GuideTeacher)))
				.Where(gt => !gt.IsDeactivated)
				.OrderBy(gt => gt.LastName)
				.ToList();
		var assistantTeachers = (
			await this._userManager.GetUsersInRoleAsync(nameof(Roles.AssistantTeacher)))
				.Where(gt => !gt.IsDeactivated)
				.OrderBy(at => at.LastName)
				.ToList();
		this.ViewData[$"{nameof(Roles.GuideTeacher)}s"] = guideTeachers.Select(gt => new SelectListItem {
			Text = $"{gt.FirstName} {gt.LastName}",
			Value = gt.Id
		});
		this.ViewData[$"{nameof(Roles.AssistantTeacher)}s"] = assistantTeachers.Select(at => new SelectListItem {
			Text = $"{at.FirstName} {at.LastName}",
			Value = at.Id
		});
	}

	public async Task<IActionResult> GuideTeacher(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		if (await this.CheckStudentSession() is not ApplicationUser student) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var parameters = new[] { nameof(ApplicationUser.FirstName), nameof(ApplicationUser.LastName), nameof(ApplicationUser.Email), nameof(ApplicationUser.TeacherOffice), nameof(ApplicationUser.TeacherSchedule), nameof(ApplicationUser.TeacherSpecialization) };
		this.SetSortParameters(sortOrder, parameters);
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		var guideTeachers = await this._userManager.GetUsersInRoleAsync(nameof(Roles.GuideTeacher));
		var orderedGuideTeachers = this.OrderGuideTeachers(sortOrder, guideTeachers, parameters);
		var guideTeacherViewModels = !searchString.IsNullOrEmpty() ? this.FilterGuideTeachers(searchString, orderedGuideTeachers, parameters) : orderedGuideTeachers.Select(t => new GuideTeacherViewModel {
			Id = t.Id,
			FirstName = t.FirstName,
			LastName = t.LastName,
			Email = t.Email,
			TeacherOffice = t.TeacherOffice,
			TeacherSchedule = t.TeacherSchedule,
			TeacherSpecialization = t.TeacherSpecialization
		}).ToList();
		return this.View(PaginatedList<GuideTeacherViewModel>.Create(guideTeacherViewModels.AsQueryable(), pageNumber ?? 1, 6));
	}

	public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		if (await this.CheckStudentSession() is not ApplicationUser student) {
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
		var studentProposals = this._dbContext.StudentProposals!.AsNoTracking()
			.Where(sp => sp.StudentOwnerOfTheStudentProposal == student)
			.Include(sp => sp.GuideTeacherOfTheStudentProposal).AsNoTracking()
			.Select(sp => new IndexViewModel {
				Id = sp.Id,
				Title = sp.Title,
				GuideTeacher = $"{sp.GuideTeacherOfTheStudentProposal!.FirstName} {sp.GuideTeacherOfTheStudentProposal!.LastName}",
				ProposalStatus = sp.ProposalStatus.ToString(),
			}).AsEnumerable();
		var orderedProposals = this.OrderProposals(sortOrder, studentProposals, parameters);
		var filteredAndOrderedProposals = !searchString.IsNullOrEmpty() ?
			this.FilterProposals(searchString, orderedProposals, parameters)
			: orderedProposals;
		return this.View(PaginatedList<IndexViewModel>.Create(filteredAndOrderedProposals.AsQueryable(), pageNumber ?? 1, 6));
	}

	public async Task<IActionResult> Create(string id) {
		if (await this.CheckStudentSession() is null) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		await this.PopulateTeachers();
		if (await this.CheckApplicationUser(id) is not ApplicationUser guideTeacher) {
			this.TempData["ErrorMessage"] = "Error al obtener al profesor guía.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		var createViewModel = new CreateViewModel {
			GuideTeacherId = guideTeacher.Id,
			GuideTeacher = $"{guideTeacher.FirstName} {guideTeacher.LastName}",
		};
		return this.View(createViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Create([FromForm] CreateViewModel model) {
		if (await this.CheckStudentSession() is not ApplicationUser student) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		if (!this.ModelState.IsValid) {
			await this.PopulateTeachers();
			this.ViewBag.WarningMessage = "Revisa que los campos estén correctos.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
				GuideTeacherId = model.GuideTeacherId,
				GuideTeacher = model.GuideTeacher,
				AssistantTeachers = model.AssistantTeachers
			});
		}
		if (await this.CheckApplicationUser(model.GuideTeacher!) is not ApplicationUser guideTeacher) {
			this.TempData["ErrorMessage"] = "Revisa tu selección del profesor guía.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		var assistantTeachers = model.AssistantTeachers!.Select(async at => await this.CheckApplicationUser(at)).Select(at => at.Result).ToList();
		var studentProposal = new StudentProposal {
			Id = Guid.NewGuid().ToString(),
			Title = model.Title,
			Description = model.Description,
			StudentOwnerOfTheStudentProposal = student,
			GuideTeacherOfTheStudentProposal = guideTeacher,
			AssistantTeachersOfTheStudentProposal = assistantTeachers!,
			ProposalStatus = StudentProposal.Status.Draft,
			CreatedAt = DateTimeOffset.Now,
			UpdatedAt = DateTimeOffset.Now
		};
		_ = await this._dbContext.StudentProposals!.AddAsync(studentProposal);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "Tu propuesta ha sido registrada correctamente.";
		return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
	}

	public async Task<IActionResult> Edit(string id) {
		if (await this.CheckStudentSession() is not ApplicationUser student) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		await this.PopulateTeachers();
		var studentProposal = await this._dbContext.StudentProposals!.AsNoTracking()
			.Include(sp => sp.StudentOwnerOfTheStudentProposal).AsNoTracking()
			.Where(sp => sp.StudentOwnerOfTheStudentProposal == student && sp.ProposalStatus == StudentProposal.Status.Draft)
			.Include(sp => sp.GuideTeacherOfTheStudentProposal).AsNoTracking()
			.Include(sp => sp.AssistantTeachersOfTheStudentProposal).AsNoTracking()
			.FirstOrDefaultAsync(sp => sp.Id == id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		var editViewModel = new EditViewModel {
			Id = id,
			Title = studentProposal!.Title,
			Description = studentProposal.Description,
			GuideTeacher = studentProposal.GuideTeacherOfTheStudentProposal!.Id,
			AssistantTeachers = studentProposal.AssistantTeachersOfTheStudentProposal!.Select(at => at!.Id).ToList(),
			CreatedAt = studentProposal.CreatedAt,
			UpdatedAt = studentProposal.UpdatedAt
		};
		return this.View(editViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit([FromForm] EditViewModel model) {
		if (!this.ModelState.IsValid) {
			await this.PopulateTeachers();
			this.ViewBag.WarningMessage = "Revisa que los campos estén correctos.";
			return this.View(new EditViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		if (await this.CheckStudentSession() is not ApplicationUser student) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var studentProposal = await this._dbContext.StudentProposals!
			.Include(sp => sp.StudentOwnerOfTheStudentProposal)
			.Include(sp => sp.GuideTeacherOfTheStudentProposal)
			.Include(sp => sp.AssistantTeachersOfTheStudentProposal)
			.FirstOrDefaultAsync(sp => sp.Id == model.Id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		if (await this.CheckApplicationUser(model.GuideTeacher!) is not ApplicationUser guideTeacher) {
			this.ViewBag.WarningMessage = "Revisa tu selección del profesor guía.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		if (studentProposal.ProposalStatus != StudentProposal.Status.Draft) {
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
		studentProposal.GuideTeacherOfTheStudentProposal = guideTeacher;
		studentProposal.AssistantTeacher1OfTheStudentProposal = assistantTeacher1;
		studentProposal.AssistantTeacher2OfTheStudentProposal = assistantTeacher2;
		studentProposal.AssistantTeacher3OfTheStudentProposal = assistantTeacher3;
		studentProposal.UpdatedAt = DateTimeOffset.Now;
		_ = this._dbContext.StudentProposals!.Update(studentProposal);
		_ = await this._dbContext.SaveChangesAsync();
		var editViewModel = new EditViewModel {
			Id = studentProposal.Id,
			Title = studentProposal!.Title,
			Description = studentProposal.Description,
			GuideTeacher = studentProposal.GuideTeacherOfTheStudentProposal!.Id,
			AssistantTeacher1 = studentProposal.AssistantTeacher1OfTheStudentProposal?.Id,
			AssistantTeacher2 = studentProposal.AssistantTeacher2OfTheStudentProposal?.Id,
			AssistantTeacher3 = studentProposal.AssistantTeacher3OfTheStudentProposal?.Id,
			CreatedAt = studentProposal.CreatedAt,
			UpdatedAt = studentProposal.UpdatedAt
		};
		this.ViewBag.SuccessMessage = "Tu propuesta ha sido actualizada correctamente.";
		return this.View(editViewModel);
	}

	public async Task<IActionResult> Delete(string id) {
		if (await this.CheckStudentSession() is not ApplicationUser student) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var studentProposal = await this._dbContext.StudentProposals!.AsNoTracking()
			.Where(sp => sp.StudentOwnerOfTheStudentProposal == student && sp.ProposalStatus == StudentProposal.Status.Draft)
			.FirstOrDefaultAsync(sp => sp.Id == id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		var deleteViewModel = new DeleteViewModel {
			Id = id,
			Title = studentProposal.Title
		};
		return this.View(deleteViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete([FromForm] DeleteViewModel model) {
		if (await this.CheckStudentSession() is not ApplicationUser student) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var studentProposal = await this._dbContext.StudentProposals!
			.Where(sp => sp.StudentOwnerOfTheStudentProposal == student && sp.ProposalStatus == StudentProposal.Status.Draft)
			.FirstOrDefaultAsync(sp => sp.Id == model.Id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		_ = this._dbContext.StudentProposals!.Remove(studentProposal);
		_ = this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "Tu propuesta ha sido eliminada correctamente.";
		return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
	}

	public async Task<IActionResult> Send(string id) {
		if (await this.CheckStudentSession() is not ApplicationUser student) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var studentProposal = await this._dbContext.StudentProposals!.AsNoTracking()
			.Where(sp => sp.StudentOwnerOfTheStudentProposal == student && sp.ProposalStatus == StudentProposal.Status.Draft)
			.Include(sp => sp.GuideTeacherOfTheStudentProposal)
			.FirstOrDefaultAsync(sp => sp.Id == id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		var sendViewModel = new SendViewModel {
			Id = id,
			Title = studentProposal.Title,
			GuideTeacherName = $"{studentProposal.GuideTeacherOfTheStudentProposal!.FirstName} {studentProposal.GuideTeacherOfTheStudentProposal!.LastName}"
		};
		return this.View(sendViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Send([FromForm] SendViewModel model) {
		if (await this.CheckStudentSession() is not ApplicationUser student) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var studentProposal = await this._dbContext.StudentProposals!
			.Where(sp => sp.StudentOwnerOfTheStudentProposal == student && sp.ProposalStatus == StudentProposal.Status.Draft)
			.Include(sp => sp.GuideTeacherOfTheStudentProposal)
			.Include(sp => sp.AssistantTeacher1OfTheStudentProposal)
			.Include(sp => sp.AssistantTeacher2OfTheStudentProposal)
			.Include(sp => sp.AssistantTeacher3OfTheStudentProposal)
			.FirstOrDefaultAsync(sp => sp.Id == model.Id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		var teacherCheck = (studentProposal.GuideTeacherOfTheStudentProposal, studentProposal.AssistantTeacher1OfTheStudentProposal, studentProposal.AssistantTeacher2OfTheStudentProposal, studentProposal.AssistantTeacher3OfTheStudentProposal) switch {
			(ApplicationUser teacher, _, _, _) when teacher.IsDeactivated => "El profesor guía está desactivado.",
			(_, ApplicationUser teacher, _, _) when teacher.IsDeactivated => "El profesor co-guía 1 está desactivado.",
			(_, _, ApplicationUser teacher, _) when teacher.IsDeactivated => "El profesor co-guía 2 está desactivado.",
			(_, _, _, ApplicationUser teacher) when teacher.IsDeactivated => "El profesor co-guía 3 está desactivado.",
			_ => null
		};
		if (teacherCheck is not null) {
			this.TempData["ErrorMessage"] = teacherCheck;
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		studentProposal.ProposalStatus = StudentProposal.Status.SentToGuideTeacher;
		studentProposal.UpdatedAt = DateTimeOffset.Now;
		_ = this._dbContext.StudentProposals!.Update(studentProposal);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "Tu propuesta ha sido enviada correctamente.";
		return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
	}

	public async Task<IActionResult> ViewRejectionReason(string id) {
		if (await this.CheckStudentSession() is not ApplicationUser student) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var studentProposal = await this._dbContext.StudentProposals!.AsNoTracking()
			.Where(sp => sp.StudentOwnerOfTheStudentProposal == student && sp.ProposalStatus == StudentProposal.Status.RejectedByGuideTeacher)
			.Include(sp => sp.GuideTeacherOfTheStudentProposal)
			.FirstOrDefaultAsync(sp => sp.Id == id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		var viewRejectionReasonViewModel = new ViewRejectionReasonViewModel {
			Id = id,
			Title = studentProposal.Title,
			Description = studentProposal.Description,
			RejectedBy = $"{studentProposal.GuideTeacherOfTheStudentProposal!.FirstName} {studentProposal.GuideTeacherOfTheStudentProposal!.LastName}",
			Reason = studentProposal.RejectionReason,
			CreatedAt = studentProposal.CreatedAt,
			UpdatedAt = studentProposal.UpdatedAt
		};
		return this.View(viewRejectionReasonViewModel);
	}

	public async Task<IActionResult> Convert(string id) {
		if (await this.CheckStudentSession() is not ApplicationUser student) {
			return this.RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty), new { area = string.Empty });
		}
		var studentProposal = await this._dbContext.StudentProposals!.AsNoTracking()
			.Where(sp => sp.StudentOwnerOfTheStudentProposal == student && sp.ProposalStatus == StudentProposal.Status.ApprovedByGuideTeacher)
			.Include(sp => sp.GuideTeacherOfTheStudentProposal).AsNoTracking()
			.Include(sp => sp.AssistantTeacher1OfTheStudentProposal).AsNoTracking()
			.Include(sp => sp.AssistantTeacher2OfTheStudentProposal).AsNoTracking()
			.Include(sp => sp.AssistantTeacher3OfTheStudentProposal).AsNoTracking()
			.FirstOrDefaultAsync(sp => sp.Id == id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction(nameof(StudentProposalController.Index), nameof(StudentProposalController).Replace("Controller", string.Empty), new { area = nameof(Student) });
		}
		var convertViewModel = new ConvertViewModel {
			Id = id,
			Title = studentProposal.Title,
			Description = studentProposal.Description,
			GuideTeacherName = $"{studentProposal.GuideTeacherOfTheStudentProposal!.FirstName} {studentProposal.GuideTeacherOfTheStudentProposal!.LastName}",
			GuideTeacherEmail = studentProposal.GuideTeacherOfTheStudentProposal!.Email,
			GuideTeacherOffice = studentProposal.GuideTeacherOfTheStudentProposal!.TeacherOffice,
			GuideTeacherSchedule = studentProposal.GuideTeacherOfTheStudentProposal!.TeacherSchedule,
			GuideTeacherSpecialization = studentProposal.GuideTeacherOfTheStudentProposal!.TeacherSpecialization,
			AssistantTeacher1 = studentProposal.AssistantTeacher1OfTheStudentProposal is not null ? $"{studentProposal.AssistantTeacher1OfTheStudentProposal!.FirstName} {studentProposal.AssistantTeacher1OfTheStudentProposal!.LastName}" : "No asignado",
			AssistantTeacher2 = studentProposal.AssistantTeacher2OfTheStudentProposal is not null ? $"{studentProposal.AssistantTeacher2OfTheStudentProposal!.FirstName} {studentProposal.AssistantTeacher2OfTheStudentProposal!.LastName}" : "No asignado",
			AssistantTeacher3 = studentProposal.AssistantTeacher3OfTheStudentProposal is not null ? $"{studentProposal.AssistantTeacher3OfTheStudentProposal!.FirstName} {studentProposal.AssistantTeacher3OfTheStudentProposal!.LastName}" : "No asignado",
			CreatedAt = studentProposal.CreatedAt,
			UpdatedAt = studentProposal.UpdatedAt
		};
		return this.View(convertViewModel);
	}
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Utal.Icc.Sgm.Areas.Student.Views.Proposal;
using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;

using static Utal.Icc.Sgm.Models.ApplicationUser;

namespace Utal.Icc.Sgm.Areas.Student.Controllers;

[Area("Student"), Authorize(Roles = "Student")]
public class ProposalController : Controller {
	private readonly ApplicationDbContext _dbContext;
	private readonly UserManager<ApplicationUser> _userManager;

	public ProposalController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager) {
		this._dbContext = dbContext;
		this._userManager = userManager;
	}

	public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber) {
		var student = this._dbContext.Users.AsNoTracking()
			.Include(s => s.StudentProposalsWhichIOwn).AsNoTracking()
			.FirstOrDefault(s => s.Id == this._userManager.GetUserId(this.User));
		if (student is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (student.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		this.ViewData["TitleSortParam"] = sortOrder == "Title" ? "TitleDesc" : "Title";
		if (searchString is not null) {
			pageNumber = 1;
		} else {
			searchString = currentFilter;
		}
		this.ViewData["CurrentFilter"] = searchString;
		var orderedProposals = sortOrder switch {
			"Title" => student.StudentProposalsWhichIOwn!.OrderBy(sp => sp.Title),
			"TitleDesc" => student.StudentProposalsWhichIOwn!.OrderByDescending(sp => sp.Title),
			_ => student.StudentProposalsWhichIOwn!.OrderBy(sp => sp.Title)
		};
		var filteredAndOrderedProposals = orderedProposals.ToList();
		if (!string.IsNullOrEmpty(searchString)) {
			filteredAndOrderedProposals = orderedProposals
				.Where(sp => sp.Title!.ToUpper().Contains(searchString.ToUpper()))
				.ToList();
		}
		var indexViewModels = filteredAndOrderedProposals.Select(sp => new IndexViewModel {
			Id = sp.Id,
			Title = sp.Title,
			ProposalStatus = sp.ProposalStatus.ToString(),
		});
		var pageSize = 6;
		return this.View(PaginatedList<IndexViewModel>.Create((await this._userManager.GetUserAsync(this.User))!.Id, indexViewModels.AsQueryable(), pageNumber ?? 1, pageSize));
	}

	public async Task<IActionResult> Create() {
		var student = await this._userManager.GetUserAsync(this.User);
		if (student is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (student.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var guideTeachers = (
			await this._userManager.GetUsersInRoleAsync(Roles.GuideTeacher.ToString()))
			.Where(gt => !gt.IsDeactivated)
			.OrderBy(gt => gt.LastName)
			.ToList();
		var assistantTeachers1 = (
			await this._userManager.GetUsersInRoleAsync(Roles.AssistantTeacher.ToString()))
			.Where(gt => !gt.IsDeactivated)
			.OrderBy(at => at.LastName)
			.ToList();
		var assistantTeachers2 = (
			await this._userManager.GetUsersInRoleAsync(Roles.AssistantTeacher.ToString()))
			.Where(gt => !gt.IsDeactivated)
			.OrderBy(at => at.LastName)
			.ToList();
		var assistantTeachers3 = (
			await this._userManager.GetUsersInRoleAsync(Roles.AssistantTeacher.ToString()))
			.Where(gt => !gt.IsDeactivated)
			.OrderBy(at => at.LastName)
			.ToList();
		this.ViewData["GuideTeachers"] = guideTeachers.Select(gt => new SelectListItem {
			Text = $"{gt.FirstName} {gt.LastName}",
			Value = gt.Id
		});
		this.ViewData["AssistantTeachers1"] = assistantTeachers1.Select(gt => new SelectListItem {
			Text = $"{gt.FirstName} {gt.LastName}",
			Value = gt.Id
		});
		this.ViewData["AssistantTeachers2"] = assistantTeachers2.Select(gt => new SelectListItem {
			Text = $"{gt.FirstName} {gt.LastName}",
			Value = gt.Id
		});
		this.ViewData["AssistantTeachers3"] = assistantTeachers3.Select(gt => new SelectListItem {
			Text = $"{gt.FirstName} {gt.LastName}",
			Value = gt.Id
		});
		return this.View(new CreateViewModel());
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Create([FromForm] CreateViewModel model) {
		var student = await this._userManager.GetUserAsync(this.User);
		if (student is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (student.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (!this.ModelState.IsValid) {
			this.ViewBag.WarningMessage = "Revisa que los campos estén correctos.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		var guideTeachers = (
			await this._userManager.GetUsersInRoleAsync(Roles.GuideTeacher.ToString()))
			.Where(gt => !gt.IsDeactivated)
			.OrderBy(gt => gt.LastName)
			.ToList();
		var assistantTeachers1 = (
			await this._userManager.GetUsersInRoleAsync(Roles.AssistantTeacher.ToString()))
			.Where(gt => !gt.IsDeactivated)
			.OrderBy(at => at.LastName)
			.ToList();
		var assistantTeachers2 = (
			await this._userManager.GetUsersInRoleAsync(Roles.AssistantTeacher.ToString()))
			.Where(gt => !gt.IsDeactivated)
			.OrderBy(at => at.LastName)
			.ToList();
		var assistantTeachers3 = (
			await this._userManager.GetUsersInRoleAsync(Roles.AssistantTeacher.ToString()))
			.Where(gt => !gt.IsDeactivated)
			.OrderBy(at => at.LastName)
			.ToList();
		this.ViewData["GuideTeachers"] = guideTeachers.Select(gt => new SelectListItem {
			Text = $"{gt.FirstName} {gt.LastName}",
			Value = gt.Id
		});
		this.ViewData["AssistantTeachers1"] = assistantTeachers1.Select(gt => new SelectListItem {
			Text = $"{gt.FirstName} {gt.LastName}",
			Value = gt.Id
		});
		this.ViewData["AssistantTeachers2"] = assistantTeachers2.Select(gt => new SelectListItem {
			Text = $"{gt.FirstName} {gt.LastName}",
			Value = gt.Id
		});
		this.ViewData["AssistantTeachers3"] = assistantTeachers3.Select(gt => new SelectListItem {
			Text = $"{gt.FirstName} {gt.LastName}",
			Value = gt.Id
		});
		var guideTeacher = await this._userManager.FindByIdAsync(model.GuideTeacher!.ToString());
		if (guideTeacher is null) {
			this.ViewBag.ErrorMessage = "Error al obtener al profesor guía.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		if (guideTeacher.IsDeactivated) {
			this.ViewBag.ErrorMessage = $"El profesor guía candidato está desactivado.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		ApplicationUser? assistantTeacher1 = null;
		ApplicationUser? assistantTeacher2 = null;
		ApplicationUser? assistantTeacher3 = null;
		if (!model.AssistantTeacher1.IsNullOrEmpty()) {
			assistantTeacher1 = await this._userManager.FindByIdAsync(model.AssistantTeacher1!.ToString());
			if (assistantTeacher1 is null) {
				this.ViewBag.ErrorMessage = "Error al obtener al primer profesor co-guía.";
				return this.View(new CreateViewModel {
					Title = model.Title,
					Description = model.Description,
				});
			}
			if (assistantTeacher1.IsDeactivated) {
				this.ViewBag.ErrorMessage = $"El primer profesor co-guía está desactivado.";
				return this.View(new CreateViewModel {
					Title = model.Title,
					Description = model.Description,
				});
			}
		}
		if (!model.AssistantTeacher2.IsNullOrEmpty()) {
			assistantTeacher2 = await this._userManager.FindByIdAsync(model.AssistantTeacher2!.ToString());
			if (assistantTeacher2 is null) {
				this.ViewBag.ErrorMessage = "Error al obtener al segundo profesor co-guía.";
				return this.View(new CreateViewModel {
					Title = model.Title,
					Description = model.Description,
				});
			}
			if (assistantTeacher2.IsDeactivated) {
				this.ViewBag.ErrorMessage = $"El segundo profesor co-guía está desactivado.";
				return this.View(new CreateViewModel {
					Title = model.Title,
					Description = model.Description,
				});
			}
		}
		if (!model.AssistantTeacher3.IsNullOrEmpty()) {
			assistantTeacher3 = await this._userManager.FindByIdAsync(model.AssistantTeacher3!.ToString());
			if (assistantTeacher3 is null) {
				this.ViewBag.ErrorMessage = "Error al obtener al tercer profesor co-guía.";
				return this.View(new CreateViewModel {
					Title = model.Title,
					Description = model.Description,
				});
			}
			if (assistantTeacher3.IsDeactivated) {
				this.ViewBag.ErrorMessage = $"El tercer profesor co-guía está desactivado.";
				return this.View(new CreateViewModel {
					Title = model.Title,
					Description = model.Description,
				});
			}
		}
		if (assistantTeacher1 is not null && guideTeacher == assistantTeacher1) {
			this.ViewBag.WarningMessage = "El profesor guía no puede ser un profesor co-guía a la vez.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		if (assistantTeacher1 is not null && assistantTeacher2 is not null && assistantTeacher1 == assistantTeacher2) {
			this.ViewBag.WarningMessage = "El profesor co-guía no puede repetirse más de una vez.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		if (assistantTeacher1 is not null && assistantTeacher3 is not null && assistantTeacher1 == assistantTeacher3) {
			this.ViewBag.WarningMessage = "El profesor co-guía no puede repetirse más de una vez.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		if (assistantTeacher2 is not null && guideTeacher == assistantTeacher2) {
			this.ViewBag.WarningMessage = "El profesor guía no puede ser un profesor co-guía a la vez.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		if (assistantTeacher2 is not null && assistantTeacher1 is not null && assistantTeacher2 == assistantTeacher1) {
			this.ViewBag.WarningMessage = "El profesor co-guía no puede repetirse más de una vez.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		if (assistantTeacher2 is not null && assistantTeacher3 is not null && assistantTeacher2 == assistantTeacher3) {
			this.ViewBag.WarningMessage = "El profesor co-guía no puede repetirse más de una vez.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		if (assistantTeacher3 is not null && guideTeacher == assistantTeacher3) {
			this.ViewBag.WarningMessage = "El profesor guía no puede ser un profesor co-guía a la vez.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		if (assistantTeacher3 is not null && assistantTeacher1 is not null && assistantTeacher3 == assistantTeacher1) {
			this.ViewBag.WarningMessage = "El profesor co-guía no puede repetirse más de una vez.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		if (assistantTeacher3 is not null && assistantTeacher2 is not null && assistantTeacher3 == assistantTeacher2) {
			this.ViewBag.WarningMessage = "El profesor co-guía no puede repetirse más de una vez.";
			return this.View(new CreateViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		var studentProposal = new StudentProposal {
			Id = Guid.NewGuid().ToString(),
			Title = model.Title,
			Description = model.Description,
			StudentOwnerOfTheStudentProposal = student,
			GuideTeacherOfTheStudentProposal = guideTeacher,
			AssistantTeacher1OfTheStudentProposal = assistantTeacher1,
			AssistantTeacher2OfTheStudentProposal = assistantTeacher2,
			AssistantTeacher3OfTheStudentProposal = assistantTeacher3,
			ProposalStatus = StudentProposal.Status.Draft,
			CreatedAt = DateTimeOffset.Now,
			UpdatedAt = DateTimeOffset.Now
		};
		_ = await this._dbContext.StudentProposals.AddAsync(studentProposal);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "Tu propuesta ha sido registrada correctamente.";
		return this.RedirectToAction("Index", "Proposal", new { area = "Student" });
	}

	public async Task<IActionResult> Edit(string id) {
		var student = await this._userManager.GetUserAsync(this.User);
		if (student is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (student.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var guideTeachers = (
			await this._userManager.GetUsersInRoleAsync(Roles.GuideTeacher.ToString()))
			.Where(gt => !gt.IsDeactivated)
			.OrderBy(gt => gt.LastName)
			.ToList();
		var assistantTeachers1 = (
			await this._userManager.GetUsersInRoleAsync(Roles.AssistantTeacher.ToString()))
			.Where(gt => !gt.IsDeactivated)
			.OrderBy(at => at.LastName)
			.ToList();
		var assistantTeachers2 = (
			await this._userManager.GetUsersInRoleAsync(Roles.AssistantTeacher.ToString()))
			.Where(gt => !gt.IsDeactivated)
			.OrderBy(at => at.LastName)
			.ToList();
		var assistantTeachers3 = (
			await this._userManager.GetUsersInRoleAsync(Roles.AssistantTeacher.ToString()))
			.Where(gt => !gt.IsDeactivated)
			.OrderBy(at => at.LastName)
			.ToList();
		this.ViewData["GuideTeachers"] = guideTeachers.Select(gt => new SelectListItem {
			Text = $"{gt.FirstName} {gt.LastName}",
			Value = gt.Id
		});
		this.ViewData["AssistantTeachers1"] = assistantTeachers1.Select(gt => new SelectListItem {
			Text = $"{gt.FirstName} {gt.LastName}",
			Value = gt.Id
		});
		this.ViewData["AssistantTeachers2"] = assistantTeachers2.Select(gt => new SelectListItem {
			Text = $"{gt.FirstName} {gt.LastName}",
			Value = gt.Id
		});
		this.ViewData["AssistantTeachers3"] = assistantTeachers3.Select(gt => new SelectListItem {
			Text = $"{gt.FirstName} {gt.LastName}",
			Value = gt.Id
		});
		var studentProposal = await this._dbContext.StudentProposals.AsNoTracking()
			.Include(sp => sp.StudentOwnerOfTheStudentProposal).AsNoTracking()
			.Where(sp => sp.StudentOwnerOfTheStudentProposal == student && sp.ProposalStatus == StudentProposal.Status.Draft)
			.Include(sp => sp.GuideTeacherOfTheStudentProposal).AsNoTracking()
			.Include(sp => sp.AssistantTeacher1OfTheStudentProposal).AsNoTracking()
			.Include(sp => sp.AssistantTeacher2OfTheStudentProposal).AsNoTracking()
			.Include(sp => sp.AssistantTeacher3OfTheStudentProposal).AsNoTracking()
			.FirstOrDefaultAsync(sp => sp.Id == id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction("Index", "Proposal", new { area = "Student" });
		}
		var editViewModel = new EditViewModel {
			Id = id,
			Title = studentProposal!.Title,
			Description = studentProposal.Description,
			GuideTeacher = studentProposal.GuideTeacherOfTheStudentProposal!.Id,
			AssistantTeacher1 = studentProposal.AssistantTeacher1OfTheStudentProposal?.Id,
			AssistantTeacher2 = studentProposal.AssistantTeacher2OfTheStudentProposal?.Id,
			AssistantTeacher3 = studentProposal.AssistantTeacher3OfTheStudentProposal?.Id,
			CreatedAt = studentProposal.CreatedAt,
			UpdatedAt = studentProposal.UpdatedAt
		};
		return this.View(editViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit([FromForm] EditViewModel model) {
		var student = await this._userManager.GetUserAsync(this.User);
		if (student is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (student.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var guideTeachers = (
			await this._userManager.GetUsersInRoleAsync(Roles.GuideTeacher.ToString()))
			.Where(gt => !gt.IsDeactivated)
			.OrderBy(gt => gt.LastName)
			.ToList();
		var assistantTeachers1 = (
			await this._userManager.GetUsersInRoleAsync(Roles.AssistantTeacher.ToString()))
			.Where(gt => !gt.IsDeactivated)
			.OrderBy(at => at.LastName)
			.ToList();
		var assistantTeachers2 = (
			await this._userManager.GetUsersInRoleAsync(Roles.AssistantTeacher.ToString()))
			.Where(gt => !gt.IsDeactivated)
			.OrderBy(at => at.LastName)
			.ToList();
		var assistantTeachers3 = (
			await this._userManager.GetUsersInRoleAsync(Roles.AssistantTeacher.ToString()))
			.Where(gt => !gt.IsDeactivated)
			.OrderBy(at => at.LastName)
			.ToList();
		this.ViewData["GuideTeachers"] = guideTeachers.Select(gt => new SelectListItem {
			Text = $"{gt.FirstName} {gt.LastName}",
			Value = gt.Id
		});
		this.ViewData["AssistantTeachers1"] = assistantTeachers1.Select(gt => new SelectListItem {
			Text = $"{gt.FirstName} {gt.LastName}",
			Value = gt.Id
		});
		this.ViewData["AssistantTeachers2"] = assistantTeachers2.Select(gt => new SelectListItem {
			Text = $"{gt.FirstName} {gt.LastName}",
			Value = gt.Id
		});
		this.ViewData["AssistantTeachers3"] = assistantTeachers3.Select(gt => new SelectListItem {
			Text = $"{gt.FirstName} {gt.LastName}",
			Value = gt.Id
		});
		if (!this.ModelState.IsValid) {
			this.ViewBag.WarningMessage = "Revisa que los campos estén correctos.";
			return this.View(new EditViewModel {
				Title = model.Title,
				Description = model.Description,
			});
		}
		var studentProposal = await this._dbContext.StudentProposals
			.Include(sp => sp.StudentOwnerOfTheStudentProposal)
			.Include(sp => sp.GuideTeacherOfTheStudentProposal)
			.Include(sp => sp.AssistantTeacher1OfTheStudentProposal)
			.Include(sp => sp.AssistantTeacher2OfTheStudentProposal)
			.Include(sp => sp.AssistantTeacher3OfTheStudentProposal)
			.FirstOrDefaultAsync(sp => sp.Id == model.Id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction("Index", "Proposal", new { area = "Student" });
		}
		var guideTeacher = await this._userManager.FindByIdAsync(model.GuideTeacher!.ToString());
		if (guideTeacher is null) {
			this.ViewBag.ErrorMessage = "Error al obtener al profesor guía.";
			return this.View(new EditViewModel {
				Id = model.Id,
				Title = model.Title,
				Description = model.Description,
				CreatedAt = studentProposal.CreatedAt,
				UpdatedAt = studentProposal.UpdatedAt
			});
		}
		if (guideTeacher.IsDeactivated) {
			this.ViewBag.ErrorMessage = $"El profesor guía candidato está desactivado.";
			return this.View(new EditViewModel {
				Id = model.Id,
				Title = model.Title,
				Description = model.Description,
				CreatedAt = studentProposal.CreatedAt,
				UpdatedAt = studentProposal.UpdatedAt
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
		ApplicationUser? assistantTeacher1 = null;
		ApplicationUser? assistantTeacher2 = null;
		ApplicationUser? assistantTeacher3 = null;
		if (!model.AssistantTeacher1.IsNullOrEmpty()) {
			assistantTeacher1 = await this._userManager.FindByIdAsync(model.AssistantTeacher1!.ToString());
			if (assistantTeacher1 is null) {
				this.ViewBag.ErrorMessage = "Error al obtener al primer profesor co-guía.";
				return this.View(new EditViewModel {
					Id = model.Id,
					Title = model.Title,
					Description = model.Description,
					CreatedAt = studentProposal.CreatedAt,
					UpdatedAt = studentProposal.UpdatedAt
				});
			}
			if (assistantTeacher1.IsDeactivated) {
				this.ViewBag.ErrorMessage = $"El primer profesor co-guía candidato está desactivado.";
				return this.View(new EditViewModel {
					Id = model.Id,
					Title = model.Title,
					Description = model.Description,
					CreatedAt = studentProposal.CreatedAt,
					UpdatedAt = studentProposal.UpdatedAt
				});
			}
		}
		if (!model.AssistantTeacher2.IsNullOrEmpty()) {
			assistantTeacher2 = await this._userManager.FindByIdAsync(model.AssistantTeacher2!.ToString());
			if (assistantTeacher2 is null) {
				this.ViewBag.ErrorMessage = "Error al obtener al segundo profesor co-guía.";
				return this.View(new EditViewModel {
					Id = model.Id,
					Title = model.Title,
					Description = model.Description,
					CreatedAt = studentProposal.CreatedAt,
					UpdatedAt = studentProposal.UpdatedAt
				});
			}
			if (assistantTeacher2.IsDeactivated) {
				this.ViewBag.ErrorMessage = $"El segundo profesor co-guía candidato está desactivado.";
				return this.View(new EditViewModel {
					Id = model.Id,
					Title = model.Title,
					Description = model.Description,
					CreatedAt = studentProposal.CreatedAt,
					UpdatedAt = studentProposal.UpdatedAt
				});
			}
		}
		if (!model.AssistantTeacher3.IsNullOrEmpty()) {
			assistantTeacher3 = await this._userManager.FindByIdAsync(model.AssistantTeacher3!.ToString());
			if (assistantTeacher3 is null) {
				this.ViewBag.ErrorMessage = "Error al obtener al tercer profesor co-guía.";
				return this.View(new EditViewModel {
					Id = model.Id,
					Title = model.Title,
					Description = model.Description,
					CreatedAt = studentProposal.CreatedAt,
					UpdatedAt = studentProposal.UpdatedAt
				});
			}
			if (assistantTeacher3.IsDeactivated) {
				this.ViewBag.ErrorMessage = $"El tercer profesor co-guía candidato está desactivado.";
				return this.View(new EditViewModel {
					Id = model.Id,
					Title = model.Title,
					Description = model.Description,
					CreatedAt = studentProposal.CreatedAt,
					UpdatedAt = studentProposal.UpdatedAt
				});
			}
		}
		if (assistantTeacher1 is not null && guideTeacher == assistantTeacher1) {
			this.ViewBag.WarningMessage = "El profesor guía no puede ser un profesor co-guía a la vez.";
			return this.View(new EditViewModel {
				Id = model.Id,
				Title = model.Title,
				Description = model.Description,
				CreatedAt = studentProposal.CreatedAt,
				UpdatedAt = studentProposal.UpdatedAt
			});
		}
		if (assistantTeacher1 is not null && assistantTeacher2 is not null && assistantTeacher1 == assistantTeacher2) {
			this.ViewBag.WarningMessage = "El profesor co-guía no puede repetirse más de una vez.";
			return this.View(new EditViewModel {
				Id = model.Id,
				Title = model.Title,
				Description = model.Description,
				CreatedAt = studentProposal.CreatedAt,
				UpdatedAt = studentProposal.UpdatedAt
			});
		}
		if (assistantTeacher1 is not null && assistantTeacher3 is not null && assistantTeacher1 == assistantTeacher3) {
			this.ViewBag.WarningMessage = "El profesor co-guía no puede repetirse más de una vez.";
			return this.View(new EditViewModel {
				Id = model.Id,
				Title = model.Title,
				Description = model.Description,
				CreatedAt = studentProposal.CreatedAt,
				UpdatedAt = studentProposal.UpdatedAt
			});
		}
		if (assistantTeacher2 is not null && guideTeacher == assistantTeacher2) {
			this.ViewBag.WarningMessage = "El profesor guía no puede ser un profesor co-guía a la vez.";
			return this.View(new EditViewModel {
				Id = model.Id,
				Title = model.Title,
				Description = model.Description,
				CreatedAt = studentProposal.CreatedAt,
				UpdatedAt = studentProposal.UpdatedAt
			});
		}
		if (assistantTeacher2 is not null && assistantTeacher1 is not null && assistantTeacher2 == assistantTeacher1) {
			this.ViewBag.WarningMessage = "El profesor co-guía no puede repetirse más de una vez.";
			return this.View(new EditViewModel {
				Id = model.Id,
				Title = model.Title,
				Description = model.Description,
				CreatedAt = studentProposal.CreatedAt,
				UpdatedAt = studentProposal.UpdatedAt
			});
		}
		if (assistantTeacher2 is not null && assistantTeacher3 is not null && assistantTeacher2 == assistantTeacher3) {
			this.ViewBag.WarningMessage = "El profesor co-guía no puede repetirse más de una vez.";
			return this.View(new EditViewModel {
				Id = model.Id,
				Title = model.Title,
				Description = model.Description,
				CreatedAt = studentProposal.CreatedAt,
				UpdatedAt = studentProposal.UpdatedAt
			});
		}
		if (assistantTeacher3 is not null && guideTeacher == assistantTeacher3) {
			this.ViewBag.WarningMessage = "El profesor guía no puede ser un profesor co-guía a la vez.";
			return this.View(new EditViewModel {
				Id = model.Id,
				Title = model.Title,
				Description = model.Description,
				CreatedAt = studentProposal.CreatedAt,
				UpdatedAt = studentProposal.UpdatedAt
			});
		}
		if (assistantTeacher3 is not null && assistantTeacher1 is not null && assistantTeacher3 == assistantTeacher1) {
			this.ViewBag.WarningMessage = "El profesor co-guía no puede repetirse más de una vez.";
			return this.View(new EditViewModel {
				Id = model.Id,
				Title = model.Title,
				Description = model.Description,
				CreatedAt = studentProposal.CreatedAt,
				UpdatedAt = studentProposal.UpdatedAt
			});
		}
		if (assistantTeacher3 is not null && assistantTeacher2 is not null && assistantTeacher3 == assistantTeacher2) {
			this.ViewBag.WarningMessage = "El profesor co-guía no puede repetirse más de una vez.";
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
		_ = this._dbContext.StudentProposals.Update(studentProposal);
		_ = await this._dbContext.SaveChangesAsync();
		this.ViewBag.SuccessMessage = "Tu propuesta ha sido actualizada correctamente.";
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
		return this.View(editViewModel);
	}

	public async Task<IActionResult> Delete(string id) {
		var student = await this._userManager.GetUserAsync(this.User);
		if (student is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (student.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var studentProposal = await this._dbContext.StudentProposals.AsNoTracking()
			.Where(sp => sp.StudentOwnerOfTheStudentProposal == student && sp.ProposalStatus == StudentProposal.Status.Draft)
			.FirstOrDefaultAsync(sp => sp.Id == id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction("Index", "Proposal", new { area = "Student" });
		}
		var deleteViewModel = new DeleteViewModel {
			Id = id,
			Title = studentProposal.Title
		};
		return this.View(deleteViewModel);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete([FromForm] DeleteViewModel model) {
		var student = await this._userManager.GetUserAsync(this.User);
		if (student is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (student.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var studentProposal = await this._dbContext.StudentProposals.AsNoTracking()
			.Where(sp => sp.StudentOwnerOfTheStudentProposal == student && sp.ProposalStatus == StudentProposal.Status.Draft)
			.FirstOrDefaultAsync(sp => sp.Id == model.Id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction("Index", "Proposal", new { area = "Student" });
		}
		_ = this._dbContext.StudentProposals.Remove(studentProposal);
		_ = this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "Tu propuesta ha sido eliminada correctamente.";
		return this.RedirectToAction("Index", "Proposal", new { area = "Student" });
	}

	public async Task<IActionResult> Send(string id) {
		var student = await this._userManager.GetUserAsync(this.User);
		if (student is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (student.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var studentProposal = await this._dbContext.StudentProposals.AsNoTracking()
			.Where(sp => sp.StudentOwnerOfTheStudentProposal == student && sp.ProposalStatus == StudentProposal.Status.Draft)
			.Include(sp => sp.GuideTeacherOfTheStudentProposal)
			.FirstOrDefaultAsync(sp => sp.Id == id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction("Index", "Proposal", new { area = "Student" });
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
		var student = await this._userManager.GetUserAsync(this.User);
		if (student is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (student.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var studentProposal = await this._dbContext.StudentProposals.AsNoTracking()
			.Where(sp => sp.StudentOwnerOfTheStudentProposal == student && sp.ProposalStatus == StudentProposal.Status.Draft)
			.Include(sp => sp.GuideTeacherOfTheStudentProposal)
			.Include(sp => sp.AssistantTeacher1OfTheStudentProposal)
			.Include(sp => sp.AssistantTeacher2OfTheStudentProposal)
			.Include(sp => sp.AssistantTeacher3OfTheStudentProposal)
			.FirstOrDefaultAsync(sp => sp.Id == model.Id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction("Index", "Proposal", new { area = "Student" });
		}
		if (studentProposal.GuideTeacherOfTheStudentProposal is not null && studentProposal.GuideTeacherOfTheStudentProposal.IsDeactivated) {
			this.TempData["ErrorMessage"] = "El profesor guía está desactivado.";
			return this.RedirectToAction("Index", "Proposal", new { area = "Student" });
		}
		if (studentProposal.AssistantTeacher1OfTheStudentProposal is not null && studentProposal.AssistantTeacher1OfTheStudentProposal.IsDeactivated) {
			this.TempData["ErrorMessage"] = "El profesor co-guía 1 está desactivado.";
			return this.RedirectToAction("Index", "Proposal", new { area = "Student" });
		}
		if (studentProposal.AssistantTeacher2OfTheStudentProposal is not null && studentProposal.AssistantTeacher2OfTheStudentProposal.IsDeactivated) {
			this.TempData["ErrorMessage"] = "El profesor co-guía 2 está desactivado.";
			return this.RedirectToAction("Index", "Proposal", new { area = "Student" });
		}
		if (studentProposal.AssistantTeacher3OfTheStudentProposal is not null && studentProposal.AssistantTeacher3OfTheStudentProposal.IsDeactivated) {
			this.TempData["ErrorMessage"] = "El profesor co-guía 3 está desactivado.";
			return this.RedirectToAction("Index", "Proposal", new { area = "Student" });
		}
		studentProposal.ProposalStatus = StudentProposal.Status.Sent;
		studentProposal.UpdatedAt = DateTimeOffset.Now;
		_ = this._dbContext.StudentProposals.Update(studentProposal);
		_ = await this._dbContext.SaveChangesAsync();
		this.TempData["SuccessMessage"] = "Tu propuesta ha sido enviada correctamente.";
		return this.RedirectToAction("Index", "Proposal", new { area = "Student" });
	}

	public async Task<IActionResult> ViewRejectionReason(string id) {
		var student = await this._userManager.GetUserAsync(this.User);
		if (student is null) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		if (student.IsDeactivated) {
			return this.RedirectToAction("Index", "Home", new { area = "" });
		}
		var studentProposal = await this._dbContext.StudentProposals.AsNoTracking()
			.Where(sp => sp.StudentOwnerOfTheStudentProposal == student && sp.ProposalStatus == StudentProposal.Status.Rejected)
			.Include(sp => sp.GuideTeacherOfTheStudentProposal)
			.FirstOrDefaultAsync(sp => sp.Id == id);
		if (studentProposal is null) {
			this.TempData["ErrorMessage"] = "Error al obtener la propuesta.";
			return this.RedirectToAction("Index", "Proposal", new { area = "Student" });
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
}
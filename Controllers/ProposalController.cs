using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;
using Utal.Icc.Sgm.ViewModels;

using static Utal.Icc.Sgm.Models.ApplicationUser;
using static Utal.Icc.Sgm.Models.Proposal;

namespace Utal.Icc.Sgm.Controllers;

public abstract class ProposalController : ApplicationController, IPopulatable, ISortable {
	public abstract string[]? Parameters { get; set; }
	
	public ProposalController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore, SignInManager<ApplicationUser> signInManager) : base(dbContext, userManager, userStore, signInManager) { }
	
	public abstract void SetSortParameters(string sortOrder, params string[] parameters);
	
	public abstract Task PopulateAssistantTeachers(ApplicationUser guideTeacher);

	protected async Task<T?> CreateAsync<T>(string id) where T : ProposalViewModel, new() {
		if (await base.CheckApplicationUser(id) is not ApplicationUser guideTeacher) {
			return null;
		}
		var output = new T {
			GuideTeacherId = guideTeacher.Id,
			GuideTeacherName = $"{guideTeacher.FirstName} {guideTeacher.LastName}",
			GuideTeacherEmail = guideTeacher.Email,
			GuideTeacherOffice = guideTeacher.TeacherOffice,
			GuideTeacherSchedule = guideTeacher.TeacherSchedule,
			GuideTeacherSpecialization = guideTeacher.TeacherSpecialization,
		};
		return output;
	}

	protected async Task<Proposal?> CreateAsync<T>(T input, ApplicationUser user) where T : ProposalViewModel {
		if (await base.CheckApplicationUser(input.GuideTeacherId!) is not ApplicationUser guideTeacher) {
			return null;
		}
		var assistantTeachers = input.AssistantTeachers!.Select(async at => await base.CheckApplicationUser(at!)).Select(at => at.Result).ToList();
		var proposal = user switch {
			_ when await base._userManager.IsInRoleAsync(user, nameof(Roles.Student)) => new Proposal {
				Id = Guid.NewGuid().ToString(),
				Title = input.Title,
				Description = input.Description,
				GuideTeacherOfTheProposal = guideTeacher,
				StudentOfTheProposal = user,
				AssistantTeachersOfTheProposal = assistantTeachers!,
				ProposalStatus = Status.Draft,
				CreatedAt = DateTimeOffset.Now,
				UpdatedAt = DateTimeOffset.Now
			},
			_ when await base._userManager.IsInRoleAsync(user, nameof(Roles.GuideTeacher)) => new Proposal {
				Id = Guid.NewGuid().ToString(),
				Title = input.Title,
				Description = input.Description,
				WasMadeByGuideTeacher = true,
				Requirements = input.Requirements,
				GuideTeacherOfTheProposal = user,
				AssistantTeachersOfTheProposal = assistantTeachers!,
				ProposalStatus = Status.Draft,
				CreatedAt = DateTimeOffset.Now,
				UpdatedAt = DateTimeOffset.Now
			},
			_ => null
		};
		_ = await base._dbContext.Proposals!.AddAsync(proposal!);
		_ = await base._dbContext.SaveChangesAsync();
		return proposal;
	}

	protected async Task<T?> EditAsync<T>(string id, ApplicationUser user) where T : ProposalViewModel, new() {
		var proposal = user switch {
			_ when await base._userManager.IsInRoleAsync(user, nameof(Roles.Student)) => await base._dbContext.Proposals!.AsNoTracking()
				.Include(p => p.StudentOfTheProposal).AsNoTracking()
				.Where(p => p.StudentOfTheProposal == user && p.ProposalStatus == Status.Draft && !p.WasMadeByGuideTeacher)
				.Include(p => p.GuideTeacherOfTheProposal).AsNoTracking()
				.Include(p => p.AssistantTeachersOfTheProposal).AsNoTracking()
				.FirstOrDefaultAsync(p => p.Id == id),
			_ when await base._userManager.IsInRoleAsync(user, nameof(Roles.GuideTeacher)) => await base._dbContext.Proposals!.AsNoTracking()
				.Include(p => p.GuideTeacherOfTheProposal).AsNoTracking()
				.Where(p => p.GuideTeacherOfTheProposal == user && p.ProposalStatus == Status.Draft && p.WasMadeByGuideTeacher)
				.Include(p => p.AssistantTeachersOfTheProposal).AsNoTracking()
				.FirstOrDefaultAsync(p => p.Id == id),
			_ => null
		};
		if (proposal is null) {
			return null;
		}
		var output = user switch {
			_ when await base._userManager.IsInRoleAsync(user, nameof(Roles.Student)) => new T {
				Id = proposal!.Id,
				Title = proposal!.Title,
				Description = proposal.Description,
				GuideTeacherId = proposal.GuideTeacherOfTheProposal!.Id,
				GuideTeacherName = $"{proposal.GuideTeacherOfTheProposal.FirstName} {proposal.GuideTeacherOfTheProposal.LastName}",
				GuideTeacherEmail = proposal.GuideTeacherOfTheProposal.Email,
				GuideTeacherOffice = proposal.GuideTeacherOfTheProposal.TeacherOffice,
				GuideTeacherSchedule = proposal.GuideTeacherOfTheProposal.TeacherSchedule,
				GuideTeacherSpecialization = proposal.GuideTeacherOfTheProposal.TeacherSpecialization,
				AssistantTeachers = proposal.AssistantTeachersOfTheProposal!.Select(at => at!.Id).ToList(),
				CreatedAt = proposal.CreatedAt,
				UpdatedAt = proposal.UpdatedAt
			},
			_ when await base._userManager.IsInRoleAsync(user, nameof(Roles.GuideTeacher)) => new T {
				Id = proposal!.Id,
				Title = proposal!.Title,
				Description = proposal.Description,
				Requirements = proposal.Requirements,
				AssistantTeachers = proposal.AssistantTeachersOfTheProposal!.Select(at => at!.Id).ToList(),
				CreatedAt = proposal.CreatedAt,
				UpdatedAt = proposal.UpdatedAt
			},
			_ => null
		};
		return output;
	}

	protected async Task<T?> EditAsync<T>(T input, ApplicationUser user) where T : ProposalViewModel, new() {
		var proposal = user switch {
			_ when await base._userManager.IsInRoleAsync(user, nameof(Roles.Student)) => await base._dbContext.Proposals!
				.Include(p => p.StudentOfTheProposal)
				.Where(p => p.StudentOfTheProposal == user && p.ProposalStatus == Status.Draft && !p.WasMadeByGuideTeacher)
				.Include(p => p.AssistantTeachersOfTheProposal)
				.FirstOrDefaultAsync(p => p.Id == input.Id),
			_ when await base._userManager.IsInRoleAsync(user, nameof(Roles.GuideTeacher)) => await base._dbContext.Proposals!
				.Include(p => p.GuideTeacherOfTheProposal)
				.Where(p => p.GuideTeacherOfTheProposal == user && p.ProposalStatus == Status.Draft && p.WasMadeByGuideTeacher)
				.Include(p => p.AssistantTeachersOfTheProposal)
				.FirstOrDefaultAsync(p => p.Id == input.Id),
			_ => null
		};
		if (proposal is null) {
			return null;
		}
		proposal.Title = input.Title;
		proposal.Description = input.Description;
		proposal.AssistantTeachersOfTheProposal = input.AssistantTeachers!.Select(async at => await this.CheckApplicationUser(at!)).Select(at => at.Result).ToList()!;
		if (await base._userManager.IsInRoleAsync(user, nameof(Roles.GuideTeacher))) {
			proposal.Requirements = input.Requirements;
		}
		proposal.UpdatedAt = DateTimeOffset.Now;
		_ = base._dbContext.Proposals!.Update(proposal);
		_ = await base._dbContext.SaveChangesAsync();
		var output = user switch {
			_ when await base._userManager.IsInRoleAsync(user, nameof(Roles.Student)) => new T {
				Id = proposal!.Id,
				Title = proposal!.Title,
				Description = proposal.Description,
				AssistantTeachers = proposal.AssistantTeachersOfTheProposal!.Select(at => at!.Id).ToList(),
				CreatedAt = proposal.CreatedAt,
				UpdatedAt = proposal.UpdatedAt
			},
			_ when await base._userManager.IsInRoleAsync(user, nameof(Roles.GuideTeacher)) => new T {
				Id = proposal!.Id,
				Title = proposal!.Title,
				Description = proposal.Description,
				Requirements = proposal.Requirements,
				AssistantTeachers = proposal.AssistantTeachersOfTheProposal!.Select(at => at!.Id).ToList(),
				CreatedAt = proposal.CreatedAt,
				UpdatedAt = proposal.UpdatedAt
			},
			_ => null
		};
		return output;
	}

	protected async Task<T?> DeleteAsync<T>(string id, ApplicationUser user) where T : ProposalViewModel, new() {
		var proposal = user switch {
			_ when await base._userManager.IsInRoleAsync(user, nameof(Roles.Student)) => await this._dbContext.Proposals!.AsNoTracking()
				.Include(p => p.StudentOfTheProposal).AsNoTracking()
				.Where(p => p.StudentOfTheProposal == user && p.ProposalStatus == Status.Draft && !p.WasMadeByGuideTeacher)
				.FirstOrDefaultAsync(p => p.Id == id),
			_ when await base._userManager.IsInRoleAsync(user, nameof(Roles.GuideTeacher)) => await this._dbContext.Proposals!.AsNoTracking()
				.Include(p => p.GuideTeacherOfTheProposal).AsNoTracking()
				.Where(p => p.GuideTeacherOfTheProposal == user && p.ProposalStatus == Status.Draft && p.WasMadeByGuideTeacher)
				.FirstOrDefaultAsync(p => p.Id == id),
			_ => null
		};
		if (proposal is null) {
			return null;
		}
		var output = new T {
			Id = id,
			Title = proposal.Title
		};
		return output;
	}

	protected async Task<bool> DeleteAsync<T>(T input, ApplicationUser user) where T: ProposalViewModel, new() {
		var proposal = user switch {
			_ when await base._userManager.IsInRoleAsync(user, nameof(Roles.Student)) => await this._dbContext.Proposals!
				.Include(p => p.StudentOfTheProposal)
				.Where(p => p.StudentOfTheProposal == user && p.ProposalStatus == Status.Draft && !p.WasMadeByGuideTeacher)
				.FirstOrDefaultAsync(p => p.Id == input.Id),
			_ when await base._userManager.IsInRoleAsync(user, nameof(Roles.GuideTeacher)) => await this._dbContext.Proposals!
				.Include(p => p.GuideTeacherOfTheProposal)
				.Where(p => p.GuideTeacherOfTheProposal == user && p.ProposalStatus == Status.Draft && p.WasMadeByGuideTeacher)
				.FirstOrDefaultAsync(p => p.Id == input.Id),
			_ => null
		};
		if (proposal is null) {
			return false;
		}
		_ = base._dbContext.Proposals!.Remove(proposal);
		_ = base._dbContext.SaveChangesAsync();
		return true;
	}

	protected async Task<T?> ViewAsync<T>(string id, ApplicationUser user) where T : ProposalViewModel, new() {
		var proposal = user switch {
			_ when await base._userManager.IsInRoleAsync(user, nameof(Roles.Student)) => await base._dbContext.Proposals!
				.Include(p => p.StudentOfTheProposal)
				.Include(p => p.StudentsWhoAreInterestedInThisProposal)
				.Where(p => (p.StudentOfTheProposal == user || p.StudentsWhoAreInterestedInThisProposal!.Contains(user)) && p.ProposalStatus == Status.Published)
				.Include(p => p.GuideTeacherOfTheProposal)
				.Include(p => p.AssistantTeachersOfTheProposal)
				.FirstOrDefaultAsync(p => p.Id == id),
			_ when await base._userManager.IsInRoleAsync(user, nameof(Roles.GuideTeacher)) => await base._dbContext.Proposals!
				.Include(p => p.GuideTeacherOfTheProposal)
				.Where(p => p.GuideTeacherOfTheProposal == user && p.ProposalStatus == Status.Published)
				.Include(p => p.AssistantTeachersOfTheProposal)
				.FirstOrDefaultAsync(p => p.Id == id),
			_ => null
		};
		if (proposal is null) {
			return null;
		}
		var output = new T {
			Id = id,
			Title = proposal.Title,
			Description = proposal.Description,
			AssistantTeachers = proposal.AssistantTeachersOfTheProposal!.Select(at => $"{at!.FirstName} {at!.LastName}").ToList(),
			CreatedAt = proposal.CreatedAt,
			UpdatedAt = proposal.UpdatedAt
		};
		if (await base._userManager.IsInRoleAsync(user, nameof(Roles.Student))) {
			output.GuideTeacherId = proposal.GuideTeacherOfTheProposal!.Id;
			output.GuideTeacherName = $"{proposal.GuideTeacherOfTheProposal.FirstName} {proposal.GuideTeacherOfTheProposal.LastName}";
			output.GuideTeacherEmail = proposal.GuideTeacherOfTheProposal.Email;
			output.GuideTeacherOffice = proposal.GuideTeacherOfTheProposal.TeacherOffice;
			output.GuideTeacherSchedule = proposal.GuideTeacherOfTheProposal.TeacherSchedule;
			output.GuideTeacherSpecialization = proposal.GuideTeacherOfTheProposal.TeacherSpecialization;
		} else if (await base._userManager.IsInRoleAsync(user, nameof(Roles.GuideTeacher))) {
			output.Requirements = proposal.Requirements;
		}
		return output;
	}

	protected async Task<T?> SummaryAsync<T>(string id, ApplicationUser user) where T : ProposalViewModel, new() {
		var proposal = user switch {
			_ when await base._userManager.IsInRoleAsync(user, nameof(Roles.Student)) => await base._dbContext.Proposals!
				.Include(p => p.StudentOfTheProposal)
				.Include(p => p.StudentsWhoAreInterestedInThisProposal)
				.Where(p => (p.StudentOfTheProposal == user || p.StudentsWhoAreInterestedInThisProposal!.Contains(user)) && p.ProposalStatus == Status.Published)
				.Include(p => p.GuideTeacherOfTheProposal)
				.Include(p => p.AssistantTeachersOfTheProposal)
				.FirstOrDefaultAsync(p => p.Id == id),
			_ when await base._userManager.IsInRoleAsync(user, nameof(Roles.GuideTeacher)) => await base._dbContext.Proposals!
				.Include(p => p.GuideTeacherOfTheProposal)
				.Where(p => p.GuideTeacherOfTheProposal == user && p.ProposalStatus == Status.Published)
				.Include(p => p.StudentOfTheProposal)
				.Include(p => p.AssistantTeachersOfTheProposal)
				.FirstOrDefaultAsync(p => p.Id == id),
			_ => null
		};
		if (proposal is null) {
			return null;
		}
		var output = new T {
			Id = id,
			Title = proposal.Title,
			Description = proposal.Description,
			AssistantTeachers = proposal.AssistantTeachersOfTheProposal!.Select(at => $"{at!.FirstName} {at!.LastName}").ToList(),
			CreatedAt = proposal.CreatedAt,
			UpdatedAt = proposal.UpdatedAt
		};
		if (await base._userManager.IsInRoleAsync(user, nameof(Roles.Student))) {
			output.GuideTeacherId = proposal.GuideTeacherOfTheProposal!.Id;
			output.GuideTeacherName = $"{proposal.GuideTeacherOfTheProposal.FirstName} {proposal.GuideTeacherOfTheProposal.LastName}";
			output.GuideTeacherEmail = proposal.GuideTeacherOfTheProposal.Email;
			output.GuideTeacherOffice = proposal.GuideTeacherOfTheProposal.TeacherOffice;
			output.GuideTeacherSchedule = proposal.GuideTeacherOfTheProposal.TeacherSchedule;
			output.GuideTeacherSpecialization = proposal.GuideTeacherOfTheProposal.TeacherSpecialization;
		} else if (await base._userManager.IsInRoleAsync(user, nameof(Roles.GuideTeacher))) {
			output.Requirements = proposal.Requirements;
			output.StudentId = proposal.StudentOfTheProposal!.Id;
			output.StudentName = $"{proposal.StudentOfTheProposal.FirstName} {proposal.StudentOfTheProposal.LastName}";
			output.StudentEmail = proposal.StudentOfTheProposal.Email;
			output.StudentUniversityId = proposal.StudentOfTheProposal.StudentUniversityId;
			output.StudentRemainingCourses = proposal.StudentOfTheProposal.StudentRemainingCourses;
			output.StudentIsDoingThePractice = proposal.StudentOfTheProposal.StudentIsDoingThePractice;
			output.StudentIsWorking = proposal.StudentOfTheProposal.StudentIsWorking;
		}
		return output;
	}

	protected async Task<T?> PublishAsync<T>(string id, ApplicationUser user) where T : ProposalViewModel, new() {
		var proposal = user switch {
			_ when await base._userManager.IsInRoleAsync(user, nameof(Roles.Student)) => await base._dbContext.Proposals!.AsNoTracking()
				.Include(p => p.StudentOfTheProposal).AsNoTracking()
				.Where(p => p.StudentOfTheProposal == user && p.ProposalStatus == Status.Draft && !p.WasMadeByGuideTeacher)
				.FirstOrDefaultAsync(p => p.Id == id),
			_ when await base._userManager.IsInRoleAsync(user, nameof(Roles.GuideTeacher)) => await base._dbContext.Proposals!.AsNoTracking()
				.Include(p => p.GuideTeacherOfTheProposal).AsNoTracking()
				.Where(p => p.GuideTeacherOfTheProposal == user && p.ProposalStatus == Status.Draft && p.WasMadeByGuideTeacher)
				.FirstOrDefaultAsync(p => p.Id == id),
			_ => null
		};
		if (proposal is null) {
			return null;
		}
		var output = new T {
			Id = id,
			Title = proposal.Title,
		};
		return output;
	}

	protected async Task<bool> PublishAsync<T>(T input, ApplicationUser user) where T : ProposalViewModel, new() {
		var proposal = user switch {
			_ when await base._userManager.IsInRoleAsync(user, nameof(Roles.Student)) => await base._dbContext.Proposals!
				.Include(p => p.StudentOfTheProposal)
				.Where(p => p.StudentOfTheProposal == user && p.ProposalStatus == Status.Draft && !p.WasMadeByGuideTeacher)
				.Include(p => p.GuideTeacherOfTheProposal)
				.FirstOrDefaultAsync(p => p.Id == input.Id),
			_ when await base._userManager.IsInRoleAsync(user, nameof(Roles.GuideTeacher)) => await base._dbContext.Proposals!
				.Include(p => p.GuideTeacherOfTheProposal)
				.Where(p => p.GuideTeacherOfTheProposal == user && p.ProposalStatus == Status.Draft && p.WasMadeByGuideTeacher)
				.Include(p => p.StudentOfTheProposal)
				.FirstOrDefaultAsync(p => p.Id == input.Id),
			_ => null
		};
		if (proposal is null) {
			return false;
		}
		if (await base._userManager.IsInRoleAsync(user, nameof(Roles.Student)) && await base.CheckApplicationUser(proposal.StudentOfTheProposal!.Id) is null) {
			return false;
		} else if (await base._userManager.IsInRoleAsync(user, nameof(Roles.GuideTeacher)) && await base.CheckApplicationUser(proposal.GuideTeacherOfTheProposal!.Id) is null) {
			return false;
		}
		foreach (var assistantTeacher in proposal.AssistantTeachersOfTheProposal!) {
			if (await base.CheckApplicationUser(assistantTeacher!.Id) is null) {
				return false;
			}
		}
		proposal.ProposalStatus = Status.Published;
		proposal.UpdatedAt = DateTimeOffset.Now;
		_ = base._dbContext.Proposals!.Update(proposal);
		_ = await base._dbContext.SaveChangesAsync();
		return true;
	}
}
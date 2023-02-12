using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;
using Utal.Icc.Sgm.ViewModels;

using static Utal.Icc.Sgm.Models.Proposal;

namespace Utal.Icc.Sgm.Controllers;

public abstract class ProposalController : ApplicationController, IPopulatable, ISortable {
	public abstract string[]? Parameters { get; set; }
	
	public ProposalController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore, SignInManager<ApplicationUser> signInManager) : base(dbContext, userManager, userStore, signInManager) { }
	
	public abstract void SetSortParameters(string sortOrder, params string[] parameters);
	
	public abstract Task PopulateAssistantTeachers(ApplicationUser guideTeacher);

	protected T Create<T>() where T : ProposalViewModel, new() => new T();

	protected async Task<Proposal?> CreateAsync<T>(T input, ApplicationUser user) where T : ProposalViewModel {
		var assistantTeachers = input.AssistantTeachers!.Select(async at => await base.CheckApplicationUser(at!)).Select(at => at.Result).ToList();
		var proposal = new Proposal {
			Id = Guid.NewGuid().ToString(),
			Title = input.Title,
			Description = input.Description,
			Requirements = input.Requirements,
			GuideTeacherOfTheProposal = user,
			AssistantTeachersOfTheProposal = assistantTeachers!,
			ProposalStatus = Status.Draft,
			CreatedAt = DateTimeOffset.Now,
			UpdatedAt = DateTimeOffset.Now
		};
		_ = await base._dbContext.Proposals!.AddAsync(proposal);
		_ = await base._dbContext.SaveChangesAsync();
		return proposal;
	}

	protected async Task<T?> EditAsync<T>(string id, ApplicationUser user) where T : ProposalViewModel, new() {
		var proposal = await base._dbContext.Proposals!.AsNoTracking()
			.Include(p => p.GuideTeacherOfTheProposal).AsNoTracking()
			.Where(p => p.GuideTeacherOfTheProposal == user && p.ProposalStatus == Status.Draft)
			.Include(p => p.AssistantTeachersOfTheProposal).AsNoTracking()
			.FirstOrDefaultAsync(p => p.Id == id);
		if (proposal is null) {
			return null;
		}
		await this.PopulateAssistantTeachers(proposal.GuideTeacherOfTheProposal!);
		var output = new T {
			Id = id,
			Title = proposal!.Title,
			Description = proposal.Description,
			Requirements = proposal.Requirements,
			AssistantTeachers = proposal.AssistantTeachersOfTheProposal?.Select(at => at!.Id).ToList(),
			CreatedAt = proposal.CreatedAt,
			UpdatedAt = proposal.UpdatedAt
		};
		return output;
	}

	protected async Task<T?> EditAsync<T>(ProposalViewModel input, ApplicationUser user) where T : ProposalViewModel, new() {
		var proposal = await base._dbContext.Proposals!
			.Include(p => p.GuideTeacherOfTheProposal)
			.Where(p => p.GuideTeacherOfTheProposal == user && p.ProposalStatus == Status.Draft)
			.Include(p => p.AssistantTeachersOfTheProposal)
			.FirstOrDefaultAsync(p => p.Id == input.Id);
		if (proposal is null) {
			return null;
		}
		await this.PopulateAssistantTeachers(proposal.GuideTeacherOfTheProposal!);
		proposal.Title = input.Title;
		proposal.Description = input.Description;
		proposal.Requirements = input.Requirements;
		var assistantTeachers = input.AssistantTeachers!.Select(async at => await this.CheckApplicationUser(at!)).Select(at => at.Result).ToList();
		proposal.AssistantTeachersOfTheProposal = assistantTeachers!;
		proposal.UpdatedAt = DateTimeOffset.Now;
		_ = base._dbContext.Proposals!.Update(proposal);
		_ = await base._dbContext.SaveChangesAsync();
		var output = new T {
			Id = proposal!.Id,
			Title = proposal!.Title,
			Description = proposal.Description,
			Requirements = proposal.Requirements,
			AssistantTeachers = proposal.AssistantTeachersOfTheProposal!.Select(at => at!.Id).ToList(),
			CreatedAt = proposal.CreatedAt,
			UpdatedAt = proposal.UpdatedAt
		};
		return output;
	}

	protected async Task<T?> DeleteAsync<T>(string id, ApplicationUser user) where T : ProposalViewModel, new() {
		var proposal = await this._dbContext.Proposals!.AsNoTracking()
			.Where(p => p.GuideTeacherOfTheProposal == user && p.ProposalStatus == Status.Draft)
			.FirstOrDefaultAsync(p => p.Id == id);
		if (proposal is null) {
			return null;
		}
		var output = new T {
			Id = id,
			Title = proposal.Title
		};
		return output;
	}

	protected async Task<bool> DeleteAsync<T>(ProposalViewModel input, ApplicationUser user) where T: ProposalViewModel, new() {
		var proposal = await this._dbContext.Proposals!
			.Where(p => p.GuideTeacherOfTheProposal == user && p.ProposalStatus == Status.Draft)
			.FirstOrDefaultAsync(p => p.Id == input.Id);
		if (proposal is null) {
			return false;
		}
		_ = base._dbContext.Proposals!.Remove(proposal);
		_ = base._dbContext.SaveChangesAsync();
		return true;
	}
}
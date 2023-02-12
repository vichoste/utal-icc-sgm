using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Utal.Icc.Sgm.Data;
using Utal.Icc.Sgm.Models;
using Utal.Icc.Sgm.ViewModels;

using static Utal.Icc.Sgm.Models.ApplicationUser;

namespace Utal.Icc.Sgm.Controllers;

public abstract class ProposalController : ApplicationController, IPopulatable, ISortable {
	public abstract string[]? Parameters { get; set; }
	
	public ProposalController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore, SignInManager<ApplicationUser> signInManager) : base(dbContext, userManager, userStore, signInManager) { }
	
	public abstract void SetSortParameters(string sortOrder, params string[] parameters);
	
	public abstract Task PopulateAssistantTeachers(ApplicationUser guideTeacher);
	
}
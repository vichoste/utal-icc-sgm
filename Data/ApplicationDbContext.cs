using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser> {
	public DbSet<StudentProposal>? StudentProposals { get; set; }
	public DbSet<GuideTeacherProposal>? GuideTeacherProposals { get; set; }

	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {
	}
}
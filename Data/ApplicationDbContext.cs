using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser> {
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {
	}

	public DbSet<TeacherProfile> TeacherProfiles { get; set; }
	public DbSet<StudentProfile> StudentProfiles { get; set; }
}
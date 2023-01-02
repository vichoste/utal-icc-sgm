using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Utal.Icc.Sgm.Areas.Account.Models;

namespace Utal.Icc.Sgm.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser> {
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {
	}
}
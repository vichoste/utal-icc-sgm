﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser> {
	public DbSet<Proposal>? Proposals { get; set; }

	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {
	}
}
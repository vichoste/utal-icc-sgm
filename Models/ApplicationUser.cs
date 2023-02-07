using Microsoft.AspNetCore.Identity;

namespace Utal.Icc.Sgm.Models;

public partial class ApplicationUser : IdentityUser {
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? Rut { get; set; }
	public DateTimeOffset CreatedAt { get; set; }
	public DateTimeOffset? UpdatedAt { get; set; }
	public bool IsDeactivated { get; set; } 
}
using Microsoft.AspNetCore.Identity;

namespace Utal.Icc.Sgm.Models;

public class ApplicationUser : IdentityUser {
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? Rut { get; set; }
	public virtual TeacherProfile? TeacherProfile { get; set; }
	public virtual StudentProfile? StudentProfile { get; set; }
}
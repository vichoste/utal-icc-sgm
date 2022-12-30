using Microsoft.AspNetCore.Identity;

namespace Utal.Icc.Sgm.Models;

public class ApplicationUser : IdentityUser {
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public virtual TeacherProfile? TeacherInformation { get; set; }
	public virtual StudentProfile? StudentProfile { get; set; }
}
using Microsoft.AspNetCore.Identity;

namespace Utal.Icc.Sgm.Models;

public class ApplicationUser : IdentityUser {
	#region Common
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? Rut { get; set; }
	#endregion
	#region Student
	public string? UniversityId { get; set; }
	public string? RemainingCourses { get; set; }
	public bool IsDoingThePractice { get; set; }
	public bool IsWorking { get; set; }
	public virtual ICollection<StudentProposal>? StudentProposals { get; set; }
	#endregion
	#region Teacher
	public string? Office { get; set; }
	public string? Schedule { get; set; }
	public string? Specialization { get; set; }
	#endregion
}
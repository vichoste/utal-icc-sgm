using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Identity;

namespace Utal.Icc.Sgm.Models;

public class ApplicationUser : IdentityUser {
	#region Common
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? Rut { get; set; }
	public bool IsDeactivated { get; set; }
	[InverseProperty(nameof(Qualification.Owners))]
	public virtual ICollection<Qualification?>? Qualifications { get; set; } = new HashSet<Qualification?>();
	public DateTimeOffset CreatedAt { get; set; }
	public DateTimeOffset? UpdatedAt { get; set; }
	#endregion
	#region Student
	public string? StudentUniversityId { get; set; }
	public string? StudentRemainingCourses { get; set; }
	public bool StudentIsDoingThePractice { get; set; }
	public bool StudentIsWorking { get; set; }
	[InverseProperty(nameof(Qualification.Candidates))]
	public virtual ICollection<Qualification?>? StudentCandidatures { get; set; } = new HashSet<Qualification?>();
	#endregion
	#region Teacher
	public string? TeacherOffice { get; set; }
	public string? TeacherSchedule { get; set; }
	public string? TeacherSpecialization { get; set; }
	#endregion
	#region Rejection
	[InverseProperty(nameof(Qualification.WhoRejected))]
	public virtual ICollection<Qualification?>? RejectedQualifications { get; set; } = new HashSet<Qualification?>();
	#endregion
}
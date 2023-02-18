using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Identity;

namespace Utal.Icc.Sgm.Models;

public class ApplicationUser : IdentityUser {
	#region Common
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? Rut { get; set; }
	public bool IsDeactivated { get; set; }
	public DateTimeOffset CreatedAt { get; set; }
	public DateTimeOffset? UpdatedAt { get; set; }
	#endregion
	#region Student
	public string? UniversityId { get; set; }
	public string? RemainingCourses { get; set; }
	public bool IsDoingThePractice { get; set; }
	public bool IsWorking { get; set; }
	[InverseProperty("Memorist")]
	public virtual ICollection<Memoir?>? Doing { get; set; } = new HashSet<Memoir?>();
	[InverseProperty("Candidates")]
	public virtual ICollection<Memoir?>? Candidatures { get; set; } = new HashSet<Memoir?>();
	#endregion
	#region Teacher
	public string? Office { get; set; }
	public string? Schedule { get; set; }
	public string? Specialization { get; set; }
	[InverseProperty("Guide")]
	public virtual ICollection<Memoir?>? Guiding { get; set; } = new HashSet<Memoir?>();
	[InverseProperty("Assistants")]
	public virtual ICollection<Memoir?>? Assisting { get; set; } = new HashSet<Memoir?>();
	#endregion
	#region Rejection
	[InverseProperty("WhoRejected")]
	public virtual ICollection<Memoir?>? Rejections { get; set; } = new HashSet<Memoir?>();
	#endregion
}

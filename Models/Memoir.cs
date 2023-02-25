using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Utal.Icc.Sgm.Models;

public class Memoir {
	#region Common
	[Key]
	public string? Id { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
	public Phase? Phase { get; set; }
	[InverseProperty("Memoir")]
	public ICollection<Vote?>? Votes { get; set; } = new HashSet<Vote?>();
	public ApplicationUser? Owner { get; set; }
	public DateTimeOffset? CreatedAt { get; set; }
	public DateTimeOffset? UpdatedAt { get; set; }
	[Timestamp]
	public byte[]? RowVersion { get; set; }
	#endregion
	#region Student
	public virtual ApplicationUser? Memorist { get; set; }
	public virtual ICollection<ApplicationUser?>? Candidates { get; set; } = new HashSet<ApplicationUser?>();
	#endregion
	#region GuideTeacher
	public virtual ApplicationUser? Guide { get; set; }
	public string? Requirements { get; set; }
	#endregion
	#region Assistants
	public virtual ICollection<ApplicationUser?>? Assistants { get; set; } = new HashSet<ApplicationUser?>();
	#endregion
	#region Rejection
	public virtual ApplicationUser? WhoRejected { get; set; }
	public string? Reason { get; set; }
	#endregion
}
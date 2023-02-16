using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Models;

public class Memoir {
	#region Common
	[Key]
	public string? Id { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
	public Phase? Phase { get; set; }
	public virtual ICollection<ApplicationUser?> Owners { get; set; } = new HashSet<ApplicationUser?>();
	public DateTimeOffset? CreatedAt { get; set; }
	public DateTimeOffset? UpdatedAt { get; set; }
	[Timestamp]
	public byte[]? RowVersion { get; set; }
	#endregion
	#region Student
	public virtual ICollection<ApplicationUser?>? Candidates { get; set; } = new HashSet<ApplicationUser?>();
	#endregion
	#region GuideTeacher
	public string? Requirements { get; set; }
	#endregion
	#region Rejection
	public virtual ApplicationUser? WhoRejected { get; set; }
	public string? Reason { get; set; }
	#endregion
}
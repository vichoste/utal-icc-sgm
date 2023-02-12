using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Models;

public partial class Proposal {
	public enum Status {
		Draft,
		Published,
		Rejected,
		Ready
	}
	public enum Authors {
		GuideTeacher,
		Student
	}
	[Key]
	public string? Id { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
	public Authors WhoIsTheAuthor { get; set; }
	public virtual ICollection<ApplicationUser?>? AssistantTeachersOfTheProposal { get; set; } = new HashSet<ApplicationUser?>();
	public DateTimeOffset? CreatedAt { get; set; }
	public DateTimeOffset? UpdatedAt { get; set; }
	[Timestamp]
	public byte[]? RowVersion { get; set; }
}
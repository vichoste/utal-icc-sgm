using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Models;

public partial class Proposal {
	public enum Status {
		Draft,
		Published,
		Rejected,
		Ready
	}
	[Key]
	public string? Id { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
	public bool WasMadeByGuideTeacher { get; set; }
	public Status? ProposalStatus { get; set; }
	public virtual ICollection<ApplicationUser?>? AssistantTeachersOfTheProposal { get; set; } = new HashSet<ApplicationUser?>();
	public DateTimeOffset? CreatedAt { get; set; }
	public DateTimeOffset? UpdatedAt { get; set; }
	[Timestamp]
	public byte[]? RowVersion { get; set; }
}
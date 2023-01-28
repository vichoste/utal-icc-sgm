using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Models;

public class StudentProposal {
	public enum Status {
		Draft,
		Sent,
		Accepted,
		Confirmed,
		Rejected
	}
	[Key]
	public Guid Id { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
	public Status? ProposalStatus { get; set; }
	public DateTimeOffset? CreatedAt { get; set; }
	public DateTimeOffset? UpdatedAt { get; set; }
	public virtual ApplicationUser? StudentOwnerOfTheStudentProposal { get; set; }
	public virtual ApplicationUser? GuideTeacherCandidateOfTheStudentProposal { get; set; }
	public virtual ICollection<ApplicationUser>? AssistantTeachersCandidatesOfTheStudentProposal { get; set; } = new HashSet<ApplicationUser>();
	[Timestamp]
	public byte[]? RowVersion { get; set; }
}
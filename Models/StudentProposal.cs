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
	public virtual ApplicationUser? GuideTeacherOfTheStudentProposal { get; set; }
	public virtual ApplicationUser? AssistantTeacherOfTheStudentProposal1 { get; set; }
	public virtual ApplicationUser? AssistantTeacherOfTheStudentProposal2 { get; set; }
	public virtual ApplicationUser? AssistantTeacherOfTheStudentProposal3 { get; set; }
	[Timestamp]
	public byte[]? RowVersion { get; set; }
}
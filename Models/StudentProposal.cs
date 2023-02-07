using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Models;

public class StudentProposal {
	public enum Status {
		Draft,
		SentToGuideTeacher,
		ApprovedByGuideTeacher,
		RejectedByGuideTeacher
	}
	[Key]
	public string? Id { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
	public Status? ProposalStatus { get; set; }
	public DateTimeOffset? CreatedAt { get; set; }
	public DateTimeOffset? UpdatedAt { get; set; }
	public virtual ApplicationUser? StudentOwnerOfTheStudentProposal { get; set; }
	public virtual ApplicationUser? GuideTeacherOfTheStudentProposal { get; set; }
	public virtual ApplicationUser? AssistantTeacher1OfTheStudentProposal { get; set; }
	public virtual ApplicationUser? AssistantTeacher2OfTheStudentProposal { get; set; }
	public virtual ApplicationUser? AssistantTeacher3OfTheStudentProposal { get; set; }
	public string? RejectionReason { get; set; }
	[Timestamp]
	public byte[]? RowVersion { get; set; }
}
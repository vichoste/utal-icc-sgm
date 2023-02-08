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
	public virtual ICollection<ApplicationUser?>? AssistantTeachersOfTheStudentProposal { get; set; } = new HashSet<ApplicationUser>();
	public string? RejectionReason { get; set; }
	[Timestamp]
	public byte[]? RowVersion { get; set; }
}
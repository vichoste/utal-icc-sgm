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
	#region Common
	[Key]
	public string? Id { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
	public Status? ProposalStatus { get; set; }
	public DateTimeOffset? CreatedAt { get; set; }
	public DateTimeOffset? UpdatedAt { get; set; }
	#endregion
	public virtual ApplicationUser? StudentOwnerOfTheStudentProposal { get; set; }
	public virtual ApplicationUser? GuideTeacherOfTheStudentProposal { get; set; }
	public virtual ApplicationUser? AssistantTeacher1OfTheStudentProposal { get; set; }
	public virtual ApplicationUser? AssistantTeacher2OfTheStudentProposal { get; set; }
	public virtual ApplicationUser? AssistantTeacher3OfTheStudentProposal { get; set; }
	#region Rejected
	public string? RejectionReason { get; set; }
	#endregion
	#region Accepted
	public bool ConfirmedByStudent { get; set; }
	public bool ConfirmedByGuideTeacher { get; set; }
	#endregion
	[Timestamp]
	public byte[]? RowVersion { get; set; }
}
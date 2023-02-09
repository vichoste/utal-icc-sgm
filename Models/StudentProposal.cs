namespace Utal.Icc.Sgm.Models;

public class StudentProposal : Proposal {
	public enum Status {
		Draft,
		SentToGuideTeacher,
		ApprovedByGuideTeacher,
		RejectedByGuideTeacher
	}
	public Status? ProposalStatus { get; set; }
	public virtual ApplicationUser? StudentOwnerOfTheStudentProposal { get; set; }
	public virtual ApplicationUser? GuideTeacherOfTheStudentProposal { get; set; }
	public virtual ICollection<ApplicationUser?>? AssistantTeachersOfTheStudentProposal { get; set; } = new HashSet<ApplicationUser?>();
	public virtual ApplicationUser? GuideTeacherWhoRejectedThisStudentProposal { get; set; }
	public string? RejectionReason { get; set; }
}
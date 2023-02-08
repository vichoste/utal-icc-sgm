using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Models;

public class GuideTeacherProposal : Proposal {
	public enum Status {
		Draft,
		Published,
		Ready
	}
	public string? Requirements { get; set; }
	public Status? ProposalStatus { get; set; }
	public virtual ApplicationUser? GuideTeacherOwnerOfTheGuideTeacherProposal { get; set; }
	public virtual ICollection<ApplicationUser?>? AssistantTeachersOfTheGuideTeacherProposal { get; set; } = new HashSet<ApplicationUser?>();
	public virtual ICollection<ApplicationUser?>? StudentsWhoAreInterestedInThisGuideTeacherProposal { get; set; } = new HashSet<ApplicationUser?>();
	public virtual ApplicationUser? StudentWhoIsAssignedToThisGuideTeacherProposal { get; set; }
}
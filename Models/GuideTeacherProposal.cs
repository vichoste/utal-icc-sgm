using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Models;

public class GuideTeacherProposal : Proposal {
	public enum Status {
		Draft,
		Published,
		Ready
	}
	[Key]
	public string? Requirements { get; set; }
	public Status? ProposalStatus { get; set; }
	public virtual ApplicationUser? GuideTeacherOwnerOfTheGuideTeacherProposal { get; set; }
	public virtual ICollection<ApplicationUser?>? StudentsWhichAreInterestedInThisGuideTeacherProposal { get; set; } = new HashSet<ApplicationUser?>();
	public virtual ICollection<ApplicationUser?>? AssistantTeachersOfTheGuideTeacherProposal { get; set; } = new HashSet<ApplicationUser?>();
	public virtual ApplicationUser? StudentWhichIsAssignedToThisGuideTeacherProposal { get; set; }
}
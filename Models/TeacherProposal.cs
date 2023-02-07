using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Models;

public class TeacherProposal {
	public enum Status {
		Draft,
		Published
	}
	[Key]
	public string? Id { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
	public string? Requirements { get; set; }
	public Status? ProposalStatus { get; set; }
	public DateTimeOffset? CreatedAt { get; set; }
	public DateTimeOffset? UpdatedAt { get; set; }
	public virtual ApplicationUser? GuideTeacherOwnerOfTheTeacherProposal { get; set; }
	public virtual ICollection<ApplicationUser>? StudentsWhichAreInterestedInThisTeacherProposal { get; set; } = new HashSet<ApplicationUser>();
	public virtual ApplicationUser? AssistantTeacher1OfTheTeacherProposal { get; set; }
	public virtual ApplicationUser? AssistantTeacher2OfTheTeacherProposal { get; set; }
	public virtual ApplicationUser? AssistantTeacher3OfTheTeacherProposal { get; set; }
	[Timestamp]
	public byte[]? RowVersion { get; set; }
}
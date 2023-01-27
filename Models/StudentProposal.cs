using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Models;

public class StudentProposal {
	[Key]
	public Guid Id { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
	public bool IsDraft { get; set; }
	public virtual ApplicationUser? StudentProposalOwner { get; set; }
	public virtual ApplicationUser? GuideTeacher { get; set; }
	public virtual ICollection<ApplicationUser>? AssistantTeachers { get; set; }
}
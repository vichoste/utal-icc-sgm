using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Models;

public class StudentProposal {
	[Key]
	public Guid Id { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
	public bool IsDraft { get; set; }
	public bool IsPending { get; set; }
	public bool IsAccepted { get; set; }
	public virtual ApplicationUser? StudentOwnerOfTheStudentProposal { get; set; }
	public virtual ApplicationUser? GuideTeacherCandidateOfTheStudentProposal { get; set; }
	public virtual ICollection<ApplicationUser>? AssistantTeachersCandidatesOfTheStudentProposal { get; set; }
}
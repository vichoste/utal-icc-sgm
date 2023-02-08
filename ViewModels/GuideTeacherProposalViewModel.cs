using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.ViewModels;

public class GuideTeacherProposal : ProposalViewModel {
	[Display(Name = "Requisitos")]
	public virtual string? Requirements { get; set; }
	[Display(Name = "Profesores co-guía")]
	public virtual ICollection<string>? AssistantTeachers { get; set; } = new HashSet<string>();
}
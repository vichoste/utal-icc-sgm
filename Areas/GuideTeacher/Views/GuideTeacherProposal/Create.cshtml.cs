using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Student.Views.GuideTeacherProposal;

public class CreateViewModel {
	[Display(Name = "Título"), Required]
	public string? Title { get; set; }
	[Display(Name = "Descripción"), Required]
	public string? Description { get; set; }
	[Display(Name = "Requisitos"), Required]
	public string? Requirements { get; set; }
	[Display(Name = "Primer profesor co-guía")]
	public string? AssistantTeacher1 { get; set; }
	[Display(Name = "Segundo profesor co-guía")]
	public string? AssistantTeacher2 { get; set; }
	[Display(Name = "Tercer profesor co-guía")]
	public string? AssistantTeacher3 { get; set; }
}
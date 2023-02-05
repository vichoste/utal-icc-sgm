using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Student.Views.StudentProposal;

public class EditViewModel {
	[Display(Name = "ID"), Required]
	public string? Id { get; set; }
	[Display(Name = "Título")]
	public string? Title { get; set; }
	[Display(Name = "Descripción")]
	public string? Description { get; set; }
	[Display(Name = "Profesor guía"), Required]
	public string? GuideTeacher { get; set; }
	[Display(Name = "Primer profesor co-guía")]
	public string? AssistantTeacher1 { get; set; }
	[Display(Name = "Segundo profesor co-guía")]
	public string? AssistantTeacher2 { get; set; }
	[Display(Name = "Tercer profesor co-guía")]
	public string? AssistantTeacher3 { get; set; }
	[Display(Name = "Creado"), Required]
	public DateTimeOffset? CreatedAt { get; set; }
	[Display(Name = "Actualizado"), Required]
	public DateTimeOffset? UpdatedAt { get; set; }
}
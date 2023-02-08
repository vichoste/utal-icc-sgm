using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Student.Views.StudentProposal;

public class EditViewModel {
	[Display(Name = "ID"), Required]
	public string? Id { get; set; }
	[Display(Name = "Título"), Required]
	public string? Title { get; set; }
	[Display(Name = "Descripción")]
	public string? Description { get; set; }
	[Display(Name = "Profesor guía"), Required]
	public string? GuideTeacher { get; set; }
	[Display(Name = "Profesores co-guía"), Required]
	public ICollection<string>? AssistantTeachers { get; set; } = new HashSet<string>();
	[Display(Name = "Creado"), Required]
	public DateTimeOffset? CreatedAt { get; set; }
	[Display(Name = "Actualizado"), Required]
	public DateTimeOffset? UpdatedAt { get; set; }
}
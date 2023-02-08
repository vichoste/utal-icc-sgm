using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Student.Views.StudentProposal;

public class CreateViewModel {
	[Display(Name = "Título"), Required]
	public string? Title { get; set; }
	[Display(Name = "Descripción"), Required]
	public string? Description { get; set; }
	[Display(Name = "Profesor guía"), Required]
	public string? GuideTeacher { get; set; }
	[Display(Name = "Profesores co-guía"), Required]
	public ICollection<string>? AssistantTeachers { get; set; } = new HashSet<string>();
}
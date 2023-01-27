using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Student.Views.Proposal;

public class CreateViewModel {
	[Display(Name = "Título"), Required]
	public string? Title { get; set; }
	[Display(Name = "Descripción"), Required]
	public string? Description { get; set; }
	[Display(Name = "Profesor guía"), Required]
	public IEnumerable<GuideTeacherViewModel>? GuideTeachers { get; set; } = new HashSet<GuideTeacherViewModel>();
	[Display(Name = "Primer profesor co-guía")]
	public IEnumerable<AssistantTeacherViewModel>? AssistantTeachers1 { get; set; } = new HashSet<AssistantTeacherViewModel>();
	[Display(Name = "Segundo profesor co-guía")]
	public IEnumerable<AssistantTeacherViewModel>? AssistantTeachers2 { get; set; } = new HashSet<AssistantTeacherViewModel>();
}

public class GuideTeacherViewModel {
	public string? Id { get; set; }
	public string? Name { get; set; }
}

public class AssistantTeacherViewModel {
	public string? Id { get; set; }
	public string? Name { get; set; }
}
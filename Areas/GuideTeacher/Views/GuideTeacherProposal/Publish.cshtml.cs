using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.GuideTeacher.Views.GuideTeacherProposal;

public class PublishViewModel {
	[Display(Name = "ID"), Required]
	public string? Id { get; set; }
	[Display(Name = "Título de la propuesta"), Required]
	public string? Title { get; set; }
}
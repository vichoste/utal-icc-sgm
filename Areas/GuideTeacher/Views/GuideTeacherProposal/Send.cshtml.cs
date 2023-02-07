using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Student.Views.GuideTeacherProposal;

public class SendViewModel {
	[Display(Name = "ID"), Required]
	public string? Id { get; set; }
	[Display(Name = "Título de la propuesta"), Required]
	public string? Title { get; set; }
	[Display(Name = "Nombre del profesor guía"), Required]
	public string? GuideTeacherName { get; set; }
}
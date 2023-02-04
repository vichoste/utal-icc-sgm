using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.GuideTeacher.Views.Proposal;

public class RejectViewModel {
	[Display(Name = "ID"), Required]
	public string? Id { get; set; }
	[Display(Name = "Título de la propuesta"), Required]
	public string? Title { get; set; }
	[Display(Name = "Estudiante"), Required]
	public string? Student { get; set; }
	[Display(Name = "Justificación"), Required]
	public string? Reason { get; set; }
}
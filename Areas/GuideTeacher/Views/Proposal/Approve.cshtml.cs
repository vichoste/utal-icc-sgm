using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.GuideTeacher.Views.Proposal;

public class ApproveViewModel {
	[Display(Name = "ID"), Required]
	public string? Id { get; set; }
	[Display(Name = "Título de la propuesta"), Required]
	public string? Title { get; set; }
	[Display(Name = "Nombre del estudiante"), Required]
	public string? StudentName { get; set; }
}
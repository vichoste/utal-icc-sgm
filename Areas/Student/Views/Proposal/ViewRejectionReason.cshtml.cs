using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Student.Views.Proposal;

public class ViewRejectionReasonViewModel {
	[Display(Name = "ID"), Required]
	public string? Id { get; set; }
	[Display(Name = "Título de la propuesta"), Required]
	public string? Title { get; set; }
	[Display(Name = "Descripción de la propuesta"), Required]
	public string? Description { get; set; }
	[Display(Name = "Justificación"), Required]
	public string? Reason { get; set; }
}
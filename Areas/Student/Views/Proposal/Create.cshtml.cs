using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Student.Views.Proposal;

public class CreateViewModel {
	[Display(Name = "Título"), Required]
	public string? Title { get; set; }
	[Display(Name = "Descripción"), Required]
	public string? Description { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Student.Views.Proposal;

public class IndexViewModel {
	[Display(Name = "ID")]
	public string? Id { get; set; }
	[Display(Name = "Título")]
	public string? Title { get; set; }
	[Display(Name = "¿Es un borrador?")]
	public bool IsDraft { get; set; }
}
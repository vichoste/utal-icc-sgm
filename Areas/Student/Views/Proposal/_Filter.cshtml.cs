using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Student.Views.Proposal;

public class FilterPartialViewModel {
	[Display(Name = "Filtro")]
	public string? SearchString { get; set; }
}
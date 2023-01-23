using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Administrator.Views.Account;

public class FilterPartialViewModel {
	[Display(Name = "Filtro")]
	public string? SearchString { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Administrator.Views.Account;

public class DeleteViewModel {
	[Display(Name = "Filtro"), Required]
	public string? Id { get; set; }
	[DataType(DataType.EmailAddress), Display(Name = "E-mail"), EmailAddress]
	public string? Email { get; set; }
}
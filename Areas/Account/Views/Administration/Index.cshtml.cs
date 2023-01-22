using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Account.Views.Administration;

public class IndexViewModel {
	[Display(Name = "ID")]
	public string? Id { get; set; }
	[Display(Name = "Nombre")]
	public string? FirstName { get; set; }
	[Display(Name = "Apellido")]
	public string? LastName { get; set; }
	[Display(Name = "Número de matrícula")]
	public string? UniversityId { get; set; }
	[Display(Name = "RUT")]
	public string? Rut { get; set; }
	[Display(Name = "E-mail")]
	public string? Email { get; set; }
}
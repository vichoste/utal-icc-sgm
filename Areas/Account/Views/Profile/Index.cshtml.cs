using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Account.Views.Profile;

public class IndexViewModel {
	[Display(Name = "Nombre")]
	public string? FirstName { get; set; }
	[Display(Name = "Apellido")]
	public string? LastName { get; set; }
	[Display(Name = "RUT")]
	public string? Rut { get; set; }
	[Display(Name = "Número de matrícula")]
	public string? UniversityId { get; set; }
	[DataType(DataType.EmailAddress), Display(Name = "E-mail"), EmailAddress]
	public string? Email { get; set; }
	[Display(Name = "Creado")]
	public DateTimeOffset? CreatedAt { get; set; }
	[Display(Name = "Actualizado")]
	public DateTimeOffset? UpdatedAt { get; set; }
}
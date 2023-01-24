using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.DirectorTeacher.Views.Student;

public class EditViewModel {
	[Display(Name = "Nombre")]
	public string? FirstName { get; set; }
	[Display(Name = "Apellido")]
	public string? LastName { get; set; }
	[Display(Name = "Número de matrícula")]
	public string? UniversityId { get; set; }
	[Display(Name = "RUT")]
	public string? Rut { get; set; }
	[DataType(DataType.EmailAddress), Display(Name = "E-mail"), EmailAddress]
	public string? Email { get; set; }
}
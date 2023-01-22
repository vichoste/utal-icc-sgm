using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Account.Views.User;

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
	[Display(Name = "Administrador")]
	public bool IsAdministrator { get; set; }
	[Display(Name = "Profesor")]
	public bool IsTeacher { get; set; }
	[Display(Name = "Estudiante")]
	public bool IsStudent { get; set; }
}
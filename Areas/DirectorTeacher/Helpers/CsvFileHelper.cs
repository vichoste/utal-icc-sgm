using System.ComponentModel.DataAnnotations;

using CsvHelper.Configuration.Attributes;

using Utal.Icc.Sgm.ViewModels;

namespace Utal.Icc.Sgm.Areas.DirectorTeacher.Helpers;

public class CsvFileHelper : ApplicationUserViewModel {
	[Index(0), Display(Name = "Nombre")]
	public override string? FirstName { get; set; }
	[Index(1), Display(Name = "Apellido")]
	public override string? LastName { get; set; }
	[Index(2), Display(Name = "Número de matrícula")]
	public string? UniversityId { get; set; }
	[Index(3), Display(Name = "RUT")]
	public override string? Rut { get; set; }
	[Index(4), DataType(DataType.EmailAddress), Display(Name = "E-mail"), EmailAddress]
	public override string? Email { get; set; }
	[Index(5), DataType(DataType.Password), Display(Name = "Contraseña"), Required, StringLength(100, ErrorMessage = "La contraseña debe tener un mínimo de 6 carácteres", MinimumLength = 6)]
	public string? Password { get; set; }
}
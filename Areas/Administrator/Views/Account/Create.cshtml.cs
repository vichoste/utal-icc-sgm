using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Administrator.Views.Account;

public class Create {
	[Display(Name = "Nombre"), Required]
	public string? FirstName { get; set; }
	[Display(Name = "Apellido"), Required]
	public string? LastName { get; set; }
	[Display(Name = "RUT"), Required]
	public string? Rut { get; set; }
	[DataType(DataType.EmailAddress), Display(Name = "E-mail"), EmailAddress, Required]
	public string? Email { get; set; }
	[DataType(DataType.Password), Display(Name = "Contraseña"), Required, StringLength(100, ErrorMessage = "La contraseña debe tener un mínimo de 6 carácteres", MinimumLength = 6)]
	public string? Password { get; set; }
	[Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden"), DataType(DataType.Password), Display(Name = "Confirmar contraseña"), Required]
	public string? ConfirmPassword { get; set; }
	[Display(Name = "Administrador")]
	public bool IsAdministrator { get; set; }
	[Display(Name = "Profesor")]
	public bool IsTeacher { get; set; }
	[Display(Name = "Estudiante")]
	public bool IsStudent { get; set; }
}
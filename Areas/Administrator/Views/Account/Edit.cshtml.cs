using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Administrator.Views.Account;

public class EditViewModel {
	[Display(Name = "Nombre")]
	public string? FirstName { get; set; }
	[Display(Name = "Apellido")]
	public string? LastName { get; set; }
	[Display(Name = "RUT")]
	public string? Rut { get; set; }
	[DataType(DataType.EmailAddress), Display(Name = "E-mail"), EmailAddress]
	public string? Email { get; set; }
	[DataType(DataType.Password), Display(Name = "Contraseña actual"), StringLength(100, ErrorMessage = "La contraseña debe tener un mínimo de 6 carácteres", MinimumLength = 6)]
	public string? CurrentPassword { get; set; }
	[DataType(DataType.Password), Display(Name = "Nueva contraseña"), StringLength(100, ErrorMessage = "La contraseña debe tener un mínimo de 6 carácteres", MinimumLength = 6)]
	public string? NewPassword { get; set; }
	[Compare(nameof(NewPassword), ErrorMessage = "Las contraseñas no coinciden"), DataType(DataType.Password), Display(Name = "Confirmar nueva contraseña")]
	public string? ConfirmNewPassword { get; set; }
	[Display(Name = "Administrador")]
	public bool IsAdministrator { get; set; }
	[Display(Name = "Profesor")]
	public bool IsTeacher { get; set; }
	[Display(Name = "Estudiante")]
	public bool IsStudent { get; set; }
}
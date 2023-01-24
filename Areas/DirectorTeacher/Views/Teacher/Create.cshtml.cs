using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.DirectorTeacher.Views.Teacher;

public class CreateViewModel {
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
	[Display(Name = "Profesor guía")]
	public bool IsGuideTeacher { get; set; }
	[Display(Name = "Profesor co-guía")]
	public bool IsAssistantTeacher { get; set; }
	[Display(Name = "Profesor de curso")]
	public bool IsCourseTeacher { get; set; }
	[Display(Name = "Profesor de comité")]
	public bool IsCommitteeTeacher { get; set; }
}
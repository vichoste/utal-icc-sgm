using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.ViewModels;

public class ApplicationUserViewModel : ApplicationViewModel {
	#region Common
	[Display(Name = "Nombre")]
	public string? FirstName { get; set; }
	[Display(Name = "Apellido")]
	public string? LastName { get; set; }
	[Display(Name = "RUT")]
	public string? Rut { get; set; }
	[Display(Name = "Desactivado")]
	public bool IsDeactivated { get; set; }
	[DataType(DataType.EmailAddress), Display(Name = "E-mail"), EmailAddress]
	public string? Email { get; set; }
	[DataType(DataType.Password), Display(Name = "Contraseña"), StringLength(100, ErrorMessage = "La contraseña debe tener un mínimo de 6 carácteres", MinimumLength = 6)]
	public string? Password { get; set; }
	[Compare("Password", ErrorMessage = "Las contraseñas no coinciden"), DataType(DataType.Password), Display(Name = "Confirmar contraseña")]
	public string? ConfirmPassword { get; set; }
	#endregion
	#region Student
	[Display(Name = "Número de matrícula")]
	public string? UniversityId { get; set; }
	[Display(Name = "Cursos restantes")]
	public string? RemainingCourses { get; set; }
	[Display(Name = "¿Está realizando práctica?")]
	public bool IsDoingThePractice { get; set; }
	[Display(Name = "¿Está trabajando?")]
	public bool IsWorking { get; set; }
	#endregion
	#region Teacher
	[Display(Name = "Oficina")]
	public string? Office { get; set; }
	[Display(Name = "Horario")]
	public string? Schedule { get; set; }
	[Display(Name = "Especialización")]
	public string? Specialization { get; set; }
	[Display(Name = "Profesor guía")]
	public bool IsGuide { get; set; }
	[Display(Name = "Profesor co-guía")]
	public bool IsAssistant { get; set; }
	[Display(Name = "Profesor de curso")]
	public bool IsCourse { get; set; }
	[Display(Name = "Profesor de comité")]
	public bool IsCommittee { get; set; }
	[Display(Name = "Director de carrera")]
	public bool IsDirector { get; set; }
	#endregion
}
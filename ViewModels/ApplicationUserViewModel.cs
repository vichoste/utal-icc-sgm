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
	[DataType(DataType.EmailAddress), Display(Name = "E-mail"), EmailAddress]
	public string? Email { get; set; }
	[DataType(DataType.Password), Display(Name = "Contraseña"), StringLength(100, ErrorMessage = "La contraseña debe tener un mínimo de 6 carácteres", MinimumLength = 6)]
	public string? Password { get; set; }
	[Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden"), DataType(DataType.Password), Display(Name = "Confirmar contraseña")]
	public string? ConfirmPassword { get; set; }
	[Display(Name = "Desactivado")]
	public bool IsDeactivated { get; set; }
	#endregion
	#region Student
	[Display(Name = "Número de matrícula")]
	public string? StudentUniversityId { get; set; }
	[Display(Name = "Cursos restantes")]
	public string? StudentRemainingCourses { get; set; }
	[Display(Name = "¿Está realizando práctica?")]
	public bool StudentIsDoingThePractice { get; set; }
	[Display(Name = "¿Está trabajando?")]
	public bool StudentIsWorking { get; set; }
	#endregion
	#region Teacher
	[Display(Name = "Oficina")]
	public string? TeacherOffice { get; set; }
	[Display(Name = "Horario")]
	public string? TeacherSchedule { get; set; }
	[Display(Name = "Especialización")]
	public string? TeacherSpecialization { get; set; }
	[Display(Name = "Profesor guía")]
	public bool IsGuideTeacher { get; set; }
	[Display(Name = "Profesor co-guía")]
	public bool IsAssistantTeacher { get; set; }
	[Display(Name = "Profesor de curso")]
	public bool IsCourseTeacher { get; set; }
	[Display(Name = "Profesor de comité")]
	public bool IsCommitteeTeacher { get; set; }
	#endregion
}
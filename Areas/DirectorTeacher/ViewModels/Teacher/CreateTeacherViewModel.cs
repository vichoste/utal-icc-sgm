using System.ComponentModel.DataAnnotations;

using Utal.Icc.Sgm.ViewModels;

namespace Utal.Icc.Sgm.Areas.DirectorTeacher.ViewModels.Teacher;

public class CreateTeacherViewModel : ApplicationUserViewModel {
	[DataType(DataType.Password), Display(Name = "Contraseña"), StringLength(100, ErrorMessage = "La contraseña debe tener un mínimo de 6 carácteres", MinimumLength = 6)]
	public string? Password { get; set; }
	[Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden"), DataType(DataType.Password), Display(Name = "Confirmar contraseña")]
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
using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Student.Views.StudentProposal;

public class CreateViewModel {
	[Display(Name = "Título"), Required]
	public string? Title { get; set; }
	[Display(Name = "Descripción"), Required]
	public string? Description { get; set; }
	[Display(Name = "ID del profesor guía"), Required]
	public string? GuideTeacherId { get; set; }
	[Display(Name = "Profesor guía")]
	public string? GuideTeacherName { get; set; }
	[DataType(DataType.EmailAddress), Display(Name = "E-mail del profesor guía"), EmailAddress]
	public string? GuideTeacherEmail { get; set; }
	[Display(Name = "Oficina")]
	public string? GuideTeacherOffice { get; set; }
	[Display(Name = "Horarios")]
	public string? GuideTeacherSchedule { get; set; }
	[Display(Name = "Especialización")]
	public string? GuideTeacherSpecialization { get; set; }
	[Display(Name = "Profesores co-guía")]
	public ICollection<string>? AssistantTeachers { get; set; } = new HashSet<string>();
}
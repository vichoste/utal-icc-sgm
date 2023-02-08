using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Student.Views.StudentProposal;

public class ConvertViewModel {
	[Display(Name = "ID"), Required]
	public string? Id { get; set; }
	[Display(Name = "Título")]
	public string? Title { get; set; }
	[Display(Name = "Descripción")]
	public string? Description { get; set; }
	[Display(Name = "Profesor guía")]
	public string? GuideTeacherName { get; set; }
	[DataType(DataType.EmailAddress), Display(Name = "E-mail"), EmailAddress]
	public string? GuideTeacherEmail { get; set; }
	[Display(Name = "Oficina")]
	public string? GuideTeacherOffice { get; set; }
	[Display(Name = "Horarios")]
	public string? GuideTeacherSchedule { get; set; }
	[Display(Name = "Especialización")]
	public string? GuideTeacherSpecialization { get; set; }
	[Display(Name = "Profesores co-guía")]
	public ICollection<string>? AssistantTeachers { get; set; } = new HashSet<string>();
	[Display(Name = "Creado")]
	public DateTimeOffset? CreatedAt { get; set; }
	[Display(Name = "Actualizado")]
	public DateTimeOffset? UpdatedAt { get; set; }
}
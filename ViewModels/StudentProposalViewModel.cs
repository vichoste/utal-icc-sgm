using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.ViewModels;

public class StudentProposalViewModel : ProposalViewModel {
	[Display(Name = "ID del estudiante")]
	public string? StudentId { get; set; }
	[Display(Name = "Nombre del estudiante")]
	public string? StudentName { get; set; }
	[DataType(DataType.EmailAddress), Display(Name = "E-mail del estudiante"), EmailAddress]
	public string? StudentEmail { get; set; }
	[Display(Name = "Número de matrícula")]
	public string? StudentUniversityId { get; set; }
	[Display(Name = "Cursos restantes")]
	public string? StudentRemainingCourses { get; set; }
	[Display(Name = "¿Está realizando práctica?")]
	public bool StudentIsDoingThePractice { get; set; }
	[Display(Name = "¿Está trabajando?")]
	public bool StudentIsWorking { get; set; }
	[Display(Name = "ID del profesor guía")]
	public string? GuideTeacherId { get; set; }
	[Display(Name = "Nombre del profesor guía")]
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
	[Display(Name = "¿Quién rechazó la propuesta?")]
	public string? WhoRejected { get; set; }
	[Display(Name = "Justificación")]
	public string? RejectionReason { get; set; }
}
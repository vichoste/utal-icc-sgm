using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Student.Views.GuideTeacherProposal;

public class ConvertViewModel {
	[Display(Name = "ID"), Required]
	public string? Id { get; set; }
	[Display(Name = "Título"), Required]
	public string? Title { get; set; }
	[Display(Name = "Descripción"), Required]
	public string? Description { get; set; }
	[Display(Name = "Nombre del estudiante"), Required]
	public string? StudentName { get; set; }
	[DataType(DataType.EmailAddress), Display(Name = "E-mail del estudiante"), EmailAddress]
	public string? StudentEmail { get; set; }
	[Display(Name = "Número de matrícula"), Required]
	public string? StudentUniversityId { get; set; }
	[Display(Name = "Cursos restantes del estudiante")]
	public string? StudentRemainingCourses { get; set; }
	[Display(Name = "¿El estudiante está haciendo la práctica?")]
	public bool StudentIsDoingThePractice { get; set; }
	[Display(Name = "¿El estudiante está trabajando?")]
	public bool StudentIsWorking { get; set; }
	[Display(Name = "Primer profesor co-guía"), Required]
	public string? AssistantTeacher1 { get; set; }
	[Display(Name = "Segundo profesor co-guía"), Required]
	public string? AssistantTeacher2 { get; set; }
	[Display(Name = "Tercer profesor co-guía"), Required]
	public string? AssistantTeacher3 { get; set; }
	[Display(Name = "Creado"), Required]
	public DateTimeOffset? CreatedAt { get; set; }
	[Display(Name = "Actualizado"), Required]
	public DateTimeOffset? UpdatedAt { get; set; }
}
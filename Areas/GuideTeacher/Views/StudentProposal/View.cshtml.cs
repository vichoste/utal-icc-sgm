using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.GuideTeacher.Views.Proposal;

public class ViewModel {
	[Display(Name = "ID"), Required]
	public string? Id { get; set; }
	[Display(Name = "Título")]
	public string? Title { get; set; }
	[Display(Name = "Descripción")]
	public string? Description { get; set; }
	[Display(Name = "Nombre del estudiante"), Required]
	public string? StudentName { get; set; }
	[DataType(DataType.EmailAddress), Display(Name = "E-mail del estudiante"), EmailAddress]
	public string? StudentEmail { get; set; }
	[Display(Name = "Cursos restantes del estudiante")]
	public string? StudentRemainingCourses { get; set; }
	[Display(Name = "¿El estudiante está haciendo la práctica?")]
	public bool StudentIsDoingThePractice { get; set; }
	[Display(Name = "¿El estudiante está trabajando?")]
	public bool StudentIsWorking { get; set; }
	[Display(Name = "Primer profesor co-guía")]
	public string? AssistantTeacher1 { get; set; }
	[Display(Name = "Segundo profesor co-guía")]
	public string? AssistantTeacher2 { get; set; }
	[Display(Name = "Tercer profesor co-guía")]
	public string? AssistantTeacher3 { get; set; }
	[Display(Name = "Creado"), Required]
	public DateTimeOffset? CreatedAt { get; set; }
	[Display(Name = "Actualizado"), Required]
	public DateTimeOffset? UpdatedAt { get; set; }
}
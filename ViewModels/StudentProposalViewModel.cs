using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.ViewModels;

public partial class ProposalViewModel {
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
	[Display(Name = "Justificación")]
	public string? RejectionReason { get; set; }
}
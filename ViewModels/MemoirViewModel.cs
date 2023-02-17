using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.ViewModels;

public class MemoirViewModel : ApplicationViewModel {
	#region Common
	[Display(Name = "Título")]
	public string? Title { get; set; }
	[Display(Name = "Descripción")]
	public string? Description { get; set; }
	[Display(Name = "Estado")]
	public string? Phase { get; set; }
	#endregion
	#region Student
	[Display(Name = "ID")]
	public string? MemoristId { get; set; }
	[Display(Name = "Memorista")]
	public string? MemoristName { get; set; }
	[Display(Name = "Número de matrícula")]
	public string? UniversityId { get; set; }
	[Display(Name = "Cursos restantes")]
	public string? RemainingCourses { get; set; }
	[Display(Name = "¿Está realizando práctica?")]
	public bool IsDoingThePractice { get; set; }
	[Display(Name = "¿Está trabajando?")]
	public bool IsWorking { get; set; }
	#endregion
	#region Guide
	[Display(Name = "ID")]
	public string? GuideId { get; set; }
	[Display(Name = "Profesor guía")]
	public string? GuideName { get; set; }
	[Display(Name = "Oficina")]
	public string? Office { get; set; }
	[Display(Name = "Horario")]
	public string? Schedule { get; set; }
	[Display(Name = "Especialización")]
	public string? Specialization { get; set; }
	#endregion
	#region Assistants
	[Display(Name = "Profesores co-guía")]
	public IEnumerable<string?>? AssistantTeachers { get; set; } = new HashSet<string?>();
	#endregion
	#region Rejection
	[Display(Name = "¿Quién rechazó la propuesta?")]
	public string? WhoRejected { get; set; }
	[Display(Name = "Justificación")]
	public string? Reason { get; set; }
	#endregion
}
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
	[Display(Name = "Memorista")]
	public string? Memorist { get; set; }
	[Display(Name = "Número de matrícula")]
	public string? UniversityId { get; set; }
	[Display(Name = "Cursos restantes")]
	public string? RemainingCourses { get; set; }
	[Display(Name = "¿Está realizando práctica?")]
	public bool IsDoingThePractice { get; set; }
	[Display(Name = "¿Está trabajando?")]
	public bool IsWorking { get; set; }
	#endregion
	[Display(Name = "¿Quién rechazó la propuesta?")]
	public string? WhoRejected { get; set; }
	[Display(Name = "Justificación")]
	public string? Reason { get; set; }
}
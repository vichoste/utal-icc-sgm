using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.ViewModels;

public class MemoirViewModel : ApplicationUserViewModel {
	#region Common
	[Display(Name = "Título")]
	public string? Title { get; set; }
	[Display(Name = "Descripción")]
	public string? Description { get; set; }
	[Display(Name = "Estado")]
	public string? Phase { get; set; }
	#endregion
	#region ApplicationUsers
	[Display(Name = "Candidatos")]
	public IEnumerable<string?>? Candidates { get; set; } = new HashSet<string?>();
	[Display(Name = "Estudiantes")]
	public IEnumerable<string?>? Students { get; set; } = new HashSet<string?>();
	[Display(Name = "Profesores co-guía")]
	public IEnumerable<string?>? AssistantTeachers { get; set; } = new HashSet<string?>();
	[Display(Name = "Profesores guía")]
	public IEnumerable<string?>? GuideTeachers { get; set; } = new HashSet<string?>();
	#endregion
	[Display(Name = "¿Quién rechazó la propuesta?")]
	public string? WhoRejected { get; set; }
	[Display(Name = "Justificación")]
	public string? Reason { get; set; }
}
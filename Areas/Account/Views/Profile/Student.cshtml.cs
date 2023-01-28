using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Account.Views.Profile;

public class StudentViewModel {
	[Display(Name = "ID"), Required]
	public Guid? Id { get; set; }
	[Display(Name = "Número de matrícula")]
	public string? UniversityId { get; set; }
	[Display(Name = "Cursos restantes")]
	public string? RemainingCourses { get; set; }
	[Display(Name = "¿Está haciendo la práctica?")]
	public bool IsDoingThePractice { get; set; }
	[Display(Name = "¿Está trabajando?")]
	public bool IsWorking { get; set; }
}
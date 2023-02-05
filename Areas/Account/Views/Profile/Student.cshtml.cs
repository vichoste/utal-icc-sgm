using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Account.Views.Profile;

public class StudentViewModel {
	[Display(Name = "Número de matrícula"), Required]
	public string? StudentUniversityId { get; set; }
	[Display(Name = "Cursos restantes")]
	public string? StudentRemainingCourses { get; set; }
	[Display(Name = "¿Estás haciendo la práctica?")]
	public bool StudentIsDoingThePractice { get; set; }
	[Display(Name = "¿Estás trabajando?")]
	public bool StudentIsWorking { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.ViewModels;

public partial class ApplicationUserViewModel {
	[Display(Name = "Número de matrícula")]
	public string? StudentUniversityId { get; set; }
	[Display(Name = "Cursos restantes")]
	public string? StudentRemainingCourses { get; set; }
	[Display(Name = "¿Está realizando práctica?")]
	public bool? StudentIsDoingThePractice { get; set; }
	[Display(Name = "¿Está trabajando?")]
	public bool? StudentIsWorking { get; set; }
}
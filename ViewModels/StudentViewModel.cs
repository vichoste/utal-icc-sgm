using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.ViewModels;

public partial class ApplicationUserViewModel {
	[Display(Name = "Número de matrícula")]
	public virtual string? StudentUniversityId { get; set; }
	[Display(Name = "Cursos restantes")]
	public virtual string? StudentRemainingCourses { get; set; }
	[Display(Name = "¿Está realizando práctica?")]
	public virtual bool StudentIsDoingThePractice { get; set; }
	[Display(Name = "¿Está trabajando?")]
	public virtual bool StudentIsWorking { get; set; }
}
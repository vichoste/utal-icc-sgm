using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.ViewModels;

public partial class ApplicationUserViewModel {
	[Display(Name = "Número de matrícula"), Required]
	public virtual string? StudentUniversityId { get; set; }
	[Display(Name = "Cursos restantes"), Required]
	public virtual string? StudentRemainingCourses { get; set; }
	[Display(Name = "¿Está realizando práctica?"), Required]
	public virtual bool StudentIsDoingThePractice { get; set; }
	[Display(Name = "¿Está trabajando?"), Required]
	public virtual bool StudentIsWorking { get; set; }
}
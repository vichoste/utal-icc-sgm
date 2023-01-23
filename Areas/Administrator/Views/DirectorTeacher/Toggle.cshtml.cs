using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Administrator.Views.DirectorTeacher;

public class ToggleViewModel {
	public string? Id { get; set; }
	[DataType(DataType.EmailAddress), Display(Name = "E-mail"), EmailAddress]
	public string? Email { get; set; }
	[Display(Name = "¿Es director de carrera?")]
	public bool IsDirectorTeacher { get; set; }
}
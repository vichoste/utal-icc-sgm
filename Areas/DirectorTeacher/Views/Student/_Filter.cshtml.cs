using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.DirectorTeacher.Views.Student;

public class FilterPartialViewModel {
	[Display(Name = "Filtro")]
	public string? SearchString { get; set; }
}
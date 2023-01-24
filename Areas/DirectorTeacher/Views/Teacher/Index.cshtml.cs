using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.DirectorTeacher.Views.Teacher;

public class IndexViewModel {
	[Display(Name = "ID")]
	public string? Id { get; set; }
	[Display(Name = "Nombre")]
	public string? FirstName { get; set; }
	[Display(Name = "Apellido")]
	public string? LastName { get; set; }
	[Display(Name = "RUT")]
	public string? Rut { get; set; }
	[Display(Name = "E-mail")]
	public string? Email { get; set; }
}
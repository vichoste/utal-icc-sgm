using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.DirectorTeacher.Views.Teacher;

public class DeleteViewModel {
	[Display(Name = "ID"), Required]
	public string? Id { get; set; }
	[DataType(DataType.EmailAddress), Display(Name = "E-mail"), EmailAddress]
	public string? Email { get; set; }
}
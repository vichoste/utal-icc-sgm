using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.DirectorTeacher.Views.Teacher;

public class TransferViewModel {
	[Display(Name = "ID"), Required]
	public string? Id { get; set; }
	[DataType(DataType.EmailAddress), Display(Name = "E-mail"), EmailAddress, Required]
	public string? Email { get; set; }
}
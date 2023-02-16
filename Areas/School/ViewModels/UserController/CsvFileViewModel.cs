using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.School.ViewModels.UserController;

public class CsvFileViewModel {
	[Display(Name = "Archivo CSV")]
	public IFormFile? CsvFile { get; set; }
}
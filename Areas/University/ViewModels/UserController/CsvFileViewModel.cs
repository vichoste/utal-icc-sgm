using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.University.ViewModels.User;

public class CsvFileViewModel {
	[Display(Name = "Archivo CSV")]
	public IFormFile? CsvFile { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.DirectorTeacher.ViewModels;

public class CsvFileViewModel {
	[Display(Name = "Archivo CSV")]
	public IFormFile? CsvFile { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.DirectorTeacher.ViewModels.Student;

public class CsvFileViewModel {
	[Display(Name = "Archivo CSV")]
	public IFormFile? CsvFile { get; set; }
}
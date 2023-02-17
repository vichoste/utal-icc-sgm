using CsvHelper.Configuration.Attributes;

namespace Utal.Icc.Sgm.Areas.University.Helpers;

public class CsvFileHelper {
	[Index(0)]
	public string? FirstName { get; set; }
	[Index(1)]
	public string? LastName { get; set; }
	[Index(2)]
	public string? UniversityId { get; set; }
	[Index(3)]
	public string? Rut { get; set; }
	[Index(4)]
	public string? Email { get; set; }
	[Index(5)]
	public string? Password { get; set; }
}
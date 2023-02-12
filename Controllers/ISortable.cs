namespace Utal.Icc.Sgm.Controllers;

public interface ISortable {
	string[]? Parameters { get; set; }
	
	void SetSortParameters(string sortOrder, params string[] parameters);
}
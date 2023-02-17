using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.ViewModels.Shared;

public class FilterPartialViewModel {
	[Display(Name = "Acción")]
	public string? Action { get; set; }
	[Display(Name = "Controlador")]
	public string? Controller { get; set; }
	[Display(Name = "Área")]
	public string? Area { get; set; }
	[Display(Name = "Filtro")]
	public string? SearchString { get; set; }

	public FilterPartialViewModel(string action, string controller, string area) {
		this.Action = action;
		this.Controller = controller;
		this.Area = area;
	}
}
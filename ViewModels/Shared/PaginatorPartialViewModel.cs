using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.ViewModels.Shared;

public class PaginatorPartialViewModel {
	[Display(Name = "Acción")]
	public string? Action { get; set; }
	[Display(Name = "Índice de página")]
	public int? PageIndex { get; set; }
	[Display(Name = "Total de páginas")]
	public int? TotalPages { get; set; }
	[Display(Name = "Página anterior")]
	public bool HasPreviousPage { get; set; }
	[Display(Name = "Página siguiente")]
	public bool HasNextPage { get; set; }

	public PaginatorPartialViewModel(string action, int pageIndex, int totalPages, bool hasPreviousPage, bool hasNextPage) {
		this.Action = action;
		this.PageIndex = pageIndex;
		this.TotalPages = totalPages;
		this.HasPreviousPage = hasPreviousPage;
		this.HasNextPage = hasNextPage;
	}
}
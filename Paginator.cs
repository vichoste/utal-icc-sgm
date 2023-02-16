using Microsoft.IdentityModel.Tokens;

using Utal.Icc.Sgm.ViewModels;

namespace Utal.Icc.Sgm;

public class Paginator<T> : List<T> where T : ApplicationViewModel {
	public int PageIndex { get; private set; }
	public int TotalPages { get; private set; }
	public bool HasPreviousPage => this.PageIndex > 1;
	public bool HasNextPage => this.PageIndex < this.TotalPages;

	public Paginator(List<T> items, int count, int pageIndex, int pageSize) {
		this.PageIndex = pageIndex;
		this.TotalPages = (int)Math.Ceiling(count / (double)pageSize);
		this.AddRange(items);
	}

	public static Paginator<T> Create(IQueryable<T> source, int pageIndex, int pageSize) {
		var count = source.Count();
		var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
		return new Paginator<T>(items, count, pageIndex, pageSize);
	}

	private IEnumerable<T> Filter(string searchString, IEnumerable<T> viewModels, params string[] parameters) {
		var result = new List<T>();
		foreach (var parameter in parameters) {
			var partials = viewModels
				.Where(vm => !(vm.GetType().GetProperty(parameter)!.GetValue(vm, null) as string)!.IsNullOrEmpty()
					&& (vm.GetType().GetProperty(parameter)!.GetValue(vm, null) as string)!.Contains(searchString));
			foreach (var partial in partials) {
				if (!result.Any(vm => vm.Id == partial.Id)) {
					result.Add(partial);
				}
			}
		}
		return result.AsEnumerable();
	}

	private IEnumerable<T> Sort(string sortOrder, IEnumerable<T> viewModels, params string[] parameters) {
		foreach (var parameter in parameters) {
			if (parameter == sortOrder) {
				return viewModels.OrderBy(vm => vm.GetType().GetProperty(parameter)!.GetValue(vm, null));
			} else if ($"{parameter}Desc" == sortOrder) {
				return viewModels.OrderByDescending(vm => vm.GetType().GetProperty(parameter)!.GetValue(vm, null));
			}
		}
		return viewModels.OrderBy(vm => vm.GetType().GetProperty(parameters[0])).AsEnumerable();
	}

	protected async Task<IQueryable<T>> GetPaginatedViewModelsAsync(string sortOrder, string currentFilter, string searchString, int? pageNumber, string[] parameters, Func<Task<IEnumerable<T>>> getViewModels) {
		var users = await getViewModels();
		var ordered = this.Sort(sortOrder, users, parameters);
		var output = !searchString.IsNullOrEmpty() ? this.Filter(searchString, ordered, parameters) : ordered;
		return output.AsQueryable();
	}

	protected IQueryable<T> GetPaginatedViewModels(string sortOrder, string currentFilter, string searchString, int? pageNumber, string[] parameters, Func<IEnumerable<T>> getViewModels) {
		var users = getViewModels();
		var ordered = this.Sort(sortOrder, users, parameters);
		var output = !searchString.IsNullOrEmpty() ? this.Filter(searchString, ordered, parameters) : ordered;
		return output.AsQueryable();
	}
}
using Microsoft.IdentityModel.Tokens;

using Utal.Icc.Sgm.ViewModels;

namespace Utal.Icc.Sgm;

public class Paginator<T> : List<T> where T : ApplicationViewModel {
	public int PageIndex { get; protected set; }
	public int TotalPages { get; protected set; }
	public bool HasPreviousPage => this.PageIndex > 1;
	public bool HasNextPage => this.PageIndex < this.TotalPages;
	public string[]? Parameters { get; private set; }

	protected Paginator(List<T> items, int count, int pageIndex, int pageSize, params string[] parameters) {
		this.Parameters = parameters;
		this.PageIndex = pageIndex;
		this.TotalPages = (int)Math.Ceiling(count / (double)pageSize);
		this.AddRange(items);
	}

	public static Paginator<T> Create(IQueryable<T> source, int pageIndex, int pageSize, params string[] parameters) {
		var count = source.Count();
		var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
		return new Paginator<T>(items, count, pageIndex, pageSize, parameters);
	}

	public void Filter(string searchString) {
		var result = new List<T>();
		foreach (var parameter in this.Parameters!) {
			var partials = this
				.Where(vm => !(vm.GetType().GetProperty(parameter)!.GetValue(vm, null) as string)!.IsNullOrEmpty()
					&& (vm.GetType().GetProperty(parameter)!.GetValue(vm, null) as string)!.Contains(searchString));
			foreach (var partial in partials) {
				if (!result.Any(vm => vm.Id == partial.Id)) {
					result.Add(partial);
				}
			}
		}
		this.Clear();
		this.AddRange(result.AsEnumerable());
	}

	public void Sort(string sortOrder) {
		foreach (var parameter in this.Parameters!) {
			if (parameter == sortOrder) {
				this.Clear();
				this.AddRange(this.OrderBy(vm => vm.GetType().GetProperty(parameter)!.GetValue(vm, null)).AsEnumerable());
				return;
			} else if ($"{parameter}Desc" == sortOrder) {
				this.Clear();
				this.AddRange(this.OrderByDescending(vm => vm.GetType().GetProperty(parameter)!.GetValue(vm, null)).AsEnumerable());
				return;
			}
		}
		this.Clear();
		this.AddRange(this.OrderBy(vm => vm.GetType().GetProperty(this.Parameters[0])!.GetValue(vm, null)).AsEnumerable());
	}
}
namespace Utal.Icc.Sgm;

public class PaginatedList<T> : List<T> {
	public string ApplicationUserId { get; private set; }
	public int PageIndex { get; private set; }
	public int TotalPages { get; private set; }
	public bool HasPreviousPage => this.PageIndex > 1;
	public bool HasNextPage => this.PageIndex < this.TotalPages;

	public PaginatedList(string applicationUserId, List<T> items, int count, int pageIndex, int pageSize) {
		this.ApplicationUserId = applicationUserId;
		this.PageIndex = pageIndex;
		this.TotalPages = (int)Math.Ceiling(count / (double)pageSize);
		this.AddRange(items);
	}

	public static PaginatedList<T> Create(string applicationUserId, IQueryable<T> source, int pageIndex, int pageSize) {
		var count = source.Count();
		var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
		return new PaginatedList<T>(applicationUserId, items, count, pageIndex, pageSize);
	}
}
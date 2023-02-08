using Utal.Icc.Sgm.ViewModels;

namespace Utal.Icc.Sgm.Controllers;

public interface IApplicationUserViewModelSortable {
	IOrderedEnumerable<T> Sort<T>(string searchString, IEnumerable<T> applicationUserViewModels, params string[] parameters) where T : ApplicationUserViewModel;
}
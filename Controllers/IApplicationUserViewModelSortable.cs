using Utal.Icc.Sgm.ViewModels;

namespace Utal.Icc.Sgm.Controllers;

public interface IApplicationUserViewModelSortable {
	IOrderedEnumerable<ApplicationUserViewModel> Sort(string searchString, IEnumerable<ApplicationUserViewModel> viewModels, params string[] parameters);
}
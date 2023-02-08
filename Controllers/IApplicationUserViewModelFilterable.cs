using Utal.Icc.Sgm.ViewModels;

namespace Utal.Icc.Sgm.Controllers;

public interface IApplicationUserViewModelFilterable {
	IEnumerable<ApplicationUserViewModel> Filter(string searchString, IOrderedEnumerable<ApplicationUserViewModel> viewModels, params string[] parameters);
}
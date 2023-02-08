using Utal.Icc.Sgm.ViewModels;

namespace Utal.Icc.Sgm.Controllers;

public interface IApplicationUserViewModelFilterable {
	IEnumerable<T> Filter<T>(string searchString, IOrderedEnumerable<T> applicationUserViewModels, params string[] parameters) where T : ApplicationUserViewModel;
}
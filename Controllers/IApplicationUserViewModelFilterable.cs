using Utal.Icc.Sgm.ViewModels;

namespace Utal.Icc.Sgm.Controllers;

public interface IApplicationUserViewModelFilterable {
	IEnumerable<T> Filter<T>(string searchString, IOrderedEnumerable<T> viewModels, params string[] parameters) where T : ApplicationUserViewModel;
}
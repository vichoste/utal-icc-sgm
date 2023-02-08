using Utal.Icc.Sgm.ViewModels;

namespace Utal.Icc.Sgm.Controllers;

public interface IProposalViewModelSortable {
	IOrderedEnumerable<ProposalViewModel> Sort(string sortOrder, IEnumerable<ProposalViewModel> viewModels, params string[] parameters);
}
using Utal.Icc.Sgm.ViewModels;

namespace Utal.Icc.Sgm.Controllers;

public interface IProposalViewModelFilterable {
	IEnumerable<ProposalViewModel> Filter(string searchString, IOrderedEnumerable<ProposalViewModel> viewModels, params string[] parameters);
}
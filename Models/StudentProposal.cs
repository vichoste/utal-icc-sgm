namespace Utal.Icc.Sgm.Models;

public partial class Proposal {
	public Status? ProposalStatus { get; set; }
	public virtual ApplicationUser? StudentOfTheProposal { get; set; }
	public virtual ApplicationUser? WhoRejectedThisProposal { get; set; }
	public string? RejectionReason { get; set; }
}
namespace Utal.Icc.Sgm.Models;

public partial class Proposal {
	public enum Status {
		Draft,
		Sent,
		Approved,
		Rejected,
		Published,
		Ready
	}
	public Status? ProposalStatus { get; set; }
	public virtual ApplicationUser? StudentOwnerOfTheProposal { get; set; }
	public virtual ApplicationUser? GuideTeacherOfTheProposal { get; set; }
	public virtual ApplicationUser? WhoRejectedThisProposal { get; set; }
	public string? RejectionReason { get; set; }
}
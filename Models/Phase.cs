namespace Utal.Icc.Sgm.Models;

public enum Phase {
	DraftByStudent,
	DraftByGuide,
	SentToGuide,
	PublishedByGuide,
	RejectedByGuide,
	ApprovedByGuide,
	ReadyByGuide,
	SentToCommittee,
	RejectedByCommittee,
	ApprovedByCommittee,
	RejectedByDirector,
	ApprovedByDirector,
	InProgress,
	Abandoned,
	Completed
}
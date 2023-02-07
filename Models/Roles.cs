namespace Utal.Icc.Sgm.Models;

public partial class ApplicationUser {
	public enum Roles {
		DirectorTeacher, // RANK ROLE
		CommitteeTeacher, // RANK ROLE
		CourseTeacher, // RANK ROLE
		GuideTeacher, // RANK ROLE
		AssistantTeacher, // RANK ROLE
		Teacher, // MAIN ROLE
		EngineerStudent, // RANK ROLE
		CompletedStudent, // RANK ROLE
		ThesisStudent, // RANK ROLE
		Student // MAIN ROLE
	}
}
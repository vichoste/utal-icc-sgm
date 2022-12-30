namespace Utal.Icc.Sgm.Models;

public class StudentProfile {
	public string? RemainingCourses { get; set; }
	public bool IsDoingThePractice { get; set; }
	public bool IsWorking { get; set; }
	public virtual ApplicationUser? ApplicationUser { get; set; }
}
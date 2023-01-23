using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Utal.Icc.Sgm.Models;

public class StudentProfile {
	[Key]
	public Guid Id { get; set; }
	public string? UniversityId { get; set; }
	public string? RemainingCourses { get; set; }
	public bool IsDoingThePractice { get; set; }
	public bool IsWorking { get; set; }
	[ForeignKey(nameof(Models.ApplicationUser.StudentProfile))]
	public virtual ApplicationUser? ApplicationUser { get; set; }
}
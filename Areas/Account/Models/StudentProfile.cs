using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Utal.Icc.Sgm.Areas.Account.Models;

public class StudentProfile {
	[Key]
	public Guid Id { get; set; }
	public string? RemainingCourses { get; set; }
	public bool IsDoingThePractice { get; set; }
	public bool IsWorking { get; set; }
	[ForeignKey(nameof(Models.ApplicationUser.StudentProfile))]
	public virtual ApplicationUser? ApplicationUser { get; set; }
}
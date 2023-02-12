using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Controllers;

public interface IPopulatable {
	Task PopulateAssistantTeachers(ApplicationUser guideTeacher);
}
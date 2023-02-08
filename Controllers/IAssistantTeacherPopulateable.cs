using Utal.Icc.Sgm.Models;

namespace Utal.Icc.Sgm.Controllers;

public interface IAssistantTeacherPopulateable {
	Task Populate(ApplicationUser guideTeacher);
}
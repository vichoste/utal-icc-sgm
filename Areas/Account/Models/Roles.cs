namespace Utal.Icc.Sgm.Areas.Account.Models;

public enum Roles {
	Administrator,
	DirectorTeacher,
	CommitteeTeacher,
	CourseTeacher,
	MainTeacher,
	AssistantTeacher,
	EngineerStudent,
	FinishedStudent,
	ThesisStudent,
	RegularStudent
}

public class SpanishRoles {
	public static string? TranslateRoleStringToSpanish(string? role) {
		return role switch {
			"Administrator" => "Administrador",
			"DirectorTeacher" => "Profesor director de escuela",
			"CommitteeTeacher" => "Profesor de comité",
			"CourseTeacher" => "Profesor de curso de titulación",
			"MainTeacher" => "Profesor guía",
			"AssistantTeacher" => "Profesor co-guía",
			"EngineerStudent" => "Estudiante titulado",
			"FinishedStudent" => "Estudiante graduado",
			"ThesisStudent" => "Estudiante memorista",
			"RegularStudent" => "Estudiante regular",
			_ => "Rol Desconocido"
		};
	}
}
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

public class RoleTextUtilities {
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
	public static string? TranslateRoleStringToEnglish(string? role) {
		return role switch {
			"Administrador" => "Administrator",
			"Profesor director de escuela" => "DirectorTeacher",
			"Profesor de comité" => "CommitteeTeacher",
			"Profesor de curso de titulación" => "CourseTeacher",
			"Profesor guía" => "MainTeacher",
			"Profesor co-guía" => "AssistantTeacher",
			"Estudiante titulado" => "EngineerStudent",
			"Estudiante graduado" => "FinishedStudent",
			"Estudiante memorista" => "ThesisStudent",
			"Estudiante regular" => "RegularStudent",
			_ => "Unknown role"
		};
	}
}
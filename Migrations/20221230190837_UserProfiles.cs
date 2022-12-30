using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Utal.Icc.Sgm.Migrations;

/// <inheritdoc />
public partial class UserProfiles : Migration {
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder) {
		_ = migrationBuilder.CreateTable(
			name: "StudentProfile",
			columns: table => new {
				Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
				RemainingCourses = table.Column<string>(type: "nvarchar(max)", nullable: true),
				IsDoingThePractice = table.Column<bool>(type: "bit", nullable: false),
				IsWorking = table.Column<bool>(type: "bit", nullable: false),
				StudentProfile = table.Column<string>(type: "nvarchar(450)", nullable: true)
			},
			constraints: table => {
				_ = table.PrimaryKey("PK_StudentProfile", x => x.Id);
				_ = table.ForeignKey(
					name: "FK_StudentProfile_AspNetUsers_StudentProfile",
					column: x => x.StudentProfile,
					principalTable: "AspNetUsers",
					principalColumn: "Id");
			});

		_ = migrationBuilder.CreateTable(
			name: "TeacherProfile",
			columns: table => new {
				Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
				Office = table.Column<string>(type: "nvarchar(max)", nullable: true),
				Schedule = table.Column<string>(type: "nvarchar(max)", nullable: true),
				Specialization = table.Column<string>(type: "nvarchar(max)", nullable: true),
				TeacherProfile = table.Column<string>(type: "nvarchar(450)", nullable: true)
			},
			constraints: table => {
				_ = table.PrimaryKey("PK_TeacherProfile", x => x.Id);
				_ = table.ForeignKey(
					name: "FK_TeacherProfile_AspNetUsers_TeacherProfile",
					column: x => x.TeacherProfile,
					principalTable: "AspNetUsers",
					principalColumn: "Id");
			});

		_ = migrationBuilder.CreateIndex(
			name: "IX_StudentProfile_StudentProfile",
			table: "StudentProfile",
			column: "StudentProfile",
			unique: true,
			filter: "[StudentProfile] IS NOT NULL");

		_ = migrationBuilder.CreateIndex(
			name: "IX_TeacherProfile_TeacherProfile",
			table: "TeacherProfile",
			column: "TeacherProfile",
			unique: true,
			filter: "[TeacherProfile] IS NOT NULL");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder) {
		_ = migrationBuilder.DropTable(
			name: "StudentProfile");

		_ = migrationBuilder.DropTable(
			name: "TeacherProfile");
	}
}

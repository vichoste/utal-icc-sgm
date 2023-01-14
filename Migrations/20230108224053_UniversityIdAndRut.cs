using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Utal.Icc.Sgm.Migrations;

/// <inheritdoc />
public partial class UniversityIdAndRut : Migration {
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder) {
		_ = migrationBuilder.AddColumn<string>(
			name: "Rut",
			table: "AspNetUsers",
			type: "nvarchar(max)",
			nullable: true);

		_ = migrationBuilder.AddColumn<string>(
			name: "UniversityId",
			table: "AspNetUsers",
			type: "nvarchar(max)",
			nullable: true);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder) {
		_ = migrationBuilder.DropColumn(
			name: "Rut",
			table: "AspNetUsers");

		_ = migrationBuilder.DropColumn(
			name: "UniversityId",
			table: "AspNetUsers");
	}
}
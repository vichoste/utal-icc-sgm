using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Utal.Icc.Sgm.Migrations;

/// <inheritdoc />
public partial class RemoveUserChangeLimitORM : Migration {
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder) {
		_ = migrationBuilder.DropColumn(
			name: "UsernameChangeLimit",
			table: "AspNetUsers");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder) {
		_ = migrationBuilder.AddColumn<int>(
			name: "UsernameChangeLimit",
			table: "AspNetUsers",
			type: "int",
			nullable: false,
			defaultValue: 0);
	}
}
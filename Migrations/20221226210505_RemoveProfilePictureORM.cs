﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Utal.Icc.Sgm.Migrations;

/// <inheritdoc />
public partial class RemoveProfilePictureORM : Migration {
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder) {
		_ = migrationBuilder.DropColumn(
			name: "ProfilePicture",
			table: "AspNetUsers");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder) {
		_ = migrationBuilder.AddColumn<byte[]>(
			name: "ProfilePicture",
			table: "AspNetUsers",
			type: "varbinary(max)",
			nullable: true);
	}
}
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Utal.Icc.Sgm.Migrations;

/// <inheritdoc />
public partial class CustomProperties : Migration {
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder) {
		_ = migrationBuilder.AlterColumn<string>(
			name: "Name",
			table: "AspNetUserTokens",
			type: "nvarchar(450)",
			nullable: false,
			oldClrType: typeof(string),
			oldType: "nvarchar(128)",
			oldMaxLength: 128);

		_ = migrationBuilder.AlterColumn<string>(
			name: "LoginProvider",
			table: "AspNetUserTokens",
			type: "nvarchar(450)",
			nullable: false,
			oldClrType: typeof(string),
			oldType: "nvarchar(128)",
			oldMaxLength: 128);

		_ = migrationBuilder.AddColumn<string>(
			name: "FirstName",
			table: "AspNetUsers",
			type: "nvarchar(max)",
			nullable: true);

		_ = migrationBuilder.AddColumn<string>(
			name: "LastName",
			table: "AspNetUsers",
			type: "nvarchar(max)",
			nullable: true);

		_ = migrationBuilder.AddColumn<byte[]>(
			name: "ProfilePicture",
			table: "AspNetUsers",
			type: "varbinary(max)",
			nullable: true);

		_ = migrationBuilder.AddColumn<int>(
			name: "UsernameChangeLimit",
			table: "AspNetUsers",
			type: "int",
			nullable: false,
			defaultValue: 0);

		_ = migrationBuilder.AlterColumn<string>(
			name: "ProviderKey",
			table: "AspNetUserLogins",
			type: "nvarchar(450)",
			nullable: false,
			oldClrType: typeof(string),
			oldType: "nvarchar(128)",
			oldMaxLength: 128);

		_ = migrationBuilder.AlterColumn<string>(
			name: "LoginProvider",
			table: "AspNetUserLogins",
			type: "nvarchar(450)",
			nullable: false,
			oldClrType: typeof(string),
			oldType: "nvarchar(128)",
			oldMaxLength: 128);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder) {
		_ = migrationBuilder.DropColumn(
			name: "FirstName",
			table: "AspNetUsers");

		_ = migrationBuilder.DropColumn(
			name: "LastName",
			table: "AspNetUsers");

		_ = migrationBuilder.DropColumn(
			name: "ProfilePicture",
			table: "AspNetUsers");

		_ = migrationBuilder.DropColumn(
			name: "UsernameChangeLimit",
			table: "AspNetUsers");

		_ = migrationBuilder.AlterColumn<string>(
			name: "Name",
			table: "AspNetUserTokens",
			type: "nvarchar(128)",
			maxLength: 128,
			nullable: false,
			oldClrType: typeof(string),
			oldType: "nvarchar(450)");

		_ = migrationBuilder.AlterColumn<string>(
			name: "LoginProvider",
			table: "AspNetUserTokens",
			type: "nvarchar(128)",
			maxLength: 128,
			nullable: false,
			oldClrType: typeof(string),
			oldType: "nvarchar(450)");

		_ = migrationBuilder.AlterColumn<string>(
			name: "ProviderKey",
			table: "AspNetUserLogins",
			type: "nvarchar(128)",
			maxLength: 128,
			nullable: false,
			oldClrType: typeof(string),
			oldType: "nvarchar(450)");

		_ = migrationBuilder.AlterColumn<string>(
			name: "LoginProvider",
			table: "AspNetUserLogins",
			type: "nvarchar(128)",
			maxLength: 128,
			nullable: false,
			oldClrType: typeof(string),
			oldType: "nvarchar(450)");
	}
}

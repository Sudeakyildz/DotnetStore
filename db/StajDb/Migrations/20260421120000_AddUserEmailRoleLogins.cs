using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StajDb;

#nullable disable

namespace StajDb.Migrations;

/// <inheritdoc />
[DbContext(typeof(DataContext))]
[Migration("20260421120000_AddUserEmailRoleLogins")]
public class AddUserEmailRoleLogins : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Email",
            table: "Users",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "FirstLoginAt",
            table: "Users",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "LastLoginAt",
            table: "Users",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "Role",
            table: "Users",
            type: "int",
            nullable: true);

        migrationBuilder.Sql(
            """
            UPDATE Users
            SET Email = UserName + N'@legacy.local',
                Role = 1
            WHERE Email IS NULL;
            """);

        migrationBuilder.AlterColumn<string>(
            name: "Email",
            table: "Users",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(256)",
            oldMaxLength: 256,
            oldNullable: true);

        migrationBuilder.AlterColumn<int>(
            name: "Role",
            table: "Users",
            type: "int",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Users_Email",
            table: "Users",
            column: "Email",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Users_Email",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "Email",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "FirstLoginAt",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "LastLoginAt",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "Role",
            table: "Users");
    }
}

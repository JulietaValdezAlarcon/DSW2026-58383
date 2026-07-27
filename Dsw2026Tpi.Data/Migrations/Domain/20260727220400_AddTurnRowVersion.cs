using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dsw2026Tpi.Data.Migrations.Domain
{
    /// <inheritdoc />
    public partial class AddTurnRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Turns",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "TelephoneNumber",
                table: "Patients",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<Guid>(
                name: "TurnId",
                table: "Appointments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Turns");

            migrationBuilder.DropColumn(
                name: "TurnId",
                table: "Appointments");

            migrationBuilder.AlterColumn<int>(
                name: "TelephoneNumber",
                table: "Patients",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");
        }
    }
}

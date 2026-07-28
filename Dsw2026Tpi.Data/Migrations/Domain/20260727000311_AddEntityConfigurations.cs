using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dsw2026Tpi.Data.Migrations.Domain
{
    /// <inheritdoc />
    public partial class AddEntityConfigurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Availability_Doctors_DoctorId",
                table: "Availability");

            migrationBuilder.DropForeignKey(
                name: "FK_Doctors_Specialities_SpecialityId",
                table: "Doctors");

            migrationBuilder.DropForeignKey(
                name: "FK_Turn_Availability_AvailabilityId",
                table: "Turn");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Turn",
                table: "Turn");

            migrationBuilder.DropIndex(
                name: "IX_Turn_AvailabilityId",
                table: "Turn");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Availability",
                table: "Availability");

            migrationBuilder.RenameTable(
                name: "Turn",
                newName: "Turns");

            migrationBuilder.RenameTable(
                name: "Availability",
                newName: "Availabilities");

            migrationBuilder.RenameIndex(
                name: "IX_Availability_DoctorId",
                table: "Availabilities",
                newName: "IX_Availabilities_DoctorId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Specialities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Specialities",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Doctors",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "LicenseNumber",
                table: "Doctors",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Doctors",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Turns",
                table: "Turns",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Availabilities",
                table: "Availabilities",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DNI = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TelephoneNumber = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateOfService = table.Column<DateOnly>(type: "date", nullable: false),
                    CancellationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointments_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Turns_AppointmentId",
                table: "Turns",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Turns_AvailabilityId_Status",
                table: "Turns",
                columns: new[] { "AvailabilityId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Availabilities_Date_DoctorId",
                table: "Availabilities",
                columns: new[] { "Date", "DoctorId" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId",
                table: "Appointments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_DNI",
                table: "Patients",
                column: "DNI",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Availabilities_Doctors_DoctorId",
                table: "Availabilities",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_Specialities_SpecialityId",
                table: "Doctors",
                column: "SpecialityId",
                principalTable: "Specialities",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Turns_Appointments_AppointmentId",
                table: "Turns",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Turns_Availabilities_AvailabilityId",
                table: "Turns",
                column: "AvailabilityId",
                principalTable: "Availabilities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Availabilities_Doctors_DoctorId",
                table: "Availabilities");

            migrationBuilder.DropForeignKey(
                name: "FK_Doctors_Specialities_SpecialityId",
                table: "Doctors");

            migrationBuilder.DropForeignKey(
                name: "FK_Turns_Appointments_AppointmentId",
                table: "Turns");

            migrationBuilder.DropForeignKey(
                name: "FK_Turns_Availabilities_AvailabilityId",
                table: "Turns");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "Patients");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Turns",
                table: "Turns");

            migrationBuilder.DropIndex(
                name: "IX_Turns_AppointmentId",
                table: "Turns");

            migrationBuilder.DropIndex(
                name: "IX_Turns_AvailabilityId_Status",
                table: "Turns");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Availabilities",
                table: "Availabilities");

            migrationBuilder.DropIndex(
                name: "IX_Availabilities_Date_DoctorId",
                table: "Availabilities");

            migrationBuilder.RenameTable(
                name: "Turns",
                newName: "Turn");

            migrationBuilder.RenameTable(
                name: "Availabilities",
                newName: "Availability");

            migrationBuilder.RenameIndex(
                name: "IX_Availabilities_DoctorId",
                table: "Availability",
                newName: "IX_Availability_DoctorId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Specialities",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Specialities",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "LicenseNumber",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Doctors",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Turn",
                table: "Turn",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Availability",
                table: "Availability",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Turn_AvailabilityId",
                table: "Turn",
                column: "AvailabilityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Availability_Doctors_DoctorId",
                table: "Availability",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_Specialities_SpecialityId",
                table: "Doctors",
                column: "SpecialityId",
                principalTable: "Specialities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Turn_Availability_AvailabilityId",
                table: "Turn",
                column: "AvailabilityId",
                principalTable: "Availability",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

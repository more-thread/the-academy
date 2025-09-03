using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRS.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistrationAuditColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RegistrationAcceptedBy",
                schema: "TRS",
                table: "tTrainingRegistration",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegistrationAcceptedDate",
                schema: "TRS",
                table: "tTrainingRegistration",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationCanceledBy",
                schema: "TRS",
                table: "tTrainingRegistration",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegistrationCanceledDate",
                schema: "TRS",
                table: "tTrainingRegistration",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationCreatedBy",
                schema: "TRS",
                table: "tTrainingRegistration",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegistrationCreatedDate",
                schema: "TRS",
                table: "tTrainingRegistration",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationDeniedBy",
                schema: "TRS",
                table: "tTrainingRegistration",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegistrationDeniedDate",
                schema: "TRS",
                table: "tTrainingRegistration",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FormAccess",
                columns: table => new
                {
                    EmployeeNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccessibleDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentVersion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DevInitials = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DevInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubMenuID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubMenuName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Controller = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccessType = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "UserInfo",
                columns: table => new
                {
                    EmpID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PositionName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BranchID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BranchName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HREmployeeStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayPic = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    DepartmentID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DepartmentName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SectionID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SectionName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FormAccess");

            migrationBuilder.DropTable(
                name: "UserInfo");

            migrationBuilder.DropColumn(
                name: "RegistrationAcceptedBy",
                schema: "TRS",
                table: "tTrainingRegistration");

            migrationBuilder.DropColumn(
                name: "RegistrationAcceptedDate",
                schema: "TRS",
                table: "tTrainingRegistration");

            migrationBuilder.DropColumn(
                name: "RegistrationCanceledBy",
                schema: "TRS",
                table: "tTrainingRegistration");

            migrationBuilder.DropColumn(
                name: "RegistrationCanceledDate",
                schema: "TRS",
                table: "tTrainingRegistration");

            migrationBuilder.DropColumn(
                name: "RegistrationCreatedBy",
                schema: "TRS",
                table: "tTrainingRegistration");

            migrationBuilder.DropColumn(
                name: "RegistrationCreatedDate",
                schema: "TRS",
                table: "tTrainingRegistration");

            migrationBuilder.DropColumn(
                name: "RegistrationDeniedBy",
                schema: "TRS",
                table: "tTrainingRegistration");

            migrationBuilder.DropColumn(
                name: "RegistrationDeniedDate",
                schema: "TRS",
                table: "tTrainingRegistration");
        }
    }
}

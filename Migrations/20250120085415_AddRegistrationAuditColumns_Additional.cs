using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRS.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistrationAuditColumns_Additional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RegistrationApprovedBy",
                schema: "TRS",
                table: "tTrainingRegistration",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegistrationApprovedDate",
                schema: "TRS",
                table: "tTrainingRegistration",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationDisapprovedBy",
                schema: "TRS",
                table: "tTrainingRegistration",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegistrationDisapprovedDate",
                schema: "TRS",
                table: "tTrainingRegistration",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegistrationApprovedBy",
                schema: "TRS",
                table: "tTrainingRegistration");

            migrationBuilder.DropColumn(
                name: "RegistrationApprovedDate",
                schema: "TRS",
                table: "tTrainingRegistration");

            migrationBuilder.DropColumn(
                name: "RegistrationDisapprovedBy",
                schema: "TRS",
                table: "tTrainingRegistration");

            migrationBuilder.DropColumn(
                name: "RegistrationDisapprovedDate",
                schema: "TRS",
                table: "tTrainingRegistration");
        }
    }
}

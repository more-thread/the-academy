using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRS.Migrations
{
    /// <inheritdoc />
    public partial class AdditionalAuditTrailColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ScheduleCompletedBy",
                schema: "TRS",
                table: "tTrainingSchedule",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduleCompletedDate",
                schema: "TRS",
                table: "tTrainingSchedule",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationConfirmedBy",
                schema: "TRS",
                table: "tTrainingRegistration",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegistrationConfirmedDate",
                schema: "TRS",
                table: "tTrainingRegistration",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationRejectedBy",
                schema: "TRS",
                table: "tTrainingRegistration",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegistrationRejectedDate",
                schema: "TRS",
                table: "tTrainingRegistration",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScheduleCompletedBy",
                schema: "TRS",
                table: "tTrainingSchedule");

            migrationBuilder.DropColumn(
                name: "ScheduleCompletedDate",
                schema: "TRS",
                table: "tTrainingSchedule");

            migrationBuilder.DropColumn(
                name: "RegistrationConfirmedBy",
                schema: "TRS",
                table: "tTrainingRegistration");

            migrationBuilder.DropColumn(
                name: "RegistrationConfirmedDate",
                schema: "TRS",
                table: "tTrainingRegistration");

            migrationBuilder.DropColumn(
                name: "RegistrationRejectedBy",
                schema: "TRS",
                table: "tTrainingRegistration");

            migrationBuilder.DropColumn(
                name: "RegistrationRejectedDate",
                schema: "TRS",
                table: "tTrainingRegistration");
        }
    }
}

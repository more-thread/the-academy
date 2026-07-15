using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRS.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleAuditColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ScheduleCanceledBy",
                schema: "TRS",
                table: "tTrainingSchedule",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduleCanceledDate",
                schema: "TRS",
                table: "tTrainingSchedule",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScheduleCreatedBy",
                schema: "TRS",
                table: "tTrainingSchedule",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduleCreatedDate",
                schema: "TRS",
                table: "tTrainingSchedule",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScheduleModifiedBy",
                schema: "TRS",
                table: "tTrainingSchedule",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduleModifiedDate",
                schema: "TRS",
                table: "tTrainingSchedule",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScheduleCanceledBy",
                schema: "TRS",
                table: "tTrainingSchedule");

            migrationBuilder.DropColumn(
                name: "ScheduleCanceledDate",
                schema: "TRS",
                table: "tTrainingSchedule");

            migrationBuilder.DropColumn(
                name: "ScheduleCreatedBy",
                schema: "TRS",
                table: "tTrainingSchedule");

            migrationBuilder.DropColumn(
                name: "ScheduleCreatedDate",
                schema: "TRS",
                table: "tTrainingSchedule");

            migrationBuilder.DropColumn(
                name: "ScheduleModifiedBy",
                schema: "TRS",
                table: "tTrainingSchedule");

            migrationBuilder.DropColumn(
                name: "ScheduleModifiedDate",
                schema: "TRS",
                table: "tTrainingSchedule");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRS.Migrations
{
    /// <inheritdoc />
    public partial class NewColumnsInTrainingRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AbsenceReason",
                schema: "TRS",
                table: "tTrainingRegistration",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Attendance",
                schema: "TRS",
                table: "tTrainingRegistration",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "EvaluationScore",
                schema: "TRS",
                table: "tTrainingRegistration",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "PostTestFirstScore",
                schema: "TRS",
                table: "tTrainingRegistration",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "PostTestSecondScore",
                schema: "TRS",
                table: "tTrainingRegistration",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "PostTestStatus",
                schema: "TRS",
                table: "tTrainingRegistration",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PostTestThirdScore",
                schema: "TRS",
                table: "tTrainingRegistration",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AbsenceReason",
                schema: "TRS",
                table: "tTrainingRegistration");

            migrationBuilder.DropColumn(
                name: "Attendance",
                schema: "TRS",
                table: "tTrainingRegistration");

            migrationBuilder.DropColumn(
                name: "EvaluationScore",
                schema: "TRS",
                table: "tTrainingRegistration");

            migrationBuilder.DropColumn(
                name: "PostTestFirstScore",
                schema: "TRS",
                table: "tTrainingRegistration");

            migrationBuilder.DropColumn(
                name: "PostTestSecondScore",
                schema: "TRS",
                table: "tTrainingRegistration");

            migrationBuilder.DropColumn(
                name: "PostTestStatus",
                schema: "TRS",
                table: "tTrainingRegistration");

            migrationBuilder.DropColumn(
                name: "PostTestThirdScore",
                schema: "TRS",
                table: "tTrainingRegistration");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRS.Migrations
{
    /// <inheritdoc />
    public partial class NewColumnsInTrainingRegistrationPercentage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PostTestFirstScorePercentage",
                schema: "TRS",
                table: "tTrainingRegistration",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "PostTestSecondScorePercentage",
                schema: "TRS",
                table: "tTrainingRegistration",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "PostTestThirdScorePercentage",
                schema: "TRS",
                table: "tTrainingRegistration",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "TrainingFeedbackStatus",
                schema: "TRS",
                table: "tTrainingRegistration",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PostTestFirstScorePercentage",
                schema: "TRS",
                table: "tTrainingRegistration");

            migrationBuilder.DropColumn(
                name: "PostTestSecondScorePercentage",
                schema: "TRS",
                table: "tTrainingRegistration");

            migrationBuilder.DropColumn(
                name: "PostTestThirdScorePercentage",
                schema: "TRS",
                table: "tTrainingRegistration");

            migrationBuilder.DropColumn(
                name: "TrainingFeedbackStatus",
                schema: "TRS",
                table: "tTrainingRegistration");
        }
    }
}

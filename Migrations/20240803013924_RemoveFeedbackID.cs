using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRS.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFeedbackID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_tTrainingFeedback",
                schema: "TRS",
                table: "tTrainingFeedback");

            migrationBuilder.DropColumn(
                name: "FeedbackID",
                schema: "TRS",
                table: "tTrainingFeedback");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tTrainingFeedback",
                schema: "TRS",
                table: "tTrainingFeedback",
                column: "RecordNo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_tTrainingFeedback",
                schema: "TRS",
                table: "tTrainingFeedback");

            migrationBuilder.AddColumn<string>(
                name: "FeedbackID",
                schema: "TRS",
                table: "tTrainingFeedback",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tTrainingFeedback",
                schema: "TRS",
                table: "tTrainingFeedback",
                column: "FeedbackID");
        }
    }
}

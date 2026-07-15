using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRS.Migrations
{
    /// <inheritdoc />
    public partial class TrainingFeedbackReupload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mTrainingFeedbackQuestions",
                schema: "TRS",
                columns: table => new
                {
                    QuestionID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RecordNo = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuestionType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    QuestionSequence = table.Column<long>(type: "bigint", nullable: false),
                    CategorySequence = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedByComputerUsed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedByComputerUsed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mTrainingFeedbackQuestions", x => x.QuestionID);
                });

            migrationBuilder.CreateTable(
                name: "tTrainingFeedback",
                schema: "TRS",
                columns: table => new
                {
                    FeedbackID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RecordNo = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedByComputerUsed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedByComputerUsed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrainingFeedbackQuestionsQuestionID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TrainingRegistrationRegistrationCode = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tTrainingFeedback", x => x.FeedbackID);
                    table.ForeignKey(
                        name: "FK_tTrainingFeedback_mTrainingFeedbackQuestions_TrainingFeedbackQuestionsQuestionID",
                        column: x => x.TrainingFeedbackQuestionsQuestionID,
                        principalSchema: "TRS",
                        principalTable: "mTrainingFeedbackQuestions",
                        principalColumn: "QuestionID");
                    table.ForeignKey(
                        name: "FK_tTrainingFeedback_tTrainingRegistration_TrainingRegistrationRegistrationCode",
                        column: x => x.TrainingRegistrationRegistrationCode,
                        principalSchema: "TRS",
                        principalTable: "tTrainingRegistration",
                        principalColumn: "RegistrationCode");
                });

            migrationBuilder.CreateIndex(
                name: "IX_tTrainingFeedback_TrainingFeedbackQuestionsQuestionID",
                schema: "TRS",
                table: "tTrainingFeedback",
                column: "TrainingFeedbackQuestionsQuestionID");

            migrationBuilder.CreateIndex(
                name: "IX_tTrainingFeedback_TrainingRegistrationRegistrationCode",
                schema: "TRS",
                table: "tTrainingFeedback",
                column: "TrainingRegistrationRegistrationCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tTrainingFeedback",
                schema: "TRS");

            migrationBuilder.DropTable(
                name: "mTrainingFeedbackQuestions",
                schema: "TRS");
        }
    }
}

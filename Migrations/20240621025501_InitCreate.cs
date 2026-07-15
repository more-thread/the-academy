using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRS.Migrations
{
    /// <inheritdoc />
    public partial class InitCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "TRS");

            migrationBuilder.CreateTable(
                name: "mTrainingProgram",
                schema: "TRS",
                columns: table => new
                {
                    ProgramCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RecordNo = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_mTrainingProgram", x => x.ProgramCode);
                });

            migrationBuilder.CreateTable(
                name: "tUserLogs",
                schema: "TRS",
                columns: table => new
                {
                    RecordNo = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Activity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedByComputerUsed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tUserLogs", x => x.RecordNo);
                });

            migrationBuilder.CreateTable(
                name: "mTrainingCourse",
                schema: "TRS",
                columns: table => new
                {
                    CourseCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RecordNo = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CourseDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WithPreTest = table.Column<bool>(type: "bit", nullable: false),
                    PreTestTotalScore = table.Column<long>(type: "bigint", nullable: true),
                    WithPostTest = table.Column<bool>(type: "bit", nullable: false),
                    PostTestTotalScore = table.Column<long>(type: "bigint", nullable: true),
                    WithEvaluation = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedByComputerUsed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedByComputerUsed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProgramCode = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mTrainingCourse", x => x.CourseCode);
                    table.ForeignKey(
                        name: "FK_mTrainingCourse_mTrainingProgram_ProgramCode",
                        column: x => x.ProgramCode,
                        principalSchema: "TRS",
                        principalTable: "mTrainingProgram",
                        principalColumn: "ProgramCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tTrainingSchedule",
                schema: "TRS",
                columns: table => new
                {
                    TrainingCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RecordNo = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RegistrationStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Region = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrainingType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    ResourceSpeaker = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClassSize = table.Column<int>(type: "int", nullable: false),
                    MinimumParticipants = table.Column<int>(type: "int", nullable: false),
                    CostPerHead = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Venue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedByComputerUsed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedByComputerUsed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CourseCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ProgramCode = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tTrainingSchedule", x => x.TrainingCode);
                    table.ForeignKey(
                        name: "FK_tTrainingSchedule_mTrainingCourse_CourseCode",
                        column: x => x.CourseCode,
                        principalSchema: "TRS",
                        principalTable: "mTrainingCourse",
                        principalColumn: "CourseCode");
                    table.ForeignKey(
                        name: "FK_tTrainingSchedule_mTrainingProgram_ProgramCode",
                        column: x => x.ProgramCode,
                        principalSchema: "TRS",
                        principalTable: "mTrainingProgram",
                        principalColumn: "ProgramCode");
                });

            migrationBuilder.CreateTable(
                name: "mJobClass",
                schema: "TRS",
                columns: table => new
                {
                    RecordNo = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobClassCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JobClassDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedByComputerUsed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedByComputerUsed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProgramCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ScheduleTrainingCode = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mJobClass", x => x.RecordNo);
                    table.ForeignKey(
                        name: "FK_mJobClass_mTrainingProgram_ProgramCode",
                        column: x => x.ProgramCode,
                        principalSchema: "TRS",
                        principalTable: "mTrainingProgram",
                        principalColumn: "ProgramCode");
                    table.ForeignKey(
                        name: "FK_mJobClass_tTrainingSchedule_ScheduleTrainingCode",
                        column: x => x.ScheduleTrainingCode,
                        principalSchema: "TRS",
                        principalTable: "tTrainingSchedule",
                        principalColumn: "TrainingCode");
                });

            migrationBuilder.CreateIndex(
                name: "IX_mJobClass_ProgramCode",
                schema: "TRS",
                table: "mJobClass",
                column: "ProgramCode");

            migrationBuilder.CreateIndex(
                name: "IX_mJobClass_ScheduleTrainingCode",
                schema: "TRS",
                table: "mJobClass",
                column: "ScheduleTrainingCode");

            migrationBuilder.CreateIndex(
                name: "IX_mTrainingCourse_ProgramCode",
                schema: "TRS",
                table: "mTrainingCourse",
                column: "ProgramCode");

            migrationBuilder.CreateIndex(
                name: "IX_tTrainingSchedule_CourseCode",
                schema: "TRS",
                table: "tTrainingSchedule",
                column: "CourseCode");

            migrationBuilder.CreateIndex(
                name: "IX_tTrainingSchedule_ProgramCode",
                schema: "TRS",
                table: "tTrainingSchedule",
                column: "ProgramCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mJobClass",
                schema: "TRS");

            migrationBuilder.DropTable(
                name: "tUserLogs",
                schema: "TRS");

            migrationBuilder.DropTable(
                name: "tTrainingSchedule",
                schema: "TRS");

            migrationBuilder.DropTable(
                name: "mTrainingCourse",
                schema: "TRS");

            migrationBuilder.DropTable(
                name: "mTrainingProgram",
                schema: "TRS");
        }
    }
}

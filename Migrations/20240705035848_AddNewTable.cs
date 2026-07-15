using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRS.Migrations
{
    /// <inheritdoc />
    public partial class AddNewTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tTrainingRegistration",
                schema: "TRS",
                columns: table => new
                {
                    RegistrationCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RecordNo = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReasonForCancellation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReasonForDenying = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrainingScheduleTrainingCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
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
                    table.PrimaryKey("PK_tTrainingRegistration", x => x.RegistrationCode);
                    table.ForeignKey(
                        name: "FK_tTrainingRegistration_tTrainingSchedule_TrainingScheduleTrainingCode",
                        column: x => x.TrainingScheduleTrainingCode,
                        principalSchema: "TRS",
                        principalTable: "tTrainingSchedule",
                        principalColumn: "TrainingCode");
                });

            migrationBuilder.CreateIndex(
                name: "IX_tTrainingRegistration_TrainingScheduleTrainingCode",
                schema: "TRS",
                table: "tTrainingRegistration",
                column: "TrainingScheduleTrainingCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tTrainingRegistration",
                schema: "TRS");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRS.Migrations
{
    /// <inheritdoc />
    public partial class AddNewColumnInTrainingRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "EmployeeNo",
                schema: "TRS",
                table: "tTrainingRegistration",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrainingRegistrationStatus",
                schema: "TRS",
                table: "tTrainingRegistration",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrainingRegistrationStatus",
                schema: "TRS",
                table: "tTrainingRegistration");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeNo",
                schema: "TRS",
                table: "tTrainingRegistration",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}

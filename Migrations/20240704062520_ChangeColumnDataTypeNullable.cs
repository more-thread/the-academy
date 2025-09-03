using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRS.Migrations
{
    /// <inheritdoc />
    public partial class ChangeColumnDataTypeNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "CostPerHead",
                schema: "TRS",
                table: "tTrainingSchedule",
                type: "float",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "CostPerHead",
                schema: "TRS",
                table: "tTrainingSchedule",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);
        }
    }
}

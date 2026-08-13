using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRS.Migrations
{
    /// <inheritdoc />
    public partial class addCategoryCodeTrainingCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CategoryCode",
                schema: "TRS",
                table: "mTrainingCourse",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryCode",
                schema: "TRS",
                table: "mTrainingCourse");
        }
    }
}

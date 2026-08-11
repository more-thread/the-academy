using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRS.Migrations
{
    /// <inheritdoc />
    public partial class addedCourseCategoryinTrainingCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CourseCategory",
                schema: "TRS",
                table: "mTrainingCourse",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_mTrainingCourse_CourseCategory",
                schema: "TRS",
                table: "mTrainingCourse",
                column: "CourseCategory");

            migrationBuilder.AddForeignKey(
                name: "FK_mTrainingCourse_mCourseCategory_CourseCategory",
                schema: "TRS",
                table: "mTrainingCourse",
                column: "CourseCategory",
                principalSchema: "TRS",
                principalTable: "mCourseCategory",
                principalColumn: "CategoryCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_mTrainingCourse_mCourseCategory_CourseCategory",
                schema: "TRS",
                table: "mTrainingCourse");

            migrationBuilder.DropIndex(
                name: "IX_mTrainingCourse_CourseCategory",
                schema: "TRS",
                table: "mTrainingCourse");

            migrationBuilder.DropColumn(
                name: "CourseCategory",
                schema: "TRS",
                table: "mTrainingCourse");
        }
    }
}

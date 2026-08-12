using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRS.Migrations
{
    /// <inheritdoc />
    public partial class fixTrainingCategoryRelationshipIssue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CourseCategory",
                schema: "TRS",
                table: "mTrainingCourse",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "CRS-0003");

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
                principalColumn: "CategoryCode",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_mTrainingCourse_mCourseCategory_CourseCategory",
                schema: "TRS",
                table: "mTrainingCourse");

            migrationBuilder.DropTable(
                name: "FormAccess");

            migrationBuilder.DropTable(
                name: "UserInfo");

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

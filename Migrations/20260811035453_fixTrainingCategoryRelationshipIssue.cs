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
            migrationBuilder.Sql(@"
                IF NOT EXISTS(SELECT * FROM TRS.mCourseCategory WHERE CategoryCode = 'CRS-0003')
                BEGIN
	                SET IDENTITY_INSERT [TRS].[mCourseCategory] ON 
	                INSERT [TRS].[mCourseCategory] ([CategoryCode], [RecordNo], [CategoryTitle], [Status], [CreatedBy], [CreatedByComputerUsed], [DateCreated], [ModifiedBy], [ModifiedByComputerUsed], [DateModified]) VALUES (N'CRS-0003', 3, N'', 0, N'ROBE_VELORIA', N'IT01-VROBERT', CAST(N'2026-08-07T13:40:50.9900000' AS DateTime2), NULL, NULL, NULL)
	                SET IDENTITY_INSERT [TRS].[mCourseCategory] OFF
                END
            ");

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

            migrationBuilder.DropIndex(
                name: "IX_mTrainingCourse_CourseCategory",
                schema: "TRS",
                table: "mTrainingCourse");

            migrationBuilder.DropColumn(
                name: "CourseCategory",
                schema: "TRS",
                table: "mTrainingCourse");

            migrationBuilder.DeleteData(
                schema: "TRS",
                table: "mCourseCategory",
                keyColumn: "CategoryCode",
                keyValue: "CRS-0003");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddEntityRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Students");

            migrationBuilder.CreateIndex(
                name: "IX_Students_CreatedByUserId",
                table: "Students",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_MarketingUserId",
                table: "Students",
                column: "MarketingUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_OrganizationId",
                table: "Students",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_SupportUserId",
                table: "Students",
                column: "SupportUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_EnrollmentId",
                table: "Payments",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_CourseId",
                table: "Enrollments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_InstructorId",
                table: "Enrollments",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_StudentId",
                table: "Enrollments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_SupportUserId",
                table: "Enrollments",
                column: "SupportUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_InstructorId",
                table: "Courses",
                column: "InstructorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Users_InstructorId",
                table: "Courses",
                column: "InstructorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Courses_CourseId",
                table: "Enrollments",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Students_StudentId",
                table: "Enrollments",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Users_InstructorId",
                table: "Enrollments",
                column: "InstructorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Users_SupportUserId",
                table: "Enrollments",
                column: "SupportUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Enrollments_EnrollmentId",
                table: "Payments",
                column: "EnrollmentId",
                principalTable: "Enrollments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Organizations_OrganizationId",
                table: "Students",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Users_CreatedByUserId",
                table: "Students",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Users_MarketingUserId",
                table: "Students",
                column: "MarketingUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Users_SupportUserId",
                table: "Students",
                column: "SupportUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Users_InstructorId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Courses_CourseId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Students_StudentId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Users_InstructorId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Users_SupportUserId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Enrollments_EnrollmentId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Organizations_OrganizationId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Users_CreatedByUserId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Users_MarketingUserId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Users_SupportUserId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_CreatedByUserId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_MarketingUserId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_OrganizationId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_SupportUserId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Payments_EnrollmentId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_CourseId",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_InstructorId",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_StudentId",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_SupportUserId",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_Courses_InstructorId",
                table: "Courses");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Students",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Students",
                type: "TEXT",
                nullable: true);
        }
    }
}

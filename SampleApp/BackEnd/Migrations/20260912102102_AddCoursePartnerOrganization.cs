using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddCoursePartnerOrganization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CoursePartnerOrganizationId",
                table: "Enrollments",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CoursePartnerOrganizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CourseId = table.Column<int>(type: "INTEGER", nullable: false),
                    PartnerOrganizationId = table.Column<int>(type: "INTEGER", nullable: false),
                    ContractNumber = table.Column<string>(type: "TEXT", nullable: true),
                    AgreedPrice = table.Column<decimal>(type: "TEXT", nullable: true),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoursePartnerOrganizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoursePartnerOrganizations_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoursePartnerOrganizations_PartnerOrganizations_PartnerOrganizationId",
                        column: x => x.PartnerOrganizationId,
                        principalTable: "PartnerOrganizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_CoursePartnerOrganizationId",
                table: "Enrollments",
                column: "CoursePartnerOrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_CoursePartnerOrganizations_CourseId",
                table: "CoursePartnerOrganizations",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CoursePartnerOrganizations_PartnerOrganizationId",
                table: "CoursePartnerOrganizations",
                column: "PartnerOrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_CoursePartnerOrganizations_CoursePartnerOrganizationId",
                table: "Enrollments",
                column: "CoursePartnerOrganizationId",
                principalTable: "CoursePartnerOrganizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_CoursePartnerOrganizations_CoursePartnerOrganizationId",
                table: "Enrollments");

            migrationBuilder.DropTable(
                name: "CoursePartnerOrganizations");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_CoursePartnerOrganizationId",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "CoursePartnerOrganizationId",
                table: "Enrollments");
        }
    }
}

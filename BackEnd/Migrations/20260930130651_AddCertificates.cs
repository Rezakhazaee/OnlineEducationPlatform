using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddCertificates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EnrollmentId = table.Column<int>(type: "INTEGER", nullable: false),
                    CertificateNumber = table.Column<string>(type: "TEXT", nullable: false),
                    StudentName = table.Column<string>(type: "TEXT", nullable: false),
                    CourseTitle = table.Column<string>(type: "TEXT", nullable: false),
                    IssuedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Certificates_Enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_CertificateNumber",
                table: "Certificates",
                column: "CertificateNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_EnrollmentId",
                table: "Certificates",
                column: "EnrollmentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Certificates");
        }
    }
}

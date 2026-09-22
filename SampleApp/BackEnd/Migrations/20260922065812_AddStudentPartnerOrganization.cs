using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentPartnerOrganization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PartnerOrganizationId",
                table: "Students",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_PartnerOrganizationId",
                table: "Students",
                column: "PartnerOrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_PartnerOrganizations_PartnerOrganizationId",
                table: "Students",
                column: "PartnerOrganizationId",
                principalTable: "PartnerOrganizations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_PartnerOrganizations_PartnerOrganizationId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_PartnerOrganizationId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "PartnerOrganizationId",
                table: "Students");
        }
    }
}

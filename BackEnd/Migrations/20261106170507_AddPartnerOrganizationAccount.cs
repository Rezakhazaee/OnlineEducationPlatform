using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddPartnerOrganizationAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PartnerOrganizationId",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_PartnerOrganizationId",
                table: "Users",
                column: "PartnerOrganizationId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_PartnerOrganizations_PartnerOrganizationId",
                table: "Users",
                column: "PartnerOrganizationId",
                principalTable: "PartnerOrganizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_PartnerOrganizations_PartnerOrganizationId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_PartnerOrganizationId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PartnerOrganizationId",
                table: "Users");
        }
    }
}

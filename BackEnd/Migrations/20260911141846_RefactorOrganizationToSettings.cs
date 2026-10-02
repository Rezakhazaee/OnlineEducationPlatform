using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class RefactorOrganizationToSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_Organizations_OrganizationId",
                table: "Students");

            migrationBuilder.DropTable(
                name: "OrganizationThemes");

            migrationBuilder.DropIndex(
                name: "IX_Students_OrganizationId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Students");

            migrationBuilder.CreateTable(
                name: "OrganizationSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    PrimaryColor = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "#568fa8"),
                    SecondaryColor = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "#263d4a"),
                    ThemeIsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganizationSettings");

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Students",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrganizationThemes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    PrimaryColor = table.Column<string>(type: "TEXT", nullable: false),
                    SecondaryColor = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationThemes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationThemes_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Students_OrganizationId",
                table: "Students",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationThemes_OrganizationId",
                table: "OrganizationThemes",
                column: "OrganizationId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Organizations_OrganizationId",
                table: "Students",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

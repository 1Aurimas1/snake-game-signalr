using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace snake_game.Migrations
{
    /// <inheritdoc />
    public partial class TournamentFieldRenames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tournaments_Users_CreatorId",
                table: "Tournaments");

            migrationBuilder.DropIndex(
                name: "IX_Tournaments_CreatorId",
                table: "Tournaments");

            migrationBuilder.RenameColumn(
                name: "MaxUsers",
                table: "Tournaments",
                newName: "OrganizerId");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "Tournaments",
                newName: "MaxPlayers");

            migrationBuilder.CreateIndex(
                name: "IX_Tournaments_OrganizerId",
                table: "Tournaments",
                column: "OrganizerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tournaments_Users_OrganizerId",
                table: "Tournaments",
                column: "OrganizerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tournaments_Users_OrganizerId",
                table: "Tournaments");

            migrationBuilder.DropIndex(
                name: "IX_Tournaments_OrganizerId",
                table: "Tournaments");

            migrationBuilder.RenameColumn(
                name: "OrganizerId",
                table: "Tournaments",
                newName: "MaxUsers");

            migrationBuilder.RenameColumn(
                name: "MaxPlayers",
                table: "Tournaments",
                newName: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Tournaments_CreatorId",
                table: "Tournaments",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tournaments_Users_CreatorId",
                table: "Tournaments",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

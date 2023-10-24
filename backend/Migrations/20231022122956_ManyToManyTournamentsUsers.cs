using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace snake_game.Migrations
{
    /// <inheritdoc />
    public partial class ManyToManyTournamentsUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Tournaments_TournamentId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_TournamentId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TournamentId",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Participations_TournamentId",
                table: "Participations",
                column: "TournamentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Participations_Tournaments_TournamentId",
                table: "Participations",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Participations_Tournaments_TournamentId",
                table: "Participations");

            migrationBuilder.DropIndex(
                name: "IX_Participations_TournamentId",
                table: "Participations");

            migrationBuilder.AddColumn<int>(
                name: "TournamentId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_TournamentId",
                table: "Users",
                column: "TournamentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Tournaments_TournamentId",
                table: "Users",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "Id");
        }
    }
}

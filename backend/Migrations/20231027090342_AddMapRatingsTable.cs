using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace snake_game.Migrations
{
    /// <inheritdoc />
    public partial class AddMapRatingsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MapRating_Maps_MapId",
                table: "MapRating");

            migrationBuilder.DropForeignKey(
                name: "FK_MapRating_Users_UserId",
                table: "MapRating");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MapRating",
                table: "MapRating");

            migrationBuilder.RenameTable(
                name: "MapRating",
                newName: "MapRatings");

            migrationBuilder.RenameIndex(
                name: "IX_MapRating_UserId",
                table: "MapRatings",
                newName: "IX_MapRatings_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_MapRating_MapId",
                table: "MapRatings",
                newName: "IX_MapRatings_MapId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MapRatings",
                table: "MapRatings",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MapRatings_Maps_MapId",
                table: "MapRatings",
                column: "MapId",
                principalTable: "Maps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MapRatings_Users_UserId",
                table: "MapRatings",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MapRatings_Maps_MapId",
                table: "MapRatings");

            migrationBuilder.DropForeignKey(
                name: "FK_MapRatings_Users_UserId",
                table: "MapRatings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MapRatings",
                table: "MapRatings");

            migrationBuilder.RenameTable(
                name: "MapRatings",
                newName: "MapRating");

            migrationBuilder.RenameIndex(
                name: "IX_MapRatings_UserId",
                table: "MapRating",
                newName: "IX_MapRating_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_MapRatings_MapId",
                table: "MapRating",
                newName: "IX_MapRating_MapId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MapRating",
                table: "MapRating",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MapRating_Maps_MapId",
                table: "MapRating",
                column: "MapId",
                principalTable: "Maps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MapRating_Users_UserId",
                table: "MapRating",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

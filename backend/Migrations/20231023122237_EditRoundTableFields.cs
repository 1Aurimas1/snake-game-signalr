using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace snake_game.Migrations
{
    /// <inheritdoc />
    public partial class EditRoundTableFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Rounds");

            migrationBuilder.AddColumn<int>(
                name: "Index",
                table: "Rounds",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Index",
                table: "Rounds");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Rounds",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}

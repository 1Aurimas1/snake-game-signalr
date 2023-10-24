using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace snake_game.Migrations
{
    /// <inheritdoc />
    public partial class NewEntities_FieldChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MapObstacle_Maps_MapId",
                table: "MapObstacle");

            migrationBuilder.DropForeignKey(
                name: "FK_MapObstacle_Obstacle_ObstacleId",
                table: "MapObstacle");

            migrationBuilder.DropForeignKey(
                name: "FK_MapObstacle_Point_PositionId",
                table: "MapObstacle");

            migrationBuilder.DropForeignKey(
                name: "FK_Maps_Tournaments_TournamentId",
                table: "Maps");

            migrationBuilder.DropForeignKey(
                name: "FK_Maps_Users_UserId",
                table: "Maps");

            migrationBuilder.DropForeignKey(
                name: "FK_Point_Obstacle_ObstacleId",
                table: "Point");

            migrationBuilder.DropIndex(
                name: "IX_Maps_TournamentId",
                table: "Maps");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Point",
                table: "Point");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Obstacle",
                table: "Obstacle");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MapObstacle",
                table: "MapObstacle");

            migrationBuilder.DropColumn(
                name: "TournamentId",
                table: "Maps");

            migrationBuilder.RenameTable(
                name: "Point",
                newName: "Points");

            migrationBuilder.RenameTable(
                name: "Obstacle",
                newName: "Obstacles");

            migrationBuilder.RenameTable(
                name: "MapObstacle",
                newName: "MapObstacles");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Tournaments",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "Tournaments",
                newName: "EndTime");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Maps",
                newName: "CreatorId");

            migrationBuilder.RenameIndex(
                name: "IX_Maps_UserId",
                table: "Maps",
                newName: "IX_Maps_CreatorId");

            migrationBuilder.RenameIndex(
                name: "IX_Point_ObstacleId",
                table: "Points",
                newName: "IX_Points_ObstacleId");

            migrationBuilder.RenameIndex(
                name: "IX_MapObstacle_PositionId",
                table: "MapObstacles",
                newName: "IX_MapObstacles_PositionId");

            migrationBuilder.RenameIndex(
                name: "IX_MapObstacle_ObstacleId",
                table: "MapObstacles",
                newName: "IX_MapObstacles_ObstacleId");

            migrationBuilder.RenameIndex(
                name: "IX_MapObstacle_MapId",
                table: "MapObstacles",
                newName: "IX_MapObstacles_MapId");

            migrationBuilder.AddColumn<int>(
                name: "CurrentRound",
                table: "Tournaments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Points",
                table: "Points",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Obstacles",
                table: "Obstacles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MapObstacles",
                table: "MapObstacles",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Rounds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    MapId = table.Column<int>(type: "integer", nullable: false),
                    TournamentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rounds_Maps_MapId",
                        column: x => x.MapId,
                        principalTable: "Maps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Rounds_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_MapId",
                table: "Rounds",
                column: "MapId");

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_TournamentId",
                table: "Rounds",
                column: "TournamentId");

            migrationBuilder.AddForeignKey(
                name: "FK_MapObstacles_Maps_MapId",
                table: "MapObstacles",
                column: "MapId",
                principalTable: "Maps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MapObstacles_Obstacles_ObstacleId",
                table: "MapObstacles",
                column: "ObstacleId",
                principalTable: "Obstacles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MapObstacles_Points_PositionId",
                table: "MapObstacles",
                column: "PositionId",
                principalTable: "Points",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Maps_Users_CreatorId",
                table: "Maps",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Points_Obstacles_ObstacleId",
                table: "Points",
                column: "ObstacleId",
                principalTable: "Obstacles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MapObstacles_Maps_MapId",
                table: "MapObstacles");

            migrationBuilder.DropForeignKey(
                name: "FK_MapObstacles_Obstacles_ObstacleId",
                table: "MapObstacles");

            migrationBuilder.DropForeignKey(
                name: "FK_MapObstacles_Points_PositionId",
                table: "MapObstacles");

            migrationBuilder.DropForeignKey(
                name: "FK_Maps_Users_CreatorId",
                table: "Maps");

            migrationBuilder.DropForeignKey(
                name: "FK_Points_Obstacles_ObstacleId",
                table: "Points");

            migrationBuilder.DropTable(
                name: "Rounds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Points",
                table: "Points");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Obstacles",
                table: "Obstacles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MapObstacles",
                table: "MapObstacles");

            migrationBuilder.DropColumn(
                name: "CurrentRound",
                table: "Tournaments");

            migrationBuilder.RenameTable(
                name: "Points",
                newName: "Point");

            migrationBuilder.RenameTable(
                name: "Obstacles",
                newName: "Obstacle");

            migrationBuilder.RenameTable(
                name: "MapObstacles",
                newName: "MapObstacle");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "Tournaments",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "Tournaments",
                newName: "EndDate");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "Maps",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Maps_CreatorId",
                table: "Maps",
                newName: "IX_Maps_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Points_ObstacleId",
                table: "Point",
                newName: "IX_Point_ObstacleId");

            migrationBuilder.RenameIndex(
                name: "IX_MapObstacles_PositionId",
                table: "MapObstacle",
                newName: "IX_MapObstacle_PositionId");

            migrationBuilder.RenameIndex(
                name: "IX_MapObstacles_ObstacleId",
                table: "MapObstacle",
                newName: "IX_MapObstacle_ObstacleId");

            migrationBuilder.RenameIndex(
                name: "IX_MapObstacles_MapId",
                table: "MapObstacle",
                newName: "IX_MapObstacle_MapId");

            migrationBuilder.AddColumn<int>(
                name: "TournamentId",
                table: "Maps",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Point",
                table: "Point",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Obstacle",
                table: "Obstacle",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MapObstacle",
                table: "MapObstacle",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Maps_TournamentId",
                table: "Maps",
                column: "TournamentId");

            migrationBuilder.AddForeignKey(
                name: "FK_MapObstacle_Maps_MapId",
                table: "MapObstacle",
                column: "MapId",
                principalTable: "Maps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MapObstacle_Obstacle_ObstacleId",
                table: "MapObstacle",
                column: "ObstacleId",
                principalTable: "Obstacle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MapObstacle_Point_PositionId",
                table: "MapObstacle",
                column: "PositionId",
                principalTable: "Point",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Maps_Tournaments_TournamentId",
                table: "Maps",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Maps_Users_UserId",
                table: "Maps",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Point_Obstacle_ObstacleId",
                table: "Point",
                column: "ObstacleId",
                principalTable: "Obstacle",
                principalColumn: "Id");
        }
    }
}

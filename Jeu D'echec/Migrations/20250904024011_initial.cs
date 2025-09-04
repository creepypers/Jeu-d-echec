using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jeu_D_echec.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    TotalGamesPlayed = table.Column<int>(type: "int", nullable: false),
                    TotalWins = table.Column<int>(type: "int", nullable: false),
                    TotalLosses = table.Column<int>(type: "int", nullable: false),
                    TotalDraws = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastPlayed = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SavedGames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Player1Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Player2Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastPlayed = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MoveCount = table.Column<int>(type: "int", nullable: false),
                    CurrentPlayer = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    GameState = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedGames", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WhitePlayerId = table.Column<int>(type: "int", nullable: false),
                    BlackPlayerId = table.Column<int>(type: "int", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WhiteRatingBefore = table.Column<int>(type: "int", nullable: false),
                    WhiteRatingAfter = table.Column<int>(type: "int", nullable: false),
                    BlackRatingBefore = table.Column<int>(type: "int", nullable: false),
                    BlackRatingAfter = table.Column<int>(type: "int", nullable: false),
                    MoveCount = table.Column<int>(type: "int", nullable: false),
                    GameDuration = table.Column<TimeSpan>(type: "time", nullable: false),
                    PlayedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameResults_Players_BlackPlayerId",
                        column: x => x.BlackPlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameResults_Players_WhitePlayerId",
                        column: x => x.WhitePlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BoardStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SavedGameId = table.Column<int>(type: "int", nullable: false),
                    Row = table.Column<int>(type: "int", nullable: false),
                    Column = table.Column<int>(type: "int", nullable: false),
                    PieceType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PieceColor = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    HasMoved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoardStates_SavedGames_SavedGameId",
                        column: x => x.SavedGameId,
                        principalTable: "SavedGames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChessMoves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SavedGameId = table.Column<int>(type: "int", nullable: false),
                    FromRow = table.Column<int>(type: "int", nullable: false),
                    FromColumn = table.Column<int>(type: "int", nullable: false),
                    ToRow = table.Column<int>(type: "int", nullable: false),
                    ToColumn = table.Column<int>(type: "int", nullable: false),
                    PieceType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PieceColor = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CapturedPieceType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CapturedPieceColor = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PromotionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MoveNumber = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChessMoves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChessMoves_SavedGames_SavedGameId",
                        column: x => x.SavedGameId,
                        principalTable: "SavedGames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoardStates_SavedGameId",
                table: "BoardStates",
                column: "SavedGameId");

            migrationBuilder.CreateIndex(
                name: "IX_ChessMoves_SavedGameId",
                table: "ChessMoves",
                column: "SavedGameId");

            migrationBuilder.CreateIndex(
                name: "IX_GameResults_BlackPlayerId",
                table: "GameResults",
                column: "BlackPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_GameResults_WhitePlayerId",
                table: "GameResults",
                column: "WhitePlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_Email",
                table: "Players",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoardStates");

            migrationBuilder.DropTable(
                name: "ChessMoves");

            migrationBuilder.DropTable(
                name: "GameResults");

            migrationBuilder.DropTable(
                name: "SavedGames");

            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}

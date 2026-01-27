using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NemesisEuchre.DataAccess.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Games",
            columns: table => new
            {
                GameId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                GameStatus = table.Column<int>(type: "int", nullable: false),
                PlayersJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                Team1Score = table.Column<short>(type: "smallint", nullable: false),
                Team2Score = table.Column<short>(type: "smallint", nullable: false),
                WinningTeam = table.Column<int>(type: "int", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
            },
            constraints: table => table.PrimaryKey("PK_Games", x => x.GameId));

        migrationBuilder.CreateTable(
            name: "Deals",
            columns: table => new
            {
                DealId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                GameId = table.Column<int>(type: "int", nullable: false),
                DealNumber = table.Column<int>(type: "int", nullable: false),
                DealStatus = table.Column<int>(type: "int", nullable: false),
                DealerPosition = table.Column<int>(type: "int", nullable: true),
                DeckJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                UpCardJson = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                Trump = table.Column<int>(type: "int", nullable: true),
                CallingPlayer = table.Column<int>(type: "int", nullable: true),
                CallingPlayerIsGoingAlone = table.Column<bool>(type: "bit", nullable: false),
                DealResult = table.Column<int>(type: "int", nullable: true),
                WinningTeam = table.Column<int>(type: "int", nullable: true),
                Team1Score = table.Column<short>(type: "smallint", nullable: false),
                Team2Score = table.Column<short>(type: "smallint", nullable: false),
                PlayersJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Deals", x => x.DealId);
                table.ForeignKey(
                    name: "FK_Deals_Games_GameId",
                    column: x => x.GameId,
                    principalTable: "Games",
                    principalColumn: "GameId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "CallTrumpDecisions",
            columns: table => new
            {
                CallTrumpDecisionId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                DealId = table.Column<int>(type: "int", nullable: false),
                HandJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                UpCardJson = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                DealerPosition = table.Column<int>(type: "int", nullable: false),
                DecidingPlayerPosition = table.Column<int>(type: "int", nullable: false),
                TeamScore = table.Column<short>(type: "smallint", nullable: false),
                OpponentScore = table.Column<short>(type: "smallint", nullable: false),
                ValidDecisionsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ChosenDecisionJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                DecisionOrder = table.Column<byte>(type: "tinyint", nullable: false),
                ActorType = table.Column<int>(type: "int", nullable: true),
                DidTeamWinDeal = table.Column<bool>(type: "bit", nullable: true),
                DidTeamWinGame = table.Column<bool>(type: "bit", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CallTrumpDecisions", x => x.CallTrumpDecisionId);
                table.ForeignKey(
                    name: "FK_CallTrumpDecisions_Deals_DealId",
                    column: x => x.DealId,
                    principalTable: "Deals",
                    principalColumn: "DealId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "DiscardCardDecisions",
            columns: table => new
            {
                DiscardCardDecisionId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                DealId = table.Column<int>(type: "int", nullable: false),
                HandJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                DealerPosition = table.Column<int>(type: "int", nullable: false),
                TeamScore = table.Column<short>(type: "smallint", nullable: false),
                OpponentScore = table.Column<short>(type: "smallint", nullable: false),
                ValidCardsToDiscardJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ChosenCardJson = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                ActorType = table.Column<int>(type: "int", nullable: true),
                DidTeamWinDeal = table.Column<bool>(type: "bit", nullable: true),
                DidTeamWinGame = table.Column<bool>(type: "bit", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DiscardCardDecisions", x => x.DiscardCardDecisionId);
                table.ForeignKey(
                    name: "FK_DiscardCardDecisions_Deals_DealId",
                    column: x => x.DealId,
                    principalTable: "Deals",
                    principalColumn: "DealId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PlayCardDecisions",
            columns: table => new
            {
                PlayCardDecisionId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                DealId = table.Column<int>(type: "int", nullable: false),
                TrickNumber = table.Column<int>(type: "int", nullable: false),
                HandJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                DecidingPlayerPosition = table.Column<int>(type: "int", nullable: false),
                CurrentTrickJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                TeamScore = table.Column<short>(type: "smallint", nullable: false),
                OpponentScore = table.Column<short>(type: "smallint", nullable: false),
                ValidCardsToPlayJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ChosenCardJson = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                LeadPosition = table.Column<int>(type: "int", nullable: false),
                ActorType = table.Column<int>(type: "int", nullable: true),
                DidTeamWinTrick = table.Column<bool>(type: "bit", nullable: true),
                DidTeamWinDeal = table.Column<bool>(type: "bit", nullable: true),
                DidTeamWinGame = table.Column<bool>(type: "bit", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PlayCardDecisions", x => x.PlayCardDecisionId);
                table.ForeignKey(
                    name: "FK_PlayCardDecisions_Deals_DealId",
                    column: x => x.DealId,
                    principalTable: "Deals",
                    principalColumn: "DealId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Tricks",
            columns: table => new
            {
                TrickId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                DealId = table.Column<int>(type: "int", nullable: false),
                TrickNumber = table.Column<int>(type: "int", nullable: false),
                LeadPosition = table.Column<int>(type: "int", nullable: false),
                CardsPlayedJson = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                LeadSuit = table.Column<int>(type: "int", nullable: true),
                WinningPosition = table.Column<int>(type: "int", nullable: true),
                WinningTeam = table.Column<int>(type: "int", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Tricks", x => x.TrickId);
                table.ForeignKey(
                    name: "FK_Tricks_Deals_DealId",
                    column: x => x.DealId,
                    principalTable: "Deals",
                    principalColumn: "DealId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_CallTrumpDecisions_ActorType",
            table: "CallTrumpDecisions",
            column: "ActorType");

        migrationBuilder.CreateIndex(
            name: "IX_CallTrumpDecisions_DealId",
            table: "CallTrumpDecisions",
            column: "DealId");

        migrationBuilder.CreateIndex(
            name: "IX_Deals_GameId",
            table: "Deals",
            column: "GameId");

        migrationBuilder.CreateIndex(
            name: "IX_DiscardCardDecisions_ActorType",
            table: "DiscardCardDecisions",
            column: "ActorType");

        migrationBuilder.CreateIndex(
            name: "IX_DiscardCardDecisions_DealId",
            table: "DiscardCardDecisions",
            column: "DealId");

        migrationBuilder.CreateIndex(
            name: "IX_Games_CreatedAt",
            table: "Games",
            column: "CreatedAt");

        migrationBuilder.CreateIndex(
            name: "IX_Games_WinningTeam",
            table: "Games",
            column: "WinningTeam");

        migrationBuilder.CreateIndex(
            name: "IX_PlayCardDecisions_ActorType_TrickNumber",
            table: "PlayCardDecisions",
            columns: ["ActorType", "TrickNumber"]);

        migrationBuilder.CreateIndex(
            name: "IX_PlayCardDecisions_DealId",
            table: "PlayCardDecisions",
            column: "DealId");

        migrationBuilder.CreateIndex(
            name: "IX_Tricks_DealId",
            table: "Tricks",
            column: "DealId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "CallTrumpDecisions");

        migrationBuilder.DropTable(
            name: "DiscardCardDecisions");

        migrationBuilder.DropTable(
            name: "PlayCardDecisions");

        migrationBuilder.DropTable(
            name: "Tricks");

        migrationBuilder.DropTable(
            name: "Deals");

        migrationBuilder.DropTable(
            name: "Games");
    }
}

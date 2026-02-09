using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NemesisEuchre.DataAccess.Migrations;

/// <inheritdoc />
public partial class NormalizeJsonColumnsToTables : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ActorTypes",
            columns: table => new
            {
                ActorTypeId = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
            },
            constraints: table => table.PrimaryKey("PK_ActorTypes", x => x.ActorTypeId));

        migrationBuilder.CreateTable(
            name: "CallTrumpDecisionValues",
            columns: table => new
            {
                CallTrumpDecisionValueId = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
            },
            constraints: table => table.PrimaryKey("PK_CallTrumpDecisionValues", x => x.CallTrumpDecisionValueId));

        migrationBuilder.CreateTable(
            name: "DealResults",
            columns: table => new
            {
                DealResultId = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
            },
            constraints: table => table.PrimaryKey("PK_DealResults", x => x.DealResultId));

        migrationBuilder.CreateTable(
            name: "DealStatuses",
            columns: table => new
            {
                DealStatusId = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
            },
            constraints: table => table.PrimaryKey("PK_DealStatuses", x => x.DealStatusId));

        migrationBuilder.CreateTable(
            name: "GameStatuses",
            columns: table => new
            {
                GameStatusId = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
            },
            constraints: table => table.PrimaryKey("PK_GameStatuses", x => x.GameStatusId));

        migrationBuilder.CreateTable(
            name: "PlayerPositions",
            columns: table => new
            {
                PlayerPositionId = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
            },
            constraints: table => table.PrimaryKey("PK_PlayerPositions", x => x.PlayerPositionId));

        migrationBuilder.CreateTable(
            name: "Ranks",
            columns: table => new
            {
                RankId = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
            },
            constraints: table => table.PrimaryKey("PK_Ranks", x => x.RankId));

        migrationBuilder.CreateTable(
            name: "RelativePlayerPositions",
            columns: table => new
            {
                RelativePlayerPositionId = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
            },
            constraints: table => table.PrimaryKey("PK_RelativePlayerPositions", x => x.RelativePlayerPositionId));

        migrationBuilder.CreateTable(
            name: "RelativeSuits",
            columns: table => new
            {
                RelativeSuitId = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
            },
            constraints: table => table.PrimaryKey("PK_RelativeSuits", x => x.RelativeSuitId));

        migrationBuilder.CreateTable(
            name: "Suits",
            columns: table => new
            {
                SuitId = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
            },
            constraints: table => table.PrimaryKey("PK_Suits", x => x.SuitId));

        migrationBuilder.CreateTable(
            name: "Teams",
            columns: table => new
            {
                TeamId = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
            },
            constraints: table => table.PrimaryKey("PK_Teams", x => x.TeamId));

        migrationBuilder.CreateTable(
            name: "RelativeCards",
            columns: table => new
            {
                RelativeCardId = table.Column<int>(type: "int", nullable: false),
                RelativeSuitId = table.Column<int>(type: "int", nullable: false),
                RankId = table.Column<int>(type: "int", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RelativeCards", x => x.RelativeCardId);
                table.ForeignKey(
                    name: "FK_RelativeCards_Ranks_RankId",
                    column: x => x.RankId,
                    principalTable: "Ranks",
                    principalColumn: "RankId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_RelativeCards_RelativeSuits_RelativeSuitId",
                    column: x => x.RelativeSuitId,
                    principalTable: "RelativeSuits",
                    principalColumn: "RelativeSuitId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Cards",
            columns: table => new
            {
                CardId = table.Column<int>(type: "int", nullable: false),
                SuitId = table.Column<int>(type: "int", nullable: false),
                RankId = table.Column<int>(type: "int", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Cards", x => x.CardId);
                table.ForeignKey(
                    name: "FK_Cards_Ranks_RankId",
                    column: x => x.RankId,
                    principalTable: "Ranks",
                    principalColumn: "RankId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Cards_Suits_SuitId",
                    column: x => x.SuitId,
                    principalTable: "Suits",
                    principalColumn: "SuitId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Games",
            columns: table => new
            {
                GameId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                GameStatusId = table.Column<int>(type: "int", nullable: false),
                Team1Score = table.Column<short>(type: "smallint", nullable: false),
                Team2Score = table.Column<short>(type: "smallint", nullable: false),
                WinningTeamId = table.Column<int>(type: "int", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Games", x => x.GameId);
                table.ForeignKey(
                    name: "FK_Games_GameStatuses_GameStatusId",
                    column: x => x.GameStatusId,
                    principalTable: "GameStatuses",
                    principalColumn: "GameStatusId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Games_Teams_WinningTeamId",
                    column: x => x.WinningTeamId,
                    principalTable: "Teams",
                    principalColumn: "TeamId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Deals",
            columns: table => new
            {
                DealId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                GameId = table.Column<int>(type: "int", nullable: false),
                DealNumber = table.Column<int>(type: "int", nullable: false),
                DealStatusId = table.Column<int>(type: "int", nullable: false),
                DealerPositionId = table.Column<int>(type: "int", nullable: true),
                UpCardId = table.Column<int>(type: "int", nullable: true),
                DiscardedCardId = table.Column<int>(type: "int", nullable: true),
                TrumpSuitId = table.Column<int>(type: "int", nullable: true),
                CallingPlayerPositionId = table.Column<int>(type: "int", nullable: true),
                CallingPlayerIsGoingAlone = table.Column<bool>(type: "bit", nullable: false),
                ChosenCallTrumpDecisionId = table.Column<int>(type: "int", nullable: true),
                DealResultId = table.Column<int>(type: "int", nullable: true),
                WinningTeamId = table.Column<int>(type: "int", nullable: true),
                Team1Score = table.Column<short>(type: "smallint", nullable: false),
                Team2Score = table.Column<short>(type: "smallint", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Deals", x => x.DealId);
                table.ForeignKey(
                    name: "FK_Deals_CallTrumpDecisionValues_ChosenCallTrumpDecisionId",
                    column: x => x.ChosenCallTrumpDecisionId,
                    principalTable: "CallTrumpDecisionValues",
                    principalColumn: "CallTrumpDecisionValueId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Deals_Cards_DiscardedCardId",
                    column: x => x.DiscardedCardId,
                    principalTable: "Cards",
                    principalColumn: "CardId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Deals_Cards_UpCardId",
                    column: x => x.UpCardId,
                    principalTable: "Cards",
                    principalColumn: "CardId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Deals_DealResults_DealResultId",
                    column: x => x.DealResultId,
                    principalTable: "DealResults",
                    principalColumn: "DealResultId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Deals_DealStatuses_DealStatusId",
                    column: x => x.DealStatusId,
                    principalTable: "DealStatuses",
                    principalColumn: "DealStatusId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Deals_Games_GameId",
                    column: x => x.GameId,
                    principalTable: "Games",
                    principalColumn: "GameId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Deals_PlayerPositions_CallingPlayerPositionId",
                    column: x => x.CallingPlayerPositionId,
                    principalTable: "PlayerPositions",
                    principalColumn: "PlayerPositionId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Deals_PlayerPositions_DealerPositionId",
                    column: x => x.DealerPositionId,
                    principalTable: "PlayerPositions",
                    principalColumn: "PlayerPositionId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Deals_Suits_TrumpSuitId",
                    column: x => x.TrumpSuitId,
                    principalTable: "Suits",
                    principalColumn: "SuitId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Deals_Teams_WinningTeamId",
                    column: x => x.WinningTeamId,
                    principalTable: "Teams",
                    principalColumn: "TeamId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "GamePlayers",
            columns: table => new
            {
                GameId = table.Column<int>(type: "int", nullable: false),
                PlayerPositionId = table.Column<int>(type: "int", nullable: false),
                ActorTypeId = table.Column<int>(type: "int", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GamePlayers", x => new { x.GameId, x.PlayerPositionId });
                table.ForeignKey(
                    name: "FK_GamePlayers_ActorTypes_ActorTypeId",
                    column: x => x.ActorTypeId,
                    principalTable: "ActorTypes",
                    principalColumn: "ActorTypeId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_GamePlayers_Games_GameId",
                    column: x => x.GameId,
                    principalTable: "Games",
                    principalColumn: "GameId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_GamePlayers_PlayerPositions_PlayerPositionId",
                    column: x => x.PlayerPositionId,
                    principalTable: "PlayerPositions",
                    principalColumn: "PlayerPositionId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "CallTrumpDecisions",
            columns: table => new
            {
                CallTrumpDecisionId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                DealId = table.Column<int>(type: "int", nullable: false),
                DealerRelativePositionId = table.Column<int>(type: "int", nullable: false),
                UpCardId = table.Column<int>(type: "int", nullable: false),
                TeamScore = table.Column<short>(type: "smallint", nullable: false),
                OpponentScore = table.Column<short>(type: "smallint", nullable: false),
                ChosenDecisionValueId = table.Column<int>(type: "int", nullable: false),
                DecisionOrder = table.Column<byte>(type: "tinyint", nullable: false),
                ActorTypeId = table.Column<int>(type: "int", nullable: true),
                DidTeamWinDeal = table.Column<bool>(type: "bit", nullable: true),
                RelativeDealPoints = table.Column<short>(type: "smallint", nullable: true),
                DidTeamWinGame = table.Column<bool>(type: "bit", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CallTrumpDecisions", x => x.CallTrumpDecisionId);
                table.ForeignKey(
                    name: "FK_CallTrumpDecisions_ActorTypes_ActorTypeId",
                    column: x => x.ActorTypeId,
                    principalTable: "ActorTypes",
                    principalColumn: "ActorTypeId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_CallTrumpDecisions_CallTrumpDecisionValues_ChosenDecisionValueId",
                    column: x => x.ChosenDecisionValueId,
                    principalTable: "CallTrumpDecisionValues",
                    principalColumn: "CallTrumpDecisionValueId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_CallTrumpDecisions_Cards_UpCardId",
                    column: x => x.UpCardId,
                    principalTable: "Cards",
                    principalColumn: "CardId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_CallTrumpDecisions_Deals_DealId",
                    column: x => x.DealId,
                    principalTable: "Deals",
                    principalColumn: "DealId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_CallTrumpDecisions_RelativePlayerPositions_DealerRelativePositionId",
                    column: x => x.DealerRelativePositionId,
                    principalTable: "RelativePlayerPositions",
                    principalColumn: "RelativePlayerPositionId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "DealDeckCards",
            columns: table => new
            {
                DealId = table.Column<int>(type: "int", nullable: false),
                CardId = table.Column<int>(type: "int", nullable: false),
                SortOrder = table.Column<int>(type: "int", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DealDeckCards", x => new { x.DealId, x.CardId });
                table.ForeignKey(
                    name: "FK_DealDeckCards_Cards_CardId",
                    column: x => x.CardId,
                    principalTable: "Cards",
                    principalColumn: "CardId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_DealDeckCards_Deals_DealId",
                    column: x => x.DealId,
                    principalTable: "Deals",
                    principalColumn: "DealId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "DealKnownPlayerSuitVoids",
            columns: table => new
            {
                DealId = table.Column<int>(type: "int", nullable: false),
                PlayerPositionId = table.Column<int>(type: "int", nullable: false),
                SuitId = table.Column<int>(type: "int", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DealKnownPlayerSuitVoids", x => new { x.DealId, x.PlayerPositionId, x.SuitId });
                table.ForeignKey(
                    name: "FK_DealKnownPlayerSuitVoids_Deals_DealId",
                    column: x => x.DealId,
                    principalTable: "Deals",
                    principalColumn: "DealId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_DealKnownPlayerSuitVoids_PlayerPositions_PlayerPositionId",
                    column: x => x.PlayerPositionId,
                    principalTable: "PlayerPositions",
                    principalColumn: "PlayerPositionId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_DealKnownPlayerSuitVoids_Suits_SuitId",
                    column: x => x.SuitId,
                    principalTable: "Suits",
                    principalColumn: "SuitId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "DealPlayers",
            columns: table => new
            {
                DealPlayerId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                DealId = table.Column<int>(type: "int", nullable: false),
                PlayerPositionId = table.Column<int>(type: "int", nullable: false),
                ActorTypeId = table.Column<int>(type: "int", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DealPlayers", x => x.DealPlayerId);
                table.ForeignKey(
                    name: "FK_DealPlayers_ActorTypes_ActorTypeId",
                    column: x => x.ActorTypeId,
                    principalTable: "ActorTypes",
                    principalColumn: "ActorTypeId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_DealPlayers_Deals_DealId",
                    column: x => x.DealId,
                    principalTable: "Deals",
                    principalColumn: "DealId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_DealPlayers_PlayerPositions_PlayerPositionId",
                    column: x => x.PlayerPositionId,
                    principalTable: "PlayerPositions",
                    principalColumn: "PlayerPositionId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "DiscardCardDecisions",
            columns: table => new
            {
                DiscardCardDecisionId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                DealId = table.Column<int>(type: "int", nullable: false),
                CallingRelativePlayerPositionId = table.Column<int>(type: "int", nullable: false),
                CallingPlayerGoingAlone = table.Column<bool>(type: "bit", nullable: false),
                TeamScore = table.Column<short>(type: "smallint", nullable: false),
                OpponentScore = table.Column<short>(type: "smallint", nullable: false),
                ChosenRelativeCardId = table.Column<int>(type: "int", nullable: false),
                ActorTypeId = table.Column<int>(type: "int", nullable: true),
                DidTeamWinDeal = table.Column<bool>(type: "bit", nullable: true),
                RelativeDealPoints = table.Column<short>(type: "smallint", nullable: true),
                DidTeamWinGame = table.Column<bool>(type: "bit", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DiscardCardDecisions", x => x.DiscardCardDecisionId);
                table.ForeignKey(
                    name: "FK_DiscardCardDecisions_ActorTypes_ActorTypeId",
                    column: x => x.ActorTypeId,
                    principalTable: "ActorTypes",
                    principalColumn: "ActorTypeId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_DiscardCardDecisions_Deals_DealId",
                    column: x => x.DealId,
                    principalTable: "Deals",
                    principalColumn: "DealId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_DiscardCardDecisions_RelativeCards_ChosenRelativeCardId",
                    column: x => x.ChosenRelativeCardId,
                    principalTable: "RelativeCards",
                    principalColumn: "RelativeCardId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_DiscardCardDecisions_RelativePlayerPositions_CallingRelativePlayerPositionId",
                    column: x => x.CallingRelativePlayerPositionId,
                    principalTable: "RelativePlayerPositions",
                    principalColumn: "RelativePlayerPositionId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Tricks",
            columns: table => new
            {
                TrickId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                DealId = table.Column<int>(type: "int", nullable: false),
                TrickNumber = table.Column<int>(type: "int", nullable: false),
                LeadPlayerPositionId = table.Column<int>(type: "int", nullable: false),
                LeadSuitId = table.Column<int>(type: "int", nullable: true),
                WinningPlayerPositionId = table.Column<int>(type: "int", nullable: true),
                WinningTeamId = table.Column<int>(type: "int", nullable: true),
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
                table.ForeignKey(
                    name: "FK_Tricks_PlayerPositions_LeadPlayerPositionId",
                    column: x => x.LeadPlayerPositionId,
                    principalTable: "PlayerPositions",
                    principalColumn: "PlayerPositionId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Tricks_PlayerPositions_WinningPlayerPositionId",
                    column: x => x.WinningPlayerPositionId,
                    principalTable: "PlayerPositions",
                    principalColumn: "PlayerPositionId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Tricks_Suits_LeadSuitId",
                    column: x => x.LeadSuitId,
                    principalTable: "Suits",
                    principalColumn: "SuitId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Tricks_Teams_WinningTeamId",
                    column: x => x.WinningTeamId,
                    principalTable: "Teams",
                    principalColumn: "TeamId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "CallTrumpDecisionCardsInHand",
            columns: table => new
            {
                CallTrumpDecisionId = table.Column<int>(type: "int", nullable: false),
                CardId = table.Column<int>(type: "int", nullable: false),
                SortOrder = table.Column<int>(type: "int", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CallTrumpDecisionCardsInHand", x => new { x.CallTrumpDecisionId, x.CardId });
                table.ForeignKey(
                    name: "FK_CallTrumpDecisionCardsInHand_CallTrumpDecisions_CallTrumpDecisionId",
                    column: x => x.CallTrumpDecisionId,
                    principalTable: "CallTrumpDecisions",
                    principalColumn: "CallTrumpDecisionId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_CallTrumpDecisionCardsInHand_Cards_CardId",
                    column: x => x.CardId,
                    principalTable: "Cards",
                    principalColumn: "CardId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "CallTrumpDecisionPredictedPoints",
            columns: table => new
            {
                CallTrumpDecisionId = table.Column<int>(type: "int", nullable: false),
                CallTrumpDecisionValueId = table.Column<int>(type: "int", nullable: false),
                PredictedPoints = table.Column<float>(type: "real", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CallTrumpDecisionPredictedPoints", x => new { x.CallTrumpDecisionId, x.CallTrumpDecisionValueId });
                table.ForeignKey(
                    name: "FK_CallTrumpDecisionPredictedPoints_CallTrumpDecisionValues_CallTrumpDecisionValueId",
                    column: x => x.CallTrumpDecisionValueId,
                    principalTable: "CallTrumpDecisionValues",
                    principalColumn: "CallTrumpDecisionValueId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_CallTrumpDecisionPredictedPoints_CallTrumpDecisions_CallTrumpDecisionId",
                    column: x => x.CallTrumpDecisionId,
                    principalTable: "CallTrumpDecisions",
                    principalColumn: "CallTrumpDecisionId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "CallTrumpValidDecisions",
            columns: table => new
            {
                CallTrumpDecisionId = table.Column<int>(type: "int", nullable: false),
                CallTrumpDecisionValueId = table.Column<int>(type: "int", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CallTrumpValidDecisions", x => new { x.CallTrumpDecisionId, x.CallTrumpDecisionValueId });
                table.ForeignKey(
                    name: "FK_CallTrumpValidDecisions_CallTrumpDecisionValues_CallTrumpDecisionValueId",
                    column: x => x.CallTrumpDecisionValueId,
                    principalTable: "CallTrumpDecisionValues",
                    principalColumn: "CallTrumpDecisionValueId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_CallTrumpValidDecisions_CallTrumpDecisions_CallTrumpDecisionId",
                    column: x => x.CallTrumpDecisionId,
                    principalTable: "CallTrumpDecisions",
                    principalColumn: "CallTrumpDecisionId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "DealPlayerStartingHandCards",
            columns: table => new
            {
                DealPlayerId = table.Column<int>(type: "int", nullable: false),
                CardId = table.Column<int>(type: "int", nullable: false),
                SortOrder = table.Column<int>(type: "int", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DealPlayerStartingHandCards", x => new { x.DealPlayerId, x.CardId });
                table.ForeignKey(
                    name: "FK_DealPlayerStartingHandCards_Cards_CardId",
                    column: x => x.CardId,
                    principalTable: "Cards",
                    principalColumn: "CardId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_DealPlayerStartingHandCards_DealPlayers_DealPlayerId",
                    column: x => x.DealPlayerId,
                    principalTable: "DealPlayers",
                    principalColumn: "DealPlayerId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "DiscardCardDecisionCardsInHand",
            columns: table => new
            {
                DiscardCardDecisionId = table.Column<int>(type: "int", nullable: false),
                RelativeCardId = table.Column<int>(type: "int", nullable: false),
                SortOrder = table.Column<int>(type: "int", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DiscardCardDecisionCardsInHand", x => new { x.DiscardCardDecisionId, x.RelativeCardId });
                table.ForeignKey(
                    name: "FK_DiscardCardDecisionCardsInHand_DiscardCardDecisions_DiscardCardDecisionId",
                    column: x => x.DiscardCardDecisionId,
                    principalTable: "DiscardCardDecisions",
                    principalColumn: "DiscardCardDecisionId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_DiscardCardDecisionCardsInHand_RelativeCards_RelativeCardId",
                    column: x => x.RelativeCardId,
                    principalTable: "RelativeCards",
                    principalColumn: "RelativeCardId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "DiscardCardDecisionPredictedPoints",
            columns: table => new
            {
                DiscardCardDecisionId = table.Column<int>(type: "int", nullable: false),
                RelativeCardId = table.Column<int>(type: "int", nullable: false),
                PredictedPoints = table.Column<float>(type: "real", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DiscardCardDecisionPredictedPoints", x => new { x.DiscardCardDecisionId, x.RelativeCardId });
                table.ForeignKey(
                    name: "FK_DiscardCardDecisionPredictedPoints_DiscardCardDecisions_DiscardCardDecisionId",
                    column: x => x.DiscardCardDecisionId,
                    principalTable: "DiscardCardDecisions",
                    principalColumn: "DiscardCardDecisionId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_DiscardCardDecisionPredictedPoints_RelativeCards_RelativeCardId",
                    column: x => x.RelativeCardId,
                    principalTable: "RelativeCards",
                    principalColumn: "RelativeCardId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "PlayCardDecisions",
            columns: table => new
            {
                PlayCardDecisionId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                DealId = table.Column<int>(type: "int", nullable: false),
                TrickId = table.Column<int>(type: "int", nullable: false),
                TeamScore = table.Column<short>(type: "smallint", nullable: false),
                OpponentScore = table.Column<short>(type: "smallint", nullable: false),
                LeadRelativePlayerPositionId = table.Column<int>(type: "int", nullable: false),
                LeadRelativeSuitId = table.Column<int>(type: "int", nullable: true),
                WinningTrickRelativePlayerPositionId = table.Column<int>(type: "int", nullable: true),
                TrickNumber = table.Column<short>(type: "smallint", nullable: false),
                CallingRelativePlayerPositionId = table.Column<int>(type: "int", nullable: false),
                CallingPlayerGoingAlone = table.Column<bool>(type: "bit", nullable: false),
                DealerRelativePlayerPositionId = table.Column<int>(type: "int", nullable: false),
                DealerPickedUpRelativeCardId = table.Column<int>(type: "int", nullable: true),
                ChosenRelativeCardId = table.Column<int>(type: "int", nullable: false),
                ActorTypeId = table.Column<int>(type: "int", nullable: true),
                DidTeamWinTrick = table.Column<bool>(type: "bit", nullable: true),
                DidTeamWinDeal = table.Column<bool>(type: "bit", nullable: true),
                RelativeDealPoints = table.Column<short>(type: "smallint", nullable: true),
                DidTeamWinGame = table.Column<bool>(type: "bit", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PlayCardDecisions", x => x.PlayCardDecisionId);
                table.ForeignKey(
                    name: "FK_PlayCardDecisions_ActorTypes_ActorTypeId",
                    column: x => x.ActorTypeId,
                    principalTable: "ActorTypes",
                    principalColumn: "ActorTypeId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_PlayCardDecisions_Deals_DealId",
                    column: x => x.DealId,
                    principalTable: "Deals",
                    principalColumn: "DealId");
                table.ForeignKey(
                    name: "FK_PlayCardDecisions_RelativeCards_ChosenRelativeCardId",
                    column: x => x.ChosenRelativeCardId,
                    principalTable: "RelativeCards",
                    principalColumn: "RelativeCardId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_PlayCardDecisions_RelativeCards_DealerPickedUpRelativeCardId",
                    column: x => x.DealerPickedUpRelativeCardId,
                    principalTable: "RelativeCards",
                    principalColumn: "RelativeCardId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_PlayCardDecisions_RelativePlayerPositions_CallingRelativePlayerPositionId",
                    column: x => x.CallingRelativePlayerPositionId,
                    principalTable: "RelativePlayerPositions",
                    principalColumn: "RelativePlayerPositionId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_PlayCardDecisions_RelativePlayerPositions_DealerRelativePlayerPositionId",
                    column: x => x.DealerRelativePlayerPositionId,
                    principalTable: "RelativePlayerPositions",
                    principalColumn: "RelativePlayerPositionId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_PlayCardDecisions_RelativePlayerPositions_LeadRelativePlayerPositionId",
                    column: x => x.LeadRelativePlayerPositionId,
                    principalTable: "RelativePlayerPositions",
                    principalColumn: "RelativePlayerPositionId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_PlayCardDecisions_RelativePlayerPositions_WinningTrickRelativePlayerPositionId",
                    column: x => x.WinningTrickRelativePlayerPositionId,
                    principalTable: "RelativePlayerPositions",
                    principalColumn: "RelativePlayerPositionId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_PlayCardDecisions_RelativeSuits_LeadRelativeSuitId",
                    column: x => x.LeadRelativeSuitId,
                    principalTable: "RelativeSuits",
                    principalColumn: "RelativeSuitId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_PlayCardDecisions_Tricks_TrickId",
                    column: x => x.TrickId,
                    principalTable: "Tricks",
                    principalColumn: "TrickId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "TrickCardsPlayed",
            columns: table => new
            {
                TrickId = table.Column<int>(type: "int", nullable: false),
                PlayerPositionId = table.Column<int>(type: "int", nullable: false),
                CardId = table.Column<int>(type: "int", nullable: false),
                PlayOrder = table.Column<int>(type: "int", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TrickCardsPlayed", x => new { x.TrickId, x.PlayerPositionId });
                table.ForeignKey(
                    name: "FK_TrickCardsPlayed_Cards_CardId",
                    column: x => x.CardId,
                    principalTable: "Cards",
                    principalColumn: "CardId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TrickCardsPlayed_PlayerPositions_PlayerPositionId",
                    column: x => x.PlayerPositionId,
                    principalTable: "PlayerPositions",
                    principalColumn: "PlayerPositionId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TrickCardsPlayed_Tricks_TrickId",
                    column: x => x.TrickId,
                    principalTable: "Tricks",
                    principalColumn: "TrickId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PlayCardDecisionCardsAccountedFor",
            columns: table => new
            {
                PlayCardDecisionId = table.Column<int>(type: "int", nullable: false),
                RelativeCardId = table.Column<int>(type: "int", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PlayCardDecisionCardsAccountedFor", x => new { x.PlayCardDecisionId, x.RelativeCardId });
                table.ForeignKey(
                    name: "FK_PlayCardDecisionCardsAccountedFor_PlayCardDecisions_PlayCardDecisionId",
                    column: x => x.PlayCardDecisionId,
                    principalTable: "PlayCardDecisions",
                    principalColumn: "PlayCardDecisionId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PlayCardDecisionCardsAccountedFor_RelativeCards_RelativeCardId",
                    column: x => x.RelativeCardId,
                    principalTable: "RelativeCards",
                    principalColumn: "RelativeCardId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "PlayCardDecisionCardsInHand",
            columns: table => new
            {
                PlayCardDecisionId = table.Column<int>(type: "int", nullable: false),
                RelativeCardId = table.Column<int>(type: "int", nullable: false),
                SortOrder = table.Column<int>(type: "int", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PlayCardDecisionCardsInHand", x => new { x.PlayCardDecisionId, x.RelativeCardId });
                table.ForeignKey(
                    name: "FK_PlayCardDecisionCardsInHand_PlayCardDecisions_PlayCardDecisionId",
                    column: x => x.PlayCardDecisionId,
                    principalTable: "PlayCardDecisions",
                    principalColumn: "PlayCardDecisionId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PlayCardDecisionCardsInHand_RelativeCards_RelativeCardId",
                    column: x => x.RelativeCardId,
                    principalTable: "RelativeCards",
                    principalColumn: "RelativeCardId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "PlayCardDecisionKnownVoids",
            columns: table => new
            {
                PlayCardDecisionId = table.Column<int>(type: "int", nullable: false),
                RelativePlayerPositionId = table.Column<int>(type: "int", nullable: false),
                RelativeSuitId = table.Column<int>(type: "int", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PlayCardDecisionKnownVoids", x => new { x.PlayCardDecisionId, x.RelativePlayerPositionId, x.RelativeSuitId });
                table.ForeignKey(
                    name: "FK_PlayCardDecisionKnownVoids_PlayCardDecisions_PlayCardDecisionId",
                    column: x => x.PlayCardDecisionId,
                    principalTable: "PlayCardDecisions",
                    principalColumn: "PlayCardDecisionId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PlayCardDecisionKnownVoids_RelativePlayerPositions_RelativePlayerPositionId",
                    column: x => x.RelativePlayerPositionId,
                    principalTable: "RelativePlayerPositions",
                    principalColumn: "RelativePlayerPositionId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_PlayCardDecisionKnownVoids_RelativeSuits_RelativeSuitId",
                    column: x => x.RelativeSuitId,
                    principalTable: "RelativeSuits",
                    principalColumn: "RelativeSuitId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "PlayCardDecisionPlayedCards",
            columns: table => new
            {
                PlayCardDecisionId = table.Column<int>(type: "int", nullable: false),
                RelativePlayerPositionId = table.Column<int>(type: "int", nullable: false),
                RelativeCardId = table.Column<int>(type: "int", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PlayCardDecisionPlayedCards", x => new { x.PlayCardDecisionId, x.RelativePlayerPositionId });
                table.ForeignKey(
                    name: "FK_PlayCardDecisionPlayedCards_PlayCardDecisions_PlayCardDecisionId",
                    column: x => x.PlayCardDecisionId,
                    principalTable: "PlayCardDecisions",
                    principalColumn: "PlayCardDecisionId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PlayCardDecisionPlayedCards_RelativeCards_RelativeCardId",
                    column: x => x.RelativeCardId,
                    principalTable: "RelativeCards",
                    principalColumn: "RelativeCardId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_PlayCardDecisionPlayedCards_RelativePlayerPositions_RelativePlayerPositionId",
                    column: x => x.RelativePlayerPositionId,
                    principalTable: "RelativePlayerPositions",
                    principalColumn: "RelativePlayerPositionId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "PlayCardDecisionPredictedPoints",
            columns: table => new
            {
                PlayCardDecisionId = table.Column<int>(type: "int", nullable: false),
                RelativeCardId = table.Column<int>(type: "int", nullable: false),
                PredictedPoints = table.Column<float>(type: "real", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PlayCardDecisionPredictedPoints", x => new { x.PlayCardDecisionId, x.RelativeCardId });
                table.ForeignKey(
                    name: "FK_PlayCardDecisionPredictedPoints_PlayCardDecisions_PlayCardDecisionId",
                    column: x => x.PlayCardDecisionId,
                    principalTable: "PlayCardDecisions",
                    principalColumn: "PlayCardDecisionId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PlayCardDecisionPredictedPoints_RelativeCards_RelativeCardId",
                    column: x => x.RelativeCardId,
                    principalTable: "RelativeCards",
                    principalColumn: "RelativeCardId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "PlayCardDecisionValidCards",
            columns: table => new
            {
                PlayCardDecisionId = table.Column<int>(type: "int", nullable: false),
                RelativeCardId = table.Column<int>(type: "int", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PlayCardDecisionValidCards", x => new { x.PlayCardDecisionId, x.RelativeCardId });
                table.ForeignKey(
                    name: "FK_PlayCardDecisionValidCards_PlayCardDecisions_PlayCardDecisionId",
                    column: x => x.PlayCardDecisionId,
                    principalTable: "PlayCardDecisions",
                    principalColumn: "PlayCardDecisionId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PlayCardDecisionValidCards_RelativeCards_RelativeCardId",
                    column: x => x.RelativeCardId,
                    principalTable: "RelativeCards",
                    principalColumn: "RelativeCardId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.InsertData(
            table: "ActorTypes",
            columns: ["ActorTypeId", "Name"],
            values: new object[,]
            {
                { 0, "User" },
                { 1, "Chaos" },
                { 2, "Chad" },
                { 3, "Beta" },
                { 10, "Gen1" },
                { 11, "Gen1Trainer" },
            });

        migrationBuilder.InsertData(
            table: "CallTrumpDecisionValues",
            columns: ["CallTrumpDecisionValueId", "Name"],
            values: new object[,]
            {
                { 0, "Pass" },
                { 1, "CallSpades" },
                { 2, "CallHearts" },
                { 3, "CallClubs" },
                { 4, "CallDiamonds" },
                { 5, "CallSpadesAndGoAlone" },
                { 6, "CallHeartsAndGoAlone" },
                { 7, "CallClubsAndGoAlone" },
                { 8, "CallDiamondsAndGoAlone" },
                { 9, "OrderItUp" },
                { 10, "OrderItUpAndGoAlone" },
            });

        migrationBuilder.InsertData(
            table: "DealResults",
            columns: ["DealResultId", "Name"],
            values: new object[,]
            {
                { 1, "WonStandardBid" },
                { 2, "WonGotAllTricks" },
                { 3, "OpponentsEuchred" },
                { 4, "WonAndWentAlone" },
                { 5, "ThrowIn" },
            });

        migrationBuilder.InsertData(
            table: "DealStatuses",
            columns: ["DealStatusId", "Name"],
            values: new object[,]
            {
                { 0, "NotStarted" },
                { 1, "SelectingTrump" },
                { 2, "Playing" },
                { 3, "Scoring" },
                { 4, "Complete" },
            });

        migrationBuilder.InsertData(
            table: "GameStatuses",
            columns: ["GameStatusId", "Name"],
            values: new object[,]
            {
                { 0, "NotStarted" },
                { 1, "Playing" },
                { 2, "Complete" },
            });

        migrationBuilder.InsertData(
            table: "PlayerPositions",
            columns: ["PlayerPositionId", "Name"],
            values: new object[,]
            {
                { 0, "North" },
                { 1, "East" },
                { 2, "South" },
                { 3, "West" },
            });

        migrationBuilder.InsertData(
            table: "Ranks",
            columns: ["RankId", "Name"],
            values: new object[,]
            {
                { 9, "Nine" },
                { 10, "Ten" },
                { 11, "Jack" },
                { 12, "Queen" },
                { 13, "King" },
                { 14, "Ace" },
                { 15, "LeftBower" },
                { 16, "RightBower" },
            });

        migrationBuilder.InsertData(
            table: "RelativePlayerPositions",
            columns: ["RelativePlayerPositionId", "Name"],
            values: new object[,]
            {
                { 0, "Self" },
                { 1, "LeftHandOpponent" },
                { 2, "Partner" },
                { 3, "RightHandOpponent" },
            });

        migrationBuilder.InsertData(
            table: "RelativeSuits",
            columns: ["RelativeSuitId", "Name"],
            values: new object[,]
            {
                { 0, "Trump" },
                { 1, "NonTrumpSameColor" },
                { 2, "NonTrumpOppositeColor1" },
                { 3, "NonTrumpOppositeColor2" },
            });

        migrationBuilder.InsertData(
            table: "Suits",
            columns: ["SuitId", "Name"],
            values: new object[,]
            {
                { 1, "Spades" },
                { 2, "Hearts" },
                { 3, "Clubs" },
                { 4, "Diamonds" },
            });

        migrationBuilder.InsertData(
            table: "Teams",
            columns: ["TeamId", "Name"],
            values: new object[,]
            {
                { 0, "Team1" },
                { 1, "Team2" },
            });

        migrationBuilder.InsertData(
            table: "Cards",
            columns: ["CardId", "RankId", "SuitId"],
            values: new object[,]
            {
                { 109, 9, 1 },
                { 110, 10, 1 },
                { 111, 11, 1 },
                { 112, 12, 1 },
                { 113, 13, 1 },
                { 114, 14, 1 },
                { 209, 9, 2 },
                { 210, 10, 2 },
                { 211, 11, 2 },
                { 212, 12, 2 },
                { 213, 13, 2 },
                { 214, 14, 2 },
                { 309, 9, 3 },
                { 310, 10, 3 },
                { 311, 11, 3 },
                { 312, 12, 3 },
                { 313, 13, 3 },
                { 314, 14, 3 },
                { 409, 9, 4 },
                { 410, 10, 4 },
                { 411, 11, 4 },
                { 412, 12, 4 },
                { 413, 13, 4 },
                { 414, 14, 4 },
            });

        migrationBuilder.InsertData(
            table: "RelativeCards",
            columns: ["RelativeCardId", "RankId", "RelativeSuitId"],
            values: new object[,]
            {
                { 9, 9, 0 },
                { 10, 10, 0 },
                { 11, 11, 0 },
                { 12, 12, 0 },
                { 13, 13, 0 },
                { 14, 14, 0 },
                { 15, 15, 0 },
                { 16, 16, 0 },
                { 109, 9, 1 },
                { 110, 10, 1 },
                { 111, 11, 1 },
                { 112, 12, 1 },
                { 113, 13, 1 },
                { 114, 14, 1 },
                { 115, 15, 1 },
                { 116, 16, 1 },
                { 209, 9, 2 },
                { 210, 10, 2 },
                { 211, 11, 2 },
                { 212, 12, 2 },
                { 213, 13, 2 },
                { 214, 14, 2 },
                { 215, 15, 2 },
                { 216, 16, 2 },
                { 309, 9, 3 },
                { 310, 10, 3 },
                { 311, 11, 3 },
                { 312, 12, 3 },
                { 313, 13, 3 },
                { 314, 14, 3 },
                { 315, 15, 3 },
                { 316, 16, 3 },
            });

        migrationBuilder.CreateIndex(
            name: "IX_CallTrumpDecisionCardsInHand_CardId",
            table: "CallTrumpDecisionCardsInHand",
            column: "CardId");

        migrationBuilder.CreateIndex(
            name: "IX_CallTrumpDecisionPredictedPoints_CallTrumpDecisionValueId",
            table: "CallTrumpDecisionPredictedPoints",
            column: "CallTrumpDecisionValueId");

        migrationBuilder.CreateIndex(
            name: "IX_CallTrumpDecisions_ActorTypeId",
            table: "CallTrumpDecisions",
            column: "ActorTypeId");

        migrationBuilder.CreateIndex(
            name: "IX_CallTrumpDecisions_ChosenDecisionValueId",
            table: "CallTrumpDecisions",
            column: "ChosenDecisionValueId");

        migrationBuilder.CreateIndex(
            name: "IX_CallTrumpDecisions_DealerRelativePositionId",
            table: "CallTrumpDecisions",
            column: "DealerRelativePositionId");

        migrationBuilder.CreateIndex(
            name: "IX_CallTrumpDecisions_DealId",
            table: "CallTrumpDecisions",
            column: "DealId");

        migrationBuilder.CreateIndex(
            name: "IX_CallTrumpDecisions_UpCardId",
            table: "CallTrumpDecisions",
            column: "UpCardId");

        migrationBuilder.CreateIndex(
            name: "IX_CallTrumpValidDecisions_CallTrumpDecisionValueId",
            table: "CallTrumpValidDecisions",
            column: "CallTrumpDecisionValueId");

        migrationBuilder.CreateIndex(
            name: "IX_Cards_RankId",
            table: "Cards",
            column: "RankId");

        migrationBuilder.CreateIndex(
            name: "IX_Cards_SuitId",
            table: "Cards",
            column: "SuitId");

        migrationBuilder.CreateIndex(
            name: "IX_DealDeckCards_CardId",
            table: "DealDeckCards",
            column: "CardId");

        migrationBuilder.CreateIndex(
            name: "IX_DealKnownPlayerSuitVoids_PlayerPositionId",
            table: "DealKnownPlayerSuitVoids",
            column: "PlayerPositionId");

        migrationBuilder.CreateIndex(
            name: "IX_DealKnownPlayerSuitVoids_SuitId",
            table: "DealKnownPlayerSuitVoids",
            column: "SuitId");

        migrationBuilder.CreateIndex(
            name: "IX_DealPlayers_ActorTypeId",
            table: "DealPlayers",
            column: "ActorTypeId");

        migrationBuilder.CreateIndex(
            name: "IX_DealPlayers_DealId_PlayerPositionId",
            table: "DealPlayers",
            columns: ["DealId", "PlayerPositionId"],
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_DealPlayers_PlayerPositionId",
            table: "DealPlayers",
            column: "PlayerPositionId");

        migrationBuilder.CreateIndex(
            name: "IX_DealPlayerStartingHandCards_CardId",
            table: "DealPlayerStartingHandCards",
            column: "CardId");

        migrationBuilder.CreateIndex(
            name: "IX_Deals_CallingPlayerPositionId",
            table: "Deals",
            column: "CallingPlayerPositionId");

        migrationBuilder.CreateIndex(
            name: "IX_Deals_ChosenCallTrumpDecisionId",
            table: "Deals",
            column: "ChosenCallTrumpDecisionId");

        migrationBuilder.CreateIndex(
            name: "IX_Deals_DealerPositionId",
            table: "Deals",
            column: "DealerPositionId");

        migrationBuilder.CreateIndex(
            name: "IX_Deals_DealResultId",
            table: "Deals",
            column: "DealResultId");

        migrationBuilder.CreateIndex(
            name: "IX_Deals_DealStatusId",
            table: "Deals",
            column: "DealStatusId");

        migrationBuilder.CreateIndex(
            name: "IX_Deals_DiscardedCardId",
            table: "Deals",
            column: "DiscardedCardId");

        migrationBuilder.CreateIndex(
            name: "IX_Deals_GameId",
            table: "Deals",
            column: "GameId");

        migrationBuilder.CreateIndex(
            name: "IX_Deals_TrumpSuitId",
            table: "Deals",
            column: "TrumpSuitId");

        migrationBuilder.CreateIndex(
            name: "IX_Deals_UpCardId",
            table: "Deals",
            column: "UpCardId");

        migrationBuilder.CreateIndex(
            name: "IX_Deals_WinningTeamId",
            table: "Deals",
            column: "WinningTeamId");

        migrationBuilder.CreateIndex(
            name: "IX_DiscardCardDecisionCardsInHand_RelativeCardId",
            table: "DiscardCardDecisionCardsInHand",
            column: "RelativeCardId");

        migrationBuilder.CreateIndex(
            name: "IX_DiscardCardDecisionPredictedPoints_RelativeCardId",
            table: "DiscardCardDecisionPredictedPoints",
            column: "RelativeCardId");

        migrationBuilder.CreateIndex(
            name: "IX_DiscardCardDecisions_ActorTypeId",
            table: "DiscardCardDecisions",
            column: "ActorTypeId");

        migrationBuilder.CreateIndex(
            name: "IX_DiscardCardDecisions_CallingRelativePlayerPositionId",
            table: "DiscardCardDecisions",
            column: "CallingRelativePlayerPositionId");

        migrationBuilder.CreateIndex(
            name: "IX_DiscardCardDecisions_ChosenRelativeCardId",
            table: "DiscardCardDecisions",
            column: "ChosenRelativeCardId");

        migrationBuilder.CreateIndex(
            name: "IX_DiscardCardDecisions_DealId",
            table: "DiscardCardDecisions",
            column: "DealId");

        migrationBuilder.CreateIndex(
            name: "IX_GamePlayers_ActorTypeId",
            table: "GamePlayers",
            column: "ActorTypeId");

        migrationBuilder.CreateIndex(
            name: "IX_GamePlayers_PlayerPositionId",
            table: "GamePlayers",
            column: "PlayerPositionId");

        migrationBuilder.CreateIndex(
            name: "IX_Games_CreatedAt",
            table: "Games",
            column: "CreatedAt");

        migrationBuilder.CreateIndex(
            name: "IX_Games_GameStatusId",
            table: "Games",
            column: "GameStatusId");

        migrationBuilder.CreateIndex(
            name: "IX_Games_WinningTeamId",
            table: "Games",
            column: "WinningTeamId");

        migrationBuilder.CreateIndex(
            name: "IX_PlayCardDecisionCardsAccountedFor_RelativeCardId",
            table: "PlayCardDecisionCardsAccountedFor",
            column: "RelativeCardId");

        migrationBuilder.CreateIndex(
            name: "IX_PlayCardDecisionCardsInHand_RelativeCardId",
            table: "PlayCardDecisionCardsInHand",
            column: "RelativeCardId");

        migrationBuilder.CreateIndex(
            name: "IX_PlayCardDecisionKnownVoids_RelativePlayerPositionId",
            table: "PlayCardDecisionKnownVoids",
            column: "RelativePlayerPositionId");

        migrationBuilder.CreateIndex(
            name: "IX_PlayCardDecisionKnownVoids_RelativeSuitId",
            table: "PlayCardDecisionKnownVoids",
            column: "RelativeSuitId");

        migrationBuilder.CreateIndex(
            name: "IX_PlayCardDecisionPlayedCards_RelativeCardId",
            table: "PlayCardDecisionPlayedCards",
            column: "RelativeCardId");

        migrationBuilder.CreateIndex(
            name: "IX_PlayCardDecisionPlayedCards_RelativePlayerPositionId",
            table: "PlayCardDecisionPlayedCards",
            column: "RelativePlayerPositionId");

        migrationBuilder.CreateIndex(
            name: "IX_PlayCardDecisionPredictedPoints_RelativeCardId",
            table: "PlayCardDecisionPredictedPoints",
            column: "RelativeCardId");

        migrationBuilder.CreateIndex(
            name: "IX_PlayCardDecisions_ActorTypeId",
            table: "PlayCardDecisions",
            column: "ActorTypeId");

        migrationBuilder.CreateIndex(
            name: "IX_PlayCardDecisions_CallingRelativePlayerPositionId",
            table: "PlayCardDecisions",
            column: "CallingRelativePlayerPositionId");

        migrationBuilder.CreateIndex(
            name: "IX_PlayCardDecisions_ChosenRelativeCardId",
            table: "PlayCardDecisions",
            column: "ChosenRelativeCardId");

        migrationBuilder.CreateIndex(
            name: "IX_PlayCardDecisions_DealerPickedUpRelativeCardId",
            table: "PlayCardDecisions",
            column: "DealerPickedUpRelativeCardId");

        migrationBuilder.CreateIndex(
            name: "IX_PlayCardDecisions_DealerRelativePlayerPositionId",
            table: "PlayCardDecisions",
            column: "DealerRelativePlayerPositionId");

        migrationBuilder.CreateIndex(
            name: "IX_PlayCardDecisions_DealId",
            table: "PlayCardDecisions",
            column: "DealId");

        migrationBuilder.CreateIndex(
            name: "IX_PlayCardDecisions_LeadRelativePlayerPositionId",
            table: "PlayCardDecisions",
            column: "LeadRelativePlayerPositionId");

        migrationBuilder.CreateIndex(
            name: "IX_PlayCardDecisions_LeadRelativeSuitId",
            table: "PlayCardDecisions",
            column: "LeadRelativeSuitId");

        migrationBuilder.CreateIndex(
            name: "IX_PlayCardDecisions_TrickId",
            table: "PlayCardDecisions",
            column: "TrickId");

        migrationBuilder.CreateIndex(
            name: "IX_PlayCardDecisions_WinningTrickRelativePlayerPositionId",
            table: "PlayCardDecisions",
            column: "WinningTrickRelativePlayerPositionId");

        migrationBuilder.CreateIndex(
            name: "IX_PlayCardDecisionValidCards_RelativeCardId",
            table: "PlayCardDecisionValidCards",
            column: "RelativeCardId");

        migrationBuilder.CreateIndex(
            name: "IX_RelativeCards_RankId",
            table: "RelativeCards",
            column: "RankId");

        migrationBuilder.CreateIndex(
            name: "IX_RelativeCards_RelativeSuitId",
            table: "RelativeCards",
            column: "RelativeSuitId");

        migrationBuilder.CreateIndex(
            name: "IX_TrickCardsPlayed_CardId",
            table: "TrickCardsPlayed",
            column: "CardId");

        migrationBuilder.CreateIndex(
            name: "IX_TrickCardsPlayed_PlayerPositionId",
            table: "TrickCardsPlayed",
            column: "PlayerPositionId");

        migrationBuilder.CreateIndex(
            name: "IX_Tricks_DealId",
            table: "Tricks",
            column: "DealId");

        migrationBuilder.CreateIndex(
            name: "IX_Tricks_LeadPlayerPositionId",
            table: "Tricks",
            column: "LeadPlayerPositionId");

        migrationBuilder.CreateIndex(
            name: "IX_Tricks_LeadSuitId",
            table: "Tricks",
            column: "LeadSuitId");

        migrationBuilder.CreateIndex(
            name: "IX_Tricks_WinningPlayerPositionId",
            table: "Tricks",
            column: "WinningPlayerPositionId");

        migrationBuilder.CreateIndex(
            name: "IX_Tricks_WinningTeamId",
            table: "Tricks",
            column: "WinningTeamId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "CallTrumpDecisionCardsInHand");

        migrationBuilder.DropTable(
            name: "CallTrumpDecisionPredictedPoints");

        migrationBuilder.DropTable(
            name: "CallTrumpValidDecisions");

        migrationBuilder.DropTable(
            name: "DealDeckCards");

        migrationBuilder.DropTable(
            name: "DealKnownPlayerSuitVoids");

        migrationBuilder.DropTable(
            name: "DealPlayerStartingHandCards");

        migrationBuilder.DropTable(
            name: "DiscardCardDecisionCardsInHand");

        migrationBuilder.DropTable(
            name: "DiscardCardDecisionPredictedPoints");

        migrationBuilder.DropTable(
            name: "GamePlayers");

        migrationBuilder.DropTable(
            name: "PlayCardDecisionCardsAccountedFor");

        migrationBuilder.DropTable(
            name: "PlayCardDecisionCardsInHand");

        migrationBuilder.DropTable(
            name: "PlayCardDecisionKnownVoids");

        migrationBuilder.DropTable(
            name: "PlayCardDecisionPlayedCards");

        migrationBuilder.DropTable(
            name: "PlayCardDecisionPredictedPoints");

        migrationBuilder.DropTable(
            name: "PlayCardDecisionValidCards");

        migrationBuilder.DropTable(
            name: "TrickCardsPlayed");

        migrationBuilder.DropTable(
            name: "CallTrumpDecisions");

        migrationBuilder.DropTable(
            name: "DealPlayers");

        migrationBuilder.DropTable(
            name: "DiscardCardDecisions");

        migrationBuilder.DropTable(
            name: "PlayCardDecisions");

        migrationBuilder.DropTable(
            name: "ActorTypes");

        migrationBuilder.DropTable(
            name: "RelativeCards");

        migrationBuilder.DropTable(
            name: "RelativePlayerPositions");

        migrationBuilder.DropTable(
            name: "Tricks");

        migrationBuilder.DropTable(
            name: "RelativeSuits");

        migrationBuilder.DropTable(
            name: "Deals");

        migrationBuilder.DropTable(
            name: "CallTrumpDecisionValues");

        migrationBuilder.DropTable(
            name: "Cards");

        migrationBuilder.DropTable(
            name: "DealResults");

        migrationBuilder.DropTable(
            name: "DealStatuses");

        migrationBuilder.DropTable(
            name: "Games");

        migrationBuilder.DropTable(
            name: "PlayerPositions");

        migrationBuilder.DropTable(
            name: "Ranks");

        migrationBuilder.DropTable(
            name: "Suits");

        migrationBuilder.DropTable(
            name: "GameStatuses");

        migrationBuilder.DropTable(
            name: "Teams");
    }
}

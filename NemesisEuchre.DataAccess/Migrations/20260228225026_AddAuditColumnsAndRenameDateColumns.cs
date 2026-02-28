using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NemesisEuchre.DataAccess.Migrations;

/// <inheritdoc />
public partial class AddAuditColumnsAndRenameDateColumns : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "LastSeenAt",
            table: "Users",
            newName: "LastSeenDate");

        migrationBuilder.RenameColumn(
            name: "CreatedAt",
            table: "Users",
            newName: "CreateDate");

        migrationBuilder.RenameColumn(
            name: "JoinedAt",
            table: "GameSessionUsers",
            newName: "JoinedDate");

        migrationBuilder.RenameColumn(
            name: "CreatedAt",
            table: "GameSessions",
            newName: "CreateDate");

        migrationBuilder.RenameColumn(
            name: "AllUsersDisconnectedAt",
            table: "GameSessions",
            newName: "AllUsersDisconnectedDate");

        migrationBuilder.RenameColumn(
            name: "DisconnectedAt",
            table: "GameSessionConnections",
            newName: "DisconnectedDate");

        migrationBuilder.RenameColumn(
            name: "ConnectedAt",
            table: "GameSessionConnections",
            newName: "ConnectedDate");

        migrationBuilder.RenameColumn(
            name: "CreatedAt",
            table: "Games",
            newName: "CreateDate");

        migrationBuilder.RenameIndex(
            name: "IX_Games_CreatedAt",
            newName: "IX_Games_CreateDate",
            table: "Games");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "Users",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "Tricks",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "Tricks",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "TrickCardsPlayed",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "TrickCardsPlayed",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "Teams",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "Teams",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "Suits",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "Suits",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "RelativeSuits",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "RelativeSuits",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "RelativePlayerPositions",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "RelativePlayerPositions",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "RelativeCards",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "RelativeCards",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "Ranks",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "Ranks",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "PlayerPositions",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "PlayerPositions",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "PlayCardDecisionValidCards",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "PlayCardDecisionValidCards",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "PlayCardDecisions",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "PlayCardDecisions",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "PlayCardDecisionPredictedPoints",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "PlayCardDecisionPredictedPoints",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "PlayCardDecisionPlayedCards",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "PlayCardDecisionPlayedCards",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "PlayCardDecisionKnownVoids",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "PlayCardDecisionKnownVoids",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "PlayCardDecisionCardsInHand",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "PlayCardDecisionCardsInHand",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "PlayCardDecisionCardsAccountedFor",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "PlayCardDecisionCardsAccountedFor",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "GameStatuses",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "GameStatuses",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "GameSessionUsers",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "GameSessionUsers",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "GameSessions",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "GameSessionConnections",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "GameSessionConnections",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "Games",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "GamePlayers",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "GamePlayers",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "DiscardCardDecisions",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "DiscardCardDecisions",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "DiscardCardDecisionPredictedPoints",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "DiscardCardDecisionPredictedPoints",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "DiscardCardDecisionCardsInHand",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "DiscardCardDecisionCardsInHand",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "DealStatuses",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "DealStatuses",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "Deals",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "Deals",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "DealResults",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "DealResults",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "DealPlayerStartingHandCards",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "DealPlayerStartingHandCards",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "DealPlayers",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "DealPlayers",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "DealKnownPlayerSuitVoids",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "DealKnownPlayerSuitVoids",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "DealDeckCards",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "DealDeckCards",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "Cards",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "Cards",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "CallTrumpValidDecisions",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "CallTrumpValidDecisions",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "CallTrumpDecisionValues",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "CallTrumpDecisionValues",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "CallTrumpDecisions",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "CallTrumpDecisions",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "CallTrumpDecisionPredictedPoints",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "CallTrumpDecisionPredictedPoints",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "CallTrumpDecisionCardsInHand",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "CallTrumpDecisionCardsInHand",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreateDate",
            table: "ActorTypes",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "ModifyDate",
            table: "ActorTypes",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "Tricks");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "Tricks");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "TrickCardsPlayed");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "TrickCardsPlayed");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "Teams");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "Teams");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "Suits");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "Suits");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "RelativeSuits");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "RelativeSuits");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "RelativePlayerPositions");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "RelativePlayerPositions");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "RelativeCards");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "RelativeCards");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "Ranks");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "Ranks");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "PlayerPositions");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "PlayerPositions");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "PlayCardDecisionValidCards");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "PlayCardDecisionValidCards");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "PlayCardDecisions");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "PlayCardDecisions");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "PlayCardDecisionPredictedPoints");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "PlayCardDecisionPredictedPoints");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "PlayCardDecisionPlayedCards");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "PlayCardDecisionPlayedCards");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "PlayCardDecisionKnownVoids");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "PlayCardDecisionKnownVoids");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "PlayCardDecisionCardsInHand");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "PlayCardDecisionCardsInHand");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "PlayCardDecisionCardsAccountedFor");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "PlayCardDecisionCardsAccountedFor");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "GameStatuses");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "GameStatuses");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "GameSessionUsers");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "GameSessionUsers");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "GameSessions");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "GameSessionConnections");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "GameSessionConnections");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "Games");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "GamePlayers");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "GamePlayers");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "DiscardCardDecisions");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "DiscardCardDecisions");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "DiscardCardDecisionPredictedPoints");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "DiscardCardDecisionPredictedPoints");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "DiscardCardDecisionCardsInHand");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "DiscardCardDecisionCardsInHand");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "DealStatuses");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "DealStatuses");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "Deals");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "Deals");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "DealResults");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "DealResults");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "DealPlayerStartingHandCards");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "DealPlayerStartingHandCards");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "DealPlayers");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "DealPlayers");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "DealKnownPlayerSuitVoids");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "DealKnownPlayerSuitVoids");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "DealDeckCards");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "DealDeckCards");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "Cards");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "Cards");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "CallTrumpValidDecisions");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "CallTrumpValidDecisions");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "CallTrumpDecisionValues");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "CallTrumpDecisionValues");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "CallTrumpDecisions");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "CallTrumpDecisions");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "CallTrumpDecisionPredictedPoints");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "CallTrumpDecisionPredictedPoints");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "CallTrumpDecisionCardsInHand");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "CallTrumpDecisionCardsInHand");

        migrationBuilder.DropColumn(
            name: "CreateDate",
            table: "ActorTypes");

        migrationBuilder.DropColumn(
            name: "ModifyDate",
            table: "ActorTypes");

        migrationBuilder.RenameColumn(
            name: "LastSeenDate",
            table: "Users",
            newName: "LastSeenAt");

        migrationBuilder.RenameColumn(
            name: "CreateDate",
            table: "Users",
            newName: "CreatedAt");

        migrationBuilder.RenameColumn(
            name: "JoinedDate",
            table: "GameSessionUsers",
            newName: "JoinedAt");

        migrationBuilder.RenameColumn(
            name: "CreateDate",
            table: "GameSessions",
            newName: "CreatedAt");

        migrationBuilder.RenameColumn(
            name: "AllUsersDisconnectedDate",
            table: "GameSessions",
            newName: "AllUsersDisconnectedAt");

        migrationBuilder.RenameColumn(
            name: "ConnectedDate",
            table: "GameSessionConnections",
            newName: "ConnectedAt");

        migrationBuilder.RenameColumn(
            name: "DisconnectedDate",
            table: "GameSessionConnections",
            newName: "DisconnectedAt");

        migrationBuilder.RenameIndex(
            name: "IX_Games_CreateDate",
            newName: "IX_Games_CreatedAt",
            table: "Games");

        migrationBuilder.RenameColumn(
            name: "CreateDate",
            table: "Games",
            newName: "CreatedAt");
    }
}

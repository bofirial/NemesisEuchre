using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NemesisEuchre.DataAccess.Migrations;

public partial class AddGamePersistenceSupport : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_PlayCardDecisions_Deals_DealId",
            table: "PlayCardDecisions");

        migrationBuilder.DropIndex(
            name: "IX_PlayCardDecisions_ActorType_TrickNumber",
            table: "PlayCardDecisions");

        migrationBuilder.DropColumn(
            name: "DecidingPlayerPosition",
            table: "PlayCardDecisions");

        migrationBuilder.RenameColumn(
            name: "TrickNumber",
            table: "PlayCardDecisions",
            newName: "TrickId");

        migrationBuilder.RenameColumn(
            name: "LeadPosition",
            table: "PlayCardDecisions",
            newName: "LeadPlayer");

        migrationBuilder.RenameColumn(
            name: "HandJson",
            table: "PlayCardDecisions",
            newName: "PlayedCardsJson");

        migrationBuilder.RenameColumn(
            name: "CurrentTrickJson",
            table: "PlayCardDecisions",
            newName: "CardsInHandJson");

        migrationBuilder.RenameColumn(
            name: "HandJson",
            table: "DiscardCardDecisions",
            newName: "CardsInHandJson");

        migrationBuilder.RenameColumn(
            name: "DealerPosition",
            table: "DiscardCardDecisions",
            newName: "CallingPlayer");

        migrationBuilder.RenameColumn(
            name: "HandJson",
            table: "CallTrumpDecisions",
            newName: "CardsInHandJson");

        migrationBuilder.AddColumn<int>(
            name: "LeadSuit",
            table: "PlayCardDecisions",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "WinningTrickPlayer",
            table: "PlayCardDecisions",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "CallingPlayerGoingAlone",
            table: "DiscardCardDecisions",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateIndex(
            name: "IX_PlayCardDecisions_ActorType",
            table: "PlayCardDecisions",
            column: "ActorType");

        migrationBuilder.CreateIndex(
            name: "IX_PlayCardDecisions_TrickId",
            table: "PlayCardDecisions",
            column: "TrickId");

        migrationBuilder.AddForeignKey(
            name: "FK_PlayCardDecisions_Deals_DealId",
            table: "PlayCardDecisions",
            column: "DealId",
            principalTable: "Deals",
            principalColumn: "DealId");

        migrationBuilder.AddForeignKey(
            name: "FK_PlayCardDecisions_Tricks_TrickId",
            table: "PlayCardDecisions",
            column: "TrickId",
            principalTable: "Tricks",
            principalColumn: "TrickId",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_PlayCardDecisions_Deals_DealId",
            table: "PlayCardDecisions");

        migrationBuilder.DropForeignKey(
            name: "FK_PlayCardDecisions_Tricks_TrickId",
            table: "PlayCardDecisions");

        migrationBuilder.DropIndex(
            name: "IX_PlayCardDecisions_ActorType",
            table: "PlayCardDecisions");

        migrationBuilder.DropIndex(
            name: "IX_PlayCardDecisions_TrickId",
            table: "PlayCardDecisions");

        migrationBuilder.DropColumn(
            name: "LeadSuit",
            table: "PlayCardDecisions");

        migrationBuilder.DropColumn(
            name: "WinningTrickPlayer",
            table: "PlayCardDecisions");

        migrationBuilder.DropColumn(
            name: "CallingPlayerGoingAlone",
            table: "DiscardCardDecisions");

        migrationBuilder.RenameColumn(
            name: "TrickId",
            table: "PlayCardDecisions",
            newName: "TrickNumber");

        migrationBuilder.RenameColumn(
            name: "PlayedCardsJson",
            table: "PlayCardDecisions",
            newName: "HandJson");

        migrationBuilder.RenameColumn(
            name: "LeadPlayer",
            table: "PlayCardDecisions",
            newName: "LeadPosition");

        migrationBuilder.RenameColumn(
            name: "CardsInHandJson",
            table: "PlayCardDecisions",
            newName: "CurrentTrickJson");

        migrationBuilder.RenameColumn(
            name: "CardsInHandJson",
            table: "DiscardCardDecisions",
            newName: "HandJson");

        migrationBuilder.RenameColumn(
            name: "CallingPlayer",
            table: "DiscardCardDecisions",
            newName: "DealerPosition");

        migrationBuilder.RenameColumn(
            name: "CardsInHandJson",
            table: "CallTrumpDecisions",
            newName: "HandJson");

        migrationBuilder.AddColumn<int>(
            name: "DecidingPlayerPosition",
            table: "PlayCardDecisions",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateIndex(
            name: "IX_PlayCardDecisions_ActorType_TrickNumber",
            table: "PlayCardDecisions",
            columns: ["ActorType", "TrickNumber"]);

        migrationBuilder.AddForeignKey(
            name: "FK_PlayCardDecisions_Deals_DealId",
            table: "PlayCardDecisions",
            column: "DealId",
            principalTable: "Deals",
            principalColumn: "DealId",
            onDelete: ReferentialAction.Cascade);
    }
}

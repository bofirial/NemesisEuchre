using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NemesisEuchre.DataAccess.Migrations;

/// <inheritdoc />
public partial class RefineJsonFieldStorageLimits : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "CardsPlayedJson",
            table: "Tricks",
            type: "nvarchar(400)",
            maxLength: 400,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(1000)",
            oldMaxLength: 1000);

        migrationBuilder.AlterColumn<string>(
            name: "ValidCardsToPlayJson",
            table: "PlayCardDecisions",
            type: "nvarchar(250)",
            maxLength: 250,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<string>(
            name: "PlayedCardsJson",
            table: "PlayCardDecisions",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<string>(
            name: "KnownPlayerSuitVoidsJson",
            table: "PlayCardDecisions",
            type: "nvarchar(1000)",
            maxLength: 1000,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(1500)",
            oldMaxLength: 1500);

        migrationBuilder.AlterColumn<string>(
            name: "DealerPickedUpCardJson",
            table: "PlayCardDecisions",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(200)",
            oldMaxLength: 200,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "ChosenCardJson",
            table: "PlayCardDecisions",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(200)",
            oldMaxLength: 200);

        migrationBuilder.AlterColumn<string>(
            name: "CardsInHandJson",
            table: "PlayCardDecisions",
            type: "nvarchar(250)",
            maxLength: 250,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<string>(
            name: "CardsAccountedForJson",
            table: "PlayCardDecisions",
            type: "nvarchar(1000)",
            maxLength: 1000,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<string>(
            name: "PlayersJson",
            table: "Games",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(4000)",
            oldMaxLength: 4000);

        migrationBuilder.AlterColumn<string>(
            name: "ChosenCardJson",
            table: "DiscardCardDecisions",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(200)",
            oldMaxLength: 200);

        migrationBuilder.AlterColumn<string>(
            name: "CardsInHandJson",
            table: "DiscardCardDecisions",
            type: "nvarchar(300)",
            maxLength: 300,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<string>(
            name: "UpCardJson",
            table: "Deals",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(200)",
            oldMaxLength: 200,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "PlayersJson",
            table: "Deals",
            type: "nvarchar(1500)",
            maxLength: 1500,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<string>(
            name: "KnownPlayerSuitVoidsJson",
            table: "Deals",
            type: "nvarchar(600)",
            maxLength: 600,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(1000)",
            oldMaxLength: 1000,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "DiscardedCardJson",
            table: "Deals",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(200)",
            oldMaxLength: 200,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "DeckJson",
            table: "Deals",
            type: "nvarchar(150)",
            maxLength: 150,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<string>(
            name: "ValidDecisionsJson",
            table: "CallTrumpDecisions",
            type: "nvarchar(150)",
            maxLength: 150,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<string>(
            name: "UpCardJson",
            table: "CallTrumpDecisions",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(200)",
            oldMaxLength: 200);

        migrationBuilder.AlterColumn<string>(
            name: "CardsInHandJson",
            table: "CallTrumpDecisions",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "CardsPlayedJson",
            table: "Tricks",
            type: "nvarchar(1000)",
            maxLength: 1000,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(400)",
            oldMaxLength: 400);

        migrationBuilder.AlterColumn<string>(
            name: "ValidCardsToPlayJson",
            table: "PlayCardDecisions",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(250)",
            oldMaxLength: 250);

        migrationBuilder.AlterColumn<string>(
            name: "PlayedCardsJson",
            table: "PlayCardDecisions",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(200)",
            oldMaxLength: 200);

        migrationBuilder.AlterColumn<string>(
            name: "KnownPlayerSuitVoidsJson",
            table: "PlayCardDecisions",
            type: "nvarchar(1500)",
            maxLength: 1500,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(1000)",
            oldMaxLength: 1000);

        migrationBuilder.AlterColumn<string>(
            name: "DealerPickedUpCardJson",
            table: "PlayCardDecisions",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(50)",
            oldMaxLength: 50,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "ChosenCardJson",
            table: "PlayCardDecisions",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(50)",
            oldMaxLength: 50);

        migrationBuilder.AlterColumn<string>(
            name: "CardsInHandJson",
            table: "PlayCardDecisions",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(250)",
            oldMaxLength: 250);

        migrationBuilder.AlterColumn<string>(
            name: "CardsAccountedForJson",
            table: "PlayCardDecisions",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(1000)",
            oldMaxLength: 1000);

        migrationBuilder.AlterColumn<string>(
            name: "PlayersJson",
            table: "Games",
            type: "nvarchar(4000)",
            maxLength: 4000,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(500)",
            oldMaxLength: 500);

        migrationBuilder.AlterColumn<string>(
            name: "ChosenCardJson",
            table: "DiscardCardDecisions",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(50)",
            oldMaxLength: 50);

        migrationBuilder.AlterColumn<string>(
            name: "CardsInHandJson",
            table: "DiscardCardDecisions",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(300)",
            oldMaxLength: 300);

        migrationBuilder.AlterColumn<string>(
            name: "UpCardJson",
            table: "Deals",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(50)",
            oldMaxLength: 50,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "PlayersJson",
            table: "Deals",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(1500)",
            oldMaxLength: 1500);

        migrationBuilder.AlterColumn<string>(
            name: "KnownPlayerSuitVoidsJson",
            table: "Deals",
            type: "nvarchar(1000)",
            maxLength: 1000,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(600)",
            oldMaxLength: 600,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "DiscardedCardJson",
            table: "Deals",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(50)",
            oldMaxLength: 50,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "DeckJson",
            table: "Deals",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(150)",
            oldMaxLength: 150);

        migrationBuilder.AlterColumn<string>(
            name: "ValidDecisionsJson",
            table: "CallTrumpDecisions",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(150)",
            oldMaxLength: 150);

        migrationBuilder.AlterColumn<string>(
            name: "UpCardJson",
            table: "CallTrumpDecisions",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(50)",
            oldMaxLength: 50);

        migrationBuilder.AlterColumn<string>(
            name: "CardsInHandJson",
            table: "CallTrumpDecisions",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(200)",
            oldMaxLength: 200);
    }
}

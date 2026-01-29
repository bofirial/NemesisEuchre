using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NemesisEuchre.DataAccess.Migrations;

/// <inheritdoc />
public partial class ConvertEnumsToStringStorage : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "WinningTeam",
            table: "Tricks",
            type: "nvarchar(10)",
            maxLength: 10,
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "WinningPosition",
            table: "Tricks",
            type: "nvarchar(10)",
            maxLength: 10,
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "LeadSuit",
            table: "Tricks",
            type: "nvarchar(10)",
            maxLength: 10,
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "LeadPosition",
            table: "Tricks",
            type: "nvarchar(10)",
            maxLength: 10,
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int");

        migrationBuilder.AlterColumn<string>(
            name: "WinningTrickPlayer",
            table: "PlayCardDecisions",
            type: "nvarchar(25)",
            maxLength: 25,
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "LeadSuit",
            table: "PlayCardDecisions",
            type: "nvarchar(30)",
            maxLength: 30,
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "LeadPlayer",
            table: "PlayCardDecisions",
            type: "nvarchar(25)",
            maxLength: 25,
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int");

        migrationBuilder.AlterColumn<string>(
            name: "ActorType",
            table: "PlayCardDecisions",
            type: "nvarchar(10)",
            maxLength: 10,
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "WinningTeam",
            table: "Games",
            type: "nvarchar(10)",
            maxLength: 10,
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "GameStatus",
            table: "Games",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int");

        migrationBuilder.AlterColumn<string>(
            name: "CallingPlayer",
            table: "DiscardCardDecisions",
            type: "nvarchar(25)",
            maxLength: 25,
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int");

        migrationBuilder.AlterColumn<string>(
            name: "ActorType",
            table: "DiscardCardDecisions",
            type: "nvarchar(10)",
            maxLength: 10,
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "WinningTeam",
            table: "Deals",
            type: "nvarchar(10)",
            maxLength: 10,
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Trump",
            table: "Deals",
            type: "nvarchar(10)",
            maxLength: 10,
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "DealerPosition",
            table: "Deals",
            type: "nvarchar(10)",
            maxLength: 10,
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "DealStatus",
            table: "Deals",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int");

        migrationBuilder.AlterColumn<string>(
            name: "DealResult",
            table: "Deals",
            type: "nvarchar(25)",
            maxLength: 25,
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "CallingPlayer",
            table: "Deals",
            type: "nvarchar(10)",
            maxLength: 10,
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "DecidingPlayerPosition",
            table: "CallTrumpDecisions",
            type: "nvarchar(10)",
            maxLength: 10,
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int");

        migrationBuilder.AlterColumn<string>(
            name: "DealerPosition",
            table: "CallTrumpDecisions",
            type: "nvarchar(10)",
            maxLength: 10,
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int");

        migrationBuilder.AlterColumn<string>(
            name: "ActorType",
            table: "CallTrumpDecisions",
            type: "nvarchar(10)",
            maxLength: 10,
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<int>(
            name: "WinningTeam",
            table: "Tricks",
            type: "int",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(10)",
            oldMaxLength: 10,
            oldNullable: true);

        migrationBuilder.AlterColumn<int>(
            name: "WinningPosition",
            table: "Tricks",
            type: "int",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(10)",
            oldMaxLength: 10,
            oldNullable: true);

        migrationBuilder.AlterColumn<int>(
            name: "LeadSuit",
            table: "Tricks",
            type: "int",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(10)",
            oldMaxLength: 10,
            oldNullable: true);

        migrationBuilder.AlterColumn<int>(
            name: "LeadPosition",
            table: "Tricks",
            type: "int",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(10)",
            oldMaxLength: 10);

        migrationBuilder.AlterColumn<int>(
            name: "WinningTrickPlayer",
            table: "PlayCardDecisions",
            type: "int",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(25)",
            oldMaxLength: 25,
            oldNullable: true);

        migrationBuilder.AlterColumn<int>(
            name: "LeadSuit",
            table: "PlayCardDecisions",
            type: "int",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(30)",
            oldMaxLength: 30,
            oldNullable: true);

        migrationBuilder.AlterColumn<int>(
            name: "LeadPlayer",
            table: "PlayCardDecisions",
            type: "int",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(25)",
            oldMaxLength: 25);

        migrationBuilder.AlterColumn<int>(
            name: "ActorType",
            table: "PlayCardDecisions",
            type: "int",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(10)",
            oldMaxLength: 10,
            oldNullable: true);

        migrationBuilder.AlterColumn<int>(
            name: "WinningTeam",
            table: "Games",
            type: "int",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(10)",
            oldMaxLength: 10,
            oldNullable: true);

        migrationBuilder.AlterColumn<int>(
            name: "GameStatus",
            table: "Games",
            type: "int",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(20)",
            oldMaxLength: 20);

        migrationBuilder.AlterColumn<int>(
            name: "CallingPlayer",
            table: "DiscardCardDecisions",
            type: "int",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(25)",
            oldMaxLength: 25);

        migrationBuilder.AlterColumn<int>(
            name: "ActorType",
            table: "DiscardCardDecisions",
            type: "int",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(10)",
            oldMaxLength: 10,
            oldNullable: true);

        migrationBuilder.AlterColumn<int>(
            name: "WinningTeam",
            table: "Deals",
            type: "int",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(10)",
            oldMaxLength: 10,
            oldNullable: true);

        migrationBuilder.AlterColumn<int>(
            name: "Trump",
            table: "Deals",
            type: "int",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(10)",
            oldMaxLength: 10,
            oldNullable: true);

        migrationBuilder.AlterColumn<int>(
            name: "DealerPosition",
            table: "Deals",
            type: "int",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(10)",
            oldMaxLength: 10,
            oldNullable: true);

        migrationBuilder.AlterColumn<int>(
            name: "DealStatus",
            table: "Deals",
            type: "int",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(20)",
            oldMaxLength: 20);

        migrationBuilder.AlterColumn<int>(
            name: "DealResult",
            table: "Deals",
            type: "int",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(25)",
            oldMaxLength: 25,
            oldNullable: true);

        migrationBuilder.AlterColumn<int>(
            name: "CallingPlayer",
            table: "Deals",
            type: "int",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(10)",
            oldMaxLength: 10,
            oldNullable: true);

        migrationBuilder.AlterColumn<int>(
            name: "DecidingPlayerPosition",
            table: "CallTrumpDecisions",
            type: "int",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(10)",
            oldMaxLength: 10);

        migrationBuilder.AlterColumn<int>(
            name: "DealerPosition",
            table: "CallTrumpDecisions",
            type: "int",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(10)",
            oldMaxLength: 10);

        migrationBuilder.AlterColumn<int>(
            name: "ActorType",
            table: "CallTrumpDecisions",
            type: "int",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(10)",
            oldMaxLength: 10,
            oldNullable: true);
    }
}

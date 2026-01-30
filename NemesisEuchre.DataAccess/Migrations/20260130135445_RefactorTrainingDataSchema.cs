using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NemesisEuchre.DataAccess.Migrations;

/// <inheritdoc />
public partial class RefactorTrainingDataSchema : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ValidCardsToDiscardJson",
            table: "DiscardCardDecisions");

        migrationBuilder.DropColumn(
            name: "DecidingPlayerPosition",
            table: "CallTrumpDecisions");

        migrationBuilder.AlterColumn<string>(
            name: "DealerPosition",
            table: "CallTrumpDecisions",
            type: "nvarchar(25)",
            maxLength: 25,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(10)",
            oldMaxLength: 10);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ValidCardsToDiscardJson",
            table: "DiscardCardDecisions",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AlterColumn<string>(
            name: "DealerPosition",
            table: "CallTrumpDecisions",
            type: "nvarchar(10)",
            maxLength: 10,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(25)",
            oldMaxLength: 25);

        migrationBuilder.AddColumn<string>(
            name: "DecidingPlayerPosition",
            table: "CallTrumpDecisions",
            type: "nvarchar(10)",
            maxLength: 10,
            nullable: false,
            defaultValue: string.Empty);
    }
}

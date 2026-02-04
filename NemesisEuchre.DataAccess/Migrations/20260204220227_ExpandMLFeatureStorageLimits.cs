using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NemesisEuchre.DataAccess.Migrations;

/// <inheritdoc />
public partial class ExpandMLFeatureStorageLimits : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "KnownPlayerSuitVoidsJson",
            table: "PlayCardDecisions",
            type: "nvarchar(1500)",
            maxLength: 1500,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(500)",
            oldMaxLength: 500);

        migrationBuilder.AlterColumn<string>(
            name: "CardsAccountedForJson",
            table: "PlayCardDecisions",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(1000)",
            oldMaxLength: 1000);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "KnownPlayerSuitVoidsJson",
            table: "PlayCardDecisions",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(1500)",
            oldMaxLength: 1500);

        migrationBuilder.AlterColumn<string>(
            name: "CardsAccountedForJson",
            table: "PlayCardDecisions",
            type: "nvarchar(1000)",
            maxLength: 1000,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");
    }
}

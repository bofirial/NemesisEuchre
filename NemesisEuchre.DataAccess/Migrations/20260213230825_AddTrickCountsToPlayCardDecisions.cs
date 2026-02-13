using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NemesisEuchre.DataAccess.Migrations;

/// <inheritdoc />
public partial class AddTrickCountsToPlayCardDecisions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<short>(
            name: "OpponentsWonTricks",
            table: "PlayCardDecisions",
            type: "smallint",
            nullable: false,
            defaultValue: (short)0);

        migrationBuilder.AddColumn<short>(
            name: "WonTricks",
            table: "PlayCardDecisions",
            type: "smallint",
            nullable: false,
            defaultValue: (short)0);

        migrationBuilder.AlterColumn<int>(
            name: "ActorTypeId",
            table: "GamePlayers",
            type: "int",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);

        migrationBuilder.AlterColumn<int>(
            name: "ActorTypeId",
            table: "DealPlayers",
            type: "int",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "OpponentsWonTricks",
            table: "PlayCardDecisions");

        migrationBuilder.DropColumn(
            name: "WonTricks",
            table: "PlayCardDecisions");

        migrationBuilder.AlterColumn<int>(
            name: "ActorTypeId",
            table: "GamePlayers",
            type: "int",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int");

        migrationBuilder.AlterColumn<int>(
            name: "ActorTypeId",
            table: "DealPlayers",
            type: "int",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int");
    }
}

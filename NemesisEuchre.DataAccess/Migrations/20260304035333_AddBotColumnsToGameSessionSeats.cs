using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NemesisEuchre.DataAccess.Migrations;

/// <inheritdoc />
public partial class AddBotColumnsToGameSessionSeats : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "BotActorType",
            table: "GameSessionSeats",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "BotModelName",
            table: "GameSessionSeats",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "BotActorType",
            table: "GameSessionSeats");

        migrationBuilder.DropColumn(
            name: "BotModelName",
            table: "GameSessionSeats");
    }
}

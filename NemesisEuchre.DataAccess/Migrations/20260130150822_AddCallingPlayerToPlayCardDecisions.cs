using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NemesisEuchre.DataAccess.Migrations;

/// <inheritdoc />
public partial class AddCallingPlayerToPlayCardDecisions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "CallingPlayer",
            table: "PlayCardDecisions",
            type: "nvarchar(25)",
            maxLength: 25,
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AddColumn<bool>(
            name: "CallingPlayerGoingAlone",
            table: "PlayCardDecisions",
            type: "bit",
            nullable: false,
            defaultValue: false);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "CallingPlayer",
            table: "PlayCardDecisions");

        migrationBuilder.DropColumn(
            name: "CallingPlayerGoingAlone",
            table: "PlayCardDecisions");
    }
}

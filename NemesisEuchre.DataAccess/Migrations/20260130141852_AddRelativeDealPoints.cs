using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NemesisEuchre.DataAccess.Migrations;

/// <inheritdoc />
public partial class AddRelativeDealPoints : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<short>(
            name: "RelativeDealPoints",
            table: "PlayCardDecisions",
            type: "smallint",
            nullable: true);

        migrationBuilder.AddColumn<short>(
            name: "RelativeDealPoints",
            table: "DiscardCardDecisions",
            type: "smallint",
            nullable: true);

        migrationBuilder.AddColumn<short>(
            name: "RelativeDealPoints",
            table: "CallTrumpDecisions",
            type: "smallint",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "RelativeDealPoints",
            table: "PlayCardDecisions");

        migrationBuilder.DropColumn(
            name: "RelativeDealPoints",
            table: "DiscardCardDecisions");

        migrationBuilder.DropColumn(
            name: "RelativeDealPoints",
            table: "CallTrumpDecisions");
    }
}

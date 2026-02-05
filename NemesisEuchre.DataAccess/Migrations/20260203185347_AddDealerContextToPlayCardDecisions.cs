using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NemesisEuchre.DataAccess.Migrations;

/// <inheritdoc />
public partial class AddDealerContextToPlayCardDecisions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "DealerPickedUpCardJson",
            table: "PlayCardDecisions",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DealerPosition",
            table: "PlayCardDecisions",
            type: "nvarchar(25)",
            maxLength: 25,
            nullable: false,
            defaultValue: "Self");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "DealerPickedUpCardJson",
            table: "PlayCardDecisions");

        migrationBuilder.DropColumn(
            name: "DealerPosition",
            table: "PlayCardDecisions");
    }
}

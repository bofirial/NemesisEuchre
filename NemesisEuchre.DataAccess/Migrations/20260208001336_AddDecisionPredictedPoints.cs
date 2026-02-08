using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NemesisEuchre.DataAccess.Migrations;

/// <inheritdoc />
public partial class AddDecisionPredictedPoints : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "DecisionPredictedPointsJson",
            table: "PlayCardDecisions",
            type: "nvarchar(350)",
            maxLength: 350,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DecisionPredictedPointsJson",
            table: "DiscardCardDecisions",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DecisionPredictedPointsJson",
            table: "CallTrumpDecisions",
            type: "nvarchar(400)",
            maxLength: 400,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "DecisionPredictedPointsJson",
            table: "PlayCardDecisions");

        migrationBuilder.DropColumn(
            name: "DecisionPredictedPointsJson",
            table: "DiscardCardDecisions");

        migrationBuilder.DropColumn(
            name: "DecisionPredictedPointsJson",
            table: "CallTrumpDecisions");
    }
}

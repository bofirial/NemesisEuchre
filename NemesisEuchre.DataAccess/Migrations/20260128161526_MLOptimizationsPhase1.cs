using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NemesisEuchre.DataAccess.Migrations;

/// <inheritdoc />
public partial class MLOptimizationsPhase1 : Migration
{
    private static readonly string[] Value = ["DidTeamWinDeal", "DidTeamWinGame"];
    private static readonly string[] TrickOutcomes = ["DidTeamWinTrick", "DidTeamWinDeal", "DidTeamWinGame"];
    private static readonly string[] DealOutcomes = ["DealStatus", "WinningTeam"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "ActorType",
            table: "PlayCardDecisions",
            type: "nvarchar(25)",
            maxLength: 25,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(10)",
            oldMaxLength: 10,
            oldNullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ChosenCardRank",
            table: "PlayCardDecisions",
            type: "nvarchar(10)",
            maxLength: 10,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ChosenCardSuit",
            table: "PlayCardDecisions",
            type: "nvarchar(10)",
            maxLength: 10,
            nullable: true);

        migrationBuilder.AddColumn<float>(
            name: "DecisionConfidence",
            table: "PlayCardDecisions",
            type: "real",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DatasetSplit",
            table: "Games",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            defaultValue: "Train");

        migrationBuilder.AlterColumn<string>(
            name: "ActorType",
            table: "DiscardCardDecisions",
            type: "nvarchar(25)",
            maxLength: 25,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(10)",
            oldMaxLength: 10,
            oldNullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ChosenCardRank",
            table: "DiscardCardDecisions",
            type: "nvarchar(10)",
            maxLength: 10,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ChosenCardSuit",
            table: "DiscardCardDecisions",
            type: "nvarchar(10)",
            maxLength: 10,
            nullable: true);

        migrationBuilder.AddColumn<float>(
            name: "DecisionConfidence",
            table: "DiscardCardDecisions",
            type: "real",
            nullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "ActorType",
            table: "CallTrumpDecisions",
            type: "nvarchar(25)",
            maxLength: 25,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(10)",
            oldMaxLength: 10,
            oldNullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ChosenDecisionType",
            table: "CallTrumpDecisions",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ChosenTrumpSuit",
            table: "CallTrumpDecisions",
            type: "nvarchar(10)",
            maxLength: 10,
            nullable: true);

        migrationBuilder.AddColumn<float>(
            name: "DecisionConfidence",
            table: "CallTrumpDecisions",
            type: "real",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "TrainingBatches",
            columns: table => new
            {
                TrainingBatchId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                GenerationNumber = table.Column<int>(type: "int", nullable: false),
                ModelVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ActorType = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                GameIdStart = table.Column<int>(type: "int", nullable: true),
                GameIdEnd = table.Column<int>(type: "int", nullable: true),
                TotalDecisions = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TrainingBatches", x => x.TrainingBatchId);
                table.ForeignKey(
                    name: "FK_TrainingBatches_Games_GameIdEnd",
                    column: x => x.GameIdEnd,
                    principalTable: "Games",
                    principalColumn: "GameId");
                table.ForeignKey(
                    name: "FK_TrainingBatches_Games_GameIdStart",
                    column: x => x.GameIdStart,
                    principalTable: "Games",
                    principalColumn: "GameId");
            });

        migrationBuilder.CreateIndex(
            name: "IX_TrainingBatches_ActorType",
            table: "TrainingBatches",
            column: "ActorType");

        migrationBuilder.CreateIndex(
            name: "IX_TrainingBatches_CreatedAt",
            table: "TrainingBatches",
            column: "CreatedAt");

        migrationBuilder.CreateIndex(
            name: "IX_TrainingBatches_GameIdEnd",
            table: "TrainingBatches",
            column: "GameIdEnd");

        migrationBuilder.CreateIndex(
            name: "IX_TrainingBatches_GameIdStart",
            table: "TrainingBatches",
            column: "GameIdStart");

        migrationBuilder.CreateIndex(
            name: "IX_TrainingBatches_GenerationNumber",
            table: "TrainingBatches",
            column: "GenerationNumber");

        // ML-optimized composite indexes for batch queries
        migrationBuilder.CreateIndex(
            name: "IX_CallTrumpDecisions_ActorType_DealId_Outcomes",
            table: "CallTrumpDecisions",
            columns: ["ActorType", "DealId"])
            .Annotation("SqlServer:Include", Value);

        migrationBuilder.CreateIndex(
            name: "IX_DiscardCardDecisions_ActorType_DealId_Outcomes",
            table: "DiscardCardDecisions",
            columns: ["ActorType", "DealId"])
            .Annotation("SqlServer:Include", Value);

        migrationBuilder.CreateIndex(
            name: "IX_PlayCardDecisions_ActorType_TrickId_Outcomes",
            table: "PlayCardDecisions",
            columns: ["ActorType", "TrickId"])
            .Annotation("SqlServer:Include", TrickOutcomes);

        // Time-based filtering for training data batches
        migrationBuilder.CreateIndex(
            name: "IX_Games_CreatedAt_WinningTeam",
            table: "Games",
            columns: ["CreatedAt", "WinningTeam"]);

        // Dataset split filtering
        migrationBuilder.CreateIndex(
            name: "IX_Games_DatasetSplit_CreatedAt",
            table: "Games",
            columns: ["DatasetSplit", "CreatedAt"]);

        // Deal lookup optimization
        migrationBuilder.CreateIndex(
            name: "IX_Deals_GameId_DealNumber",
            table: "Deals",
            columns: ["GameId", "DealNumber"])
            .Annotation("SqlServer:Include", DealOutcomes);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Drop ML-optimized indexes
        migrationBuilder.DropIndex(
            name: "IX_CallTrumpDecisions_ActorType_DealId_Outcomes",
            table: "CallTrumpDecisions");

        migrationBuilder.DropIndex(
            name: "IX_DiscardCardDecisions_ActorType_DealId_Outcomes",
            table: "DiscardCardDecisions");

        migrationBuilder.DropIndex(
            name: "IX_PlayCardDecisions_ActorType_TrickId_Outcomes",
            table: "PlayCardDecisions");

        migrationBuilder.DropIndex(
            name: "IX_Games_CreatedAt_WinningTeam",
            table: "Games");

        migrationBuilder.DropIndex(
            name: "IX_Games_DatasetSplit_CreatedAt",
            table: "Games");

        migrationBuilder.DropIndex(
            name: "IX_Deals_GameId_DealNumber",
            table: "Deals");

        migrationBuilder.DropTable(
            name: "TrainingBatches");

        migrationBuilder.DropColumn(
            name: "ChosenCardRank",
            table: "PlayCardDecisions");

        migrationBuilder.DropColumn(
            name: "ChosenCardSuit",
            table: "PlayCardDecisions");

        migrationBuilder.DropColumn(
            name: "DecisionConfidence",
            table: "PlayCardDecisions");

        migrationBuilder.DropColumn(
            name: "DatasetSplit",
            table: "Games");

        migrationBuilder.DropColumn(
            name: "ChosenCardRank",
            table: "DiscardCardDecisions");

        migrationBuilder.DropColumn(
            name: "ChosenCardSuit",
            table: "DiscardCardDecisions");

        migrationBuilder.DropColumn(
            name: "DecisionConfidence",
            table: "DiscardCardDecisions");

        migrationBuilder.DropColumn(
            name: "ChosenDecisionType",
            table: "CallTrumpDecisions");

        migrationBuilder.DropColumn(
            name: "ChosenTrumpSuit",
            table: "CallTrumpDecisions");

        migrationBuilder.DropColumn(
            name: "DecisionConfidence",
            table: "CallTrumpDecisions");

        migrationBuilder.AlterColumn<string>(
            name: "ActorType",
            table: "PlayCardDecisions",
            type: "nvarchar(10)",
            maxLength: 10,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(25)",
            oldMaxLength: 25,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "ActorType",
            table: "DiscardCardDecisions",
            type: "nvarchar(10)",
            maxLength: 10,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(25)",
            oldMaxLength: 25,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "ActorType",
            table: "CallTrumpDecisions",
            type: "nvarchar(10)",
            maxLength: 10,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(25)",
            oldMaxLength: 25,
            oldNullable: true);
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NemesisEuchre.DataAccess.Migrations;

/// <inheritdoc />
public partial class SplitSelectingTrumpDealStatus : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.UpdateData(
            table: "DealStatuses",
            keyColumn: "DealStatusId",
            keyValue: 1,
            column: "Name",
            value: "SelectingTrumpPhase1");

        migrationBuilder.InsertData(
            table: "DealStatuses",
            columns: ["DealStatusId", "Name"],
            values: [5, "SelectingTrumpPhase2"]);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            table: "DealStatuses",
            keyColumn: "DealStatusId",
            keyValue: 5);

        migrationBuilder.UpdateData(
            table: "DealStatuses",
            keyColumn: "DealStatusId",
            keyValue: 1,
            column: "Name",
            value: "SelectingTrump");
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NemesisEuchre.DataAccess.Migrations;

/// <inheritdoc />
public partial class AddGameSessionSeats : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "GameSessionSeats",
            columns: table => new
            {
                GameSessionId = table.Column<int>(type: "int", nullable: false),
                Position = table.Column<int>(type: "int", nullable: false),
                UserId = table.Column<int>(type: "int", nullable: true),
                CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                ModifyDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GameSessionSeats", x => new { x.GameSessionId, x.Position });
                table.ForeignKey(
                    name: "FK_GameSessionSeats_GameSessions_GameSessionId",
                    column: x => x.GameSessionId,
                    principalTable: "GameSessions",
                    principalColumn: "GameSessionId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_GameSessionSeats_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "UserId",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(
            name: "IX_GameSessionSeats_UserId",
            table: "GameSessionSeats",
            column: "UserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "GameSessionSeats");
    }
}

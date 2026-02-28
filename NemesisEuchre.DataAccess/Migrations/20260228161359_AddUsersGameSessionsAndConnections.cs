using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NemesisEuchre.DataAccess.Migrations;

/// <inheritdoc />
public partial class AddUsersGameSessionsAndConnections : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "GameSessionId",
            table: "Games",
            type: "int",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "GameSessions",
            columns: table => new
            {
                GameSessionId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SessionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                AllUsersDisconnectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
            },
            constraints: table => table.PrimaryKey("PK_GameSessions", x => x.GameSessionId));

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                UserId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                GitHubId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                GitHubLogin = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                LastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
            },
            constraints: table => table.PrimaryKey("PK_Users", x => x.UserId));

        migrationBuilder.CreateTable(
            name: "GameSessionConnections",
            columns: table => new
            {
                ConnectionId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                GameSessionId = table.Column<int>(type: "int", nullable: false),
                UserId = table.Column<int>(type: "int", nullable: false),
                ConnectedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                DisconnectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GameSessionConnections", x => x.ConnectionId);
                table.ForeignKey(
                    name: "FK_GameSessionConnections_GameSessions_GameSessionId",
                    column: x => x.GameSessionId,
                    principalTable: "GameSessions",
                    principalColumn: "GameSessionId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_GameSessionConnections_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "UserId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "GameSessionUsers",
            columns: table => new
            {
                GameSessionId = table.Column<int>(type: "int", nullable: false),
                UserId = table.Column<int>(type: "int", nullable: false),
                JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                IsSessionLeader = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GameSessionUsers", x => new { x.GameSessionId, x.UserId });
                table.ForeignKey(
                    name: "FK_GameSessionUsers_GameSessions_GameSessionId",
                    column: x => x.GameSessionId,
                    principalTable: "GameSessions",
                    principalColumn: "GameSessionId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_GameSessionUsers_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "UserId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Games_GameSessionId",
            table: "Games",
            column: "GameSessionId");

        migrationBuilder.CreateIndex(
            name: "IX_GameSessionConnections_GameSessionId",
            table: "GameSessionConnections",
            column: "GameSessionId");

        migrationBuilder.CreateIndex(
            name: "IX_GameSessionConnections_UserId",
            table: "GameSessionConnections",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_GameSessions_SessionName",
            table: "GameSessions",
            column: "SessionName");

        migrationBuilder.CreateIndex(
            name: "IX_GameSessionUsers_UserId",
            table: "GameSessionUsers",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Users_GitHubId",
            table: "Users",
            column: "GitHubId",
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_Games_GameSessions_GameSessionId",
            table: "Games",
            column: "GameSessionId",
            principalTable: "GameSessions",
            principalColumn: "GameSessionId",
            onDelete: ReferentialAction.SetNull);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Games_GameSessions_GameSessionId",
            table: "Games");

        migrationBuilder.DropTable(
            name: "GameSessionConnections");

        migrationBuilder.DropTable(
            name: "GameSessionUsers");

        migrationBuilder.DropTable(
            name: "GameSessions");

        migrationBuilder.DropTable(
            name: "Users");

        migrationBuilder.DropIndex(
            name: "IX_Games_GameSessionId",
            table: "Games");

        migrationBuilder.DropColumn(
            name: "GameSessionId",
            table: "Games");
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NemesisEuchre.DataAccess.Entities;

public class GameSessionEntity
{
    public int GameSessionId { get; set; }

    public required string SessionName { get; set; }

    public DateTime? AllUsersDisconnectedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<GameSessionUserEntity> GameSessionUsers { get; set; } = [];

    public ICollection<GameEntity> Games { get; set; } = [];
}

public class GameSessionEntityConfiguration : IEntityTypeConfiguration<GameSessionEntity>
{
    public void Configure(EntityTypeBuilder<GameSessionEntity> builder)
    {
        builder.ToTable("GameSessions");

        builder.HasKey(e => e.GameSessionId);

        builder.Property(e => e.GameSessionId)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.SessionName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(e => e.SessionName)
            .HasDatabaseName("IX_GameSessions_SessionName");
    }
}

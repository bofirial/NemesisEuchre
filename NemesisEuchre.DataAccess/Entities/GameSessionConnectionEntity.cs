using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NemesisEuchre.DataAccess.Entities;

public class GameSessionConnectionEntity
{
    public string ConnectionId { get; set; } = string.Empty;

    public int GameSessionId { get; set; }

    public int UserId { get; set; }

    public DateTime ConnectedAt { get; set; }

    public DateTime? DisconnectedAt { get; set; }

    public GameSessionEntity? GameSession { get; set; }

    public UserEntity? User { get; set; }
}

public class GameSessionConnectionEntityConfiguration : IEntityTypeConfiguration<GameSessionConnectionEntity>
{
    public void Configure(EntityTypeBuilder<GameSessionConnectionEntity> builder)
    {
        builder.ToTable("GameSessionConnections");

        builder.HasKey(e => e.ConnectionId);

        builder.Property(e => e.ConnectionId).HasMaxLength(128);

        builder.Property(e => e.ConnectedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(e => e.GameSession)
            .WithMany()
            .HasForeignKey(e => e.GameSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

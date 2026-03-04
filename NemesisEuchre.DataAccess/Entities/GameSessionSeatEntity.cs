using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.DataAccess.Entities;

public class GameSessionSeatEntity : EntityBase
{
    public int GameSessionId { get; set; }

    public PlayerPosition Position { get; set; }

    public int? UserId { get; set; }

    public ActorType? BotActorType { get; set; }

    public string? BotModelName { get; set; }

    public GameSessionEntity? GameSession { get; set; }

    public UserEntity? User { get; set; }
}

public class GameSessionSeatEntityConfiguration : IEntityTypeConfiguration<GameSessionSeatEntity>
{
    public void Configure(EntityTypeBuilder<GameSessionSeatEntity> builder)
    {
        builder.ToTable("GameSessionSeats");

        builder.HasKey(e => new { e.GameSessionId, e.Position });

        builder.Property(e => e.Position).IsRequired();

        builder.Property(e => e.BotModelName).HasMaxLength(100);

        builder.HasOne(e => e.GameSession)
            .WithMany(s => s.GameSessionSeats)
            .HasForeignKey(e => e.GameSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.DataAccess.Entities.Metadata;

namespace NemesisEuchre.DataAccess.Entities;

public class GamePlayer
{
    public int GameId { get; set; }

    public int PlayerPositionId { get; set; }

    public int ActorTypeId { get; set; }

    public GameEntity? Game { get; set; }

    public PlayerPositionMetadata? PlayerPosition { get; set; }

    public ActorTypeMetadata? ActorType { get; set; }
}

public class GamePlayerConfiguration : IEntityTypeConfiguration<GamePlayer>
{
    public void Configure(EntityTypeBuilder<GamePlayer> builder)
    {
        builder.ToTable("GamePlayers");

        builder.HasKey(e => new { e.GameId, e.PlayerPositionId });

        builder.HasOne(e => e.Game)
            .WithMany(g => g.GamePlayers)
            .HasForeignKey(e => e.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.PlayerPosition)
            .WithMany()
            .HasForeignKey(e => e.PlayerPositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ActorType)
            .WithMany()
            .HasForeignKey(e => e.ActorTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

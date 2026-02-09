using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.DataAccess.Entities.Metadata;

namespace NemesisEuchre.DataAccess.Entities;

public class DealPlayerEntity
{
    public int DealPlayerId { get; set; }

    public int DealId { get; set; }

    public int PlayerPositionId { get; set; }

    public int? ActorTypeId { get; set; }

    public DealEntity? Deal { get; set; }

    public PlayerPositionMetadata? PlayerPosition { get; set; }

    public ActorTypeMetadata? ActorType { get; set; }

    public ICollection<DealPlayerStartingHandCard> StartingHandCards { get; set; } = [];
}

public class DealPlayerEntityConfiguration : IEntityTypeConfiguration<DealPlayerEntity>
{
    public void Configure(EntityTypeBuilder<DealPlayerEntity> builder)
    {
        builder.ToTable("DealPlayers");

        builder.HasKey(e => e.DealPlayerId);

        builder.Property(e => e.DealPlayerId)
            .ValueGeneratedOnAdd();

        builder.HasOne(e => e.Deal)
            .WithMany(d => d.DealPlayers)
            .HasForeignKey(e => e.DealId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.PlayerPosition)
            .WithMany()
            .HasForeignKey(e => e.PlayerPositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ActorType)
            .WithMany()
            .HasForeignKey(e => e.ActorTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.DealId, e.PlayerPositionId })
            .IsUnique()
            .HasDatabaseName("IX_DealPlayers_DealId_PlayerPositionId");
    }
}

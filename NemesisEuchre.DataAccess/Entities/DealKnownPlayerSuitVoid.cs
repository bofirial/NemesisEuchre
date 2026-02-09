using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.DataAccess.Entities.Metadata;

namespace NemesisEuchre.DataAccess.Entities;

public class DealKnownPlayerSuitVoid
{
    public int DealId { get; set; }

    public int PlayerPositionId { get; set; }

    public int SuitId { get; set; }

    public DealEntity? Deal { get; set; }

    public PlayerPositionMetadata? PlayerPosition { get; set; }

    public SuitMetadata? Suit { get; set; }
}

public class DealKnownPlayerSuitVoidConfiguration : IEntityTypeConfiguration<DealKnownPlayerSuitVoid>
{
    public void Configure(EntityTypeBuilder<DealKnownPlayerSuitVoid> builder)
    {
        builder.ToTable("DealKnownPlayerSuitVoids");

        builder.HasKey(e => new { e.DealId, e.PlayerPositionId, e.SuitId });

        builder.HasOne(e => e.Deal)
            .WithMany(d => d.DealKnownPlayerSuitVoids)
            .HasForeignKey(e => e.DealId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.PlayerPosition)
            .WithMany()
            .HasForeignKey(e => e.PlayerPositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Suit)
            .WithMany()
            .HasForeignKey(e => e.SuitId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

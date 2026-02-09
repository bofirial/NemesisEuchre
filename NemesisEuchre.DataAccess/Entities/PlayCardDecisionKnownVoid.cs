using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.DataAccess.Entities.Metadata;

namespace NemesisEuchre.DataAccess.Entities;

public class PlayCardDecisionKnownVoid
{
    public int PlayCardDecisionId { get; set; }

    public int RelativePlayerPositionId { get; set; }

    public int RelativeSuitId { get; set; }

    public PlayCardDecisionEntity? PlayCardDecision { get; set; }

    public RelativePlayerPositionMetadata? RelativePlayerPosition { get; set; }

    public RelativeSuitMetadata? RelativeSuit { get; set; }
}

public class PlayCardDecisionKnownVoidConfiguration : IEntityTypeConfiguration<PlayCardDecisionKnownVoid>
{
    public void Configure(EntityTypeBuilder<PlayCardDecisionKnownVoid> builder)
    {
        builder.ToTable("PlayCardDecisionKnownVoids");

        builder.HasKey(e => new { e.PlayCardDecisionId, e.RelativePlayerPositionId, e.RelativeSuitId });

        builder.HasOne(e => e.PlayCardDecision)
            .WithMany(d => d.KnownVoids)
            .HasForeignKey(e => e.PlayCardDecisionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.RelativePlayerPosition)
            .WithMany()
            .HasForeignKey(e => e.RelativePlayerPositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.RelativeSuit)
            .WithMany()
            .HasForeignKey(e => e.RelativeSuitId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.DataAccess.Entities.Metadata;

namespace NemesisEuchre.DataAccess.Entities;

public class DiscardCardDecisionPredictedPoints
{
    public int DiscardCardDecisionId { get; set; }

    public int RelativeCardId { get; set; }

    public float PredictedPoints { get; set; }

    public DiscardCardDecisionEntity? DiscardCardDecision { get; set; }

    public RelativeCardMetadata? RelativeCard { get; set; }
}

public class DiscardCardDecisionPredictedPointsConfiguration : IEntityTypeConfiguration<DiscardCardDecisionPredictedPoints>
{
    public void Configure(EntityTypeBuilder<DiscardCardDecisionPredictedPoints> builder)
    {
        builder.ToTable("DiscardCardDecisionPredictedPoints");

        builder.HasKey(e => new { e.DiscardCardDecisionId, e.RelativeCardId });

        builder.HasOne(e => e.DiscardCardDecision)
            .WithMany(d => d.PredictedPoints)
            .HasForeignKey(e => e.DiscardCardDecisionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.RelativeCard)
            .WithMany()
            .HasForeignKey(e => e.RelativeCardId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

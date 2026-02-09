using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.DataAccess.Entities.Metadata;

namespace NemesisEuchre.DataAccess.Entities;

public class PlayCardDecisionPredictedPoints
{
    public int PlayCardDecisionId { get; set; }

    public int RelativeCardId { get; set; }

    public float PredictedPoints { get; set; }

    public PlayCardDecisionEntity? PlayCardDecision { get; set; }

    public RelativeCardMetadata? RelativeCard { get; set; }
}

public class PlayCardDecisionPredictedPointsConfiguration : IEntityTypeConfiguration<PlayCardDecisionPredictedPoints>
{
    public void Configure(EntityTypeBuilder<PlayCardDecisionPredictedPoints> builder)
    {
        builder.ToTable("PlayCardDecisionPredictedPoints");

        builder.HasKey(e => new { e.PlayCardDecisionId, e.RelativeCardId });

        builder.HasOne(e => e.PlayCardDecision)
            .WithMany(d => d.PredictedPoints)
            .HasForeignKey(e => e.PlayCardDecisionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.RelativeCard)
            .WithMany()
            .HasForeignKey(e => e.RelativeCardId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.DataAccess.Entities.Metadata;

namespace NemesisEuchre.DataAccess.Entities;

public class CallTrumpDecisionPredictedPoints
{
    public int CallTrumpDecisionId { get; set; }

    public int CallTrumpDecisionValueId { get; set; }

    public float PredictedPoints { get; set; }

    public CallTrumpDecisionEntity? CallTrumpDecision { get; set; }

    public CallTrumpDecisionValueMetadata? CallTrumpDecisionValue { get; set; }
}

public class CallTrumpDecisionPredictedPointsConfiguration : IEntityTypeConfiguration<CallTrumpDecisionPredictedPoints>
{
    public void Configure(EntityTypeBuilder<CallTrumpDecisionPredictedPoints> builder)
    {
        builder.ToTable("CallTrumpDecisionPredictedPoints");

        builder.HasKey(e => new { e.CallTrumpDecisionId, e.CallTrumpDecisionValueId });

        builder.HasOne(e => e.CallTrumpDecision)
            .WithMany(d => d.PredictedPoints)
            .HasForeignKey(e => e.CallTrumpDecisionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.CallTrumpDecisionValue)
            .WithMany()
            .HasForeignKey(e => e.CallTrumpDecisionValueId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

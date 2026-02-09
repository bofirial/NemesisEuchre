using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.DataAccess.Entities.Metadata;

namespace NemesisEuchre.DataAccess.Entities;

public class CallTrumpDecisionValidDecision
{
    public int CallTrumpDecisionId { get; set; }

    public int CallTrumpDecisionValueId { get; set; }

    public CallTrumpDecisionEntity? CallTrumpDecision { get; set; }

    public CallTrumpDecisionValueMetadata? CallTrumpDecisionValue { get; set; }
}

public class CallTrumpDecisionValidDecisionConfiguration : IEntityTypeConfiguration<CallTrumpDecisionValidDecision>
{
    public void Configure(EntityTypeBuilder<CallTrumpDecisionValidDecision> builder)
    {
        builder.ToTable("CallTrumpValidDecisions");

        builder.HasKey(e => new { e.CallTrumpDecisionId, e.CallTrumpDecisionValueId });

        builder.HasOne(e => e.CallTrumpDecision)
            .WithMany(d => d.ValidDecisions)
            .HasForeignKey(e => e.CallTrumpDecisionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.CallTrumpDecisionValue)
            .WithMany()
            .HasForeignKey(e => e.CallTrumpDecisionValueId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

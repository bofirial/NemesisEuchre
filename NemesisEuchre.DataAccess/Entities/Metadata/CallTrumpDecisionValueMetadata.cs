using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NemesisEuchre.DataAccess.Entities.Metadata;

public class CallTrumpDecisionValueMetadata
{
    public int CallTrumpDecisionValueId { get; set; }

    public required string Name { get; set; }
}

public class CallTrumpDecisionValueMetadataConfiguration : IEntityTypeConfiguration<CallTrumpDecisionValueMetadata>
{
    public void Configure(EntityTypeBuilder<CallTrumpDecisionValueMetadata> builder)
    {
        builder.ToTable("CallTrumpDecisionValues");

        builder.HasKey(e => e.CallTrumpDecisionValueId);

        builder.Property(e => e.CallTrumpDecisionValueId)
            .ValueGeneratedNever();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(30);

        builder.HasData(
            new CallTrumpDecisionValueMetadata { CallTrumpDecisionValueId = 0, Name = "Pass" },
            new CallTrumpDecisionValueMetadata { CallTrumpDecisionValueId = 1, Name = "CallSpades" },
            new CallTrumpDecisionValueMetadata { CallTrumpDecisionValueId = 2, Name = "CallHearts" },
            new CallTrumpDecisionValueMetadata { CallTrumpDecisionValueId = 3, Name = "CallClubs" },
            new CallTrumpDecisionValueMetadata { CallTrumpDecisionValueId = 4, Name = "CallDiamonds" },
            new CallTrumpDecisionValueMetadata { CallTrumpDecisionValueId = 5, Name = "CallSpadesAndGoAlone" },
            new CallTrumpDecisionValueMetadata { CallTrumpDecisionValueId = 6, Name = "CallHeartsAndGoAlone" },
            new CallTrumpDecisionValueMetadata { CallTrumpDecisionValueId = 7, Name = "CallClubsAndGoAlone" },
            new CallTrumpDecisionValueMetadata { CallTrumpDecisionValueId = 8, Name = "CallDiamondsAndGoAlone" },
            new CallTrumpDecisionValueMetadata { CallTrumpDecisionValueId = 9, Name = "OrderItUp" },
            new CallTrumpDecisionValueMetadata { CallTrumpDecisionValueId = 10, Name = "OrderItUpAndGoAlone" });
    }
}

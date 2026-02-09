using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.DataAccess.Entities.Metadata;

namespace NemesisEuchre.DataAccess.Entities;

public class DiscardCardDecisionCardsInHand
{
    public int DiscardCardDecisionId { get; set; }

    public int RelativeCardId { get; set; }

    public int SortOrder { get; set; }

    public DiscardCardDecisionEntity? DiscardCardDecision { get; set; }

    public RelativeCardMetadata? RelativeCard { get; set; }
}

public class DiscardCardDecisionCardsInHandConfiguration : IEntityTypeConfiguration<DiscardCardDecisionCardsInHand>
{
    public void Configure(EntityTypeBuilder<DiscardCardDecisionCardsInHand> builder)
    {
        builder.ToTable("DiscardCardDecisionCardsInHand");

        builder.HasKey(e => new { e.DiscardCardDecisionId, e.RelativeCardId });

        builder.HasOne(e => e.DiscardCardDecision)
            .WithMany(d => d.CardsInHand)
            .HasForeignKey(e => e.DiscardCardDecisionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.RelativeCard)
            .WithMany()
            .HasForeignKey(e => e.RelativeCardId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

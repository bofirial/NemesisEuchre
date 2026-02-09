using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.DataAccess.Entities.Metadata;

namespace NemesisEuchre.DataAccess.Entities;

public class CallTrumpDecisionCardsInHand
{
    public int CallTrumpDecisionId { get; set; }

    public int CardId { get; set; }

    public int SortOrder { get; set; }

    public CallTrumpDecisionEntity? CallTrumpDecision { get; set; }

    public CardMetadata? Card { get; set; }
}

public class CallTrumpDecisionCardsInHandConfiguration : IEntityTypeConfiguration<CallTrumpDecisionCardsInHand>
{
    public void Configure(EntityTypeBuilder<CallTrumpDecisionCardsInHand> builder)
    {
        builder.ToTable("CallTrumpDecisionCardsInHand");

        builder.HasKey(e => new { e.CallTrumpDecisionId, e.CardId });

        builder.HasOne(e => e.CallTrumpDecision)
            .WithMany(d => d.CardsInHand)
            .HasForeignKey(e => e.CallTrumpDecisionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Card)
            .WithMany()
            .HasForeignKey(e => e.CardId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

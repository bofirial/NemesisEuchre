using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.DataAccess.Entities.Metadata;

namespace NemesisEuchre.DataAccess.Entities;

public class PlayCardDecisionCardsInHand
{
    public int PlayCardDecisionId { get; set; }

    public int RelativeCardId { get; set; }

    public int SortOrder { get; set; }

    public PlayCardDecisionEntity? PlayCardDecision { get; set; }

    public RelativeCardMetadata? RelativeCard { get; set; }
}

public class PlayCardDecisionCardsInHandConfiguration : IEntityTypeConfiguration<PlayCardDecisionCardsInHand>
{
    public void Configure(EntityTypeBuilder<PlayCardDecisionCardsInHand> builder)
    {
        builder.ToTable("PlayCardDecisionCardsInHand");

        builder.HasKey(e => new { e.PlayCardDecisionId, e.RelativeCardId });

        builder.HasOne(e => e.PlayCardDecision)
            .WithMany(d => d.CardsInHand)
            .HasForeignKey(e => e.PlayCardDecisionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.RelativeCard)
            .WithMany()
            .HasForeignKey(e => e.RelativeCardId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

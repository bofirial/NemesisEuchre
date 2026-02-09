using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.DataAccess.Entities.Metadata;

namespace NemesisEuchre.DataAccess.Entities;

public class PlayCardDecisionPlayedCard
{
    public int PlayCardDecisionId { get; set; }

    public int RelativePlayerPositionId { get; set; }

    public int RelativeCardId { get; set; }

    public PlayCardDecisionEntity? PlayCardDecision { get; set; }

    public RelativePlayerPositionMetadata? RelativePlayerPosition { get; set; }

    public RelativeCardMetadata? RelativeCard { get; set; }
}

public class PlayCardDecisionPlayedCardConfiguration : IEntityTypeConfiguration<PlayCardDecisionPlayedCard>
{
    public void Configure(EntityTypeBuilder<PlayCardDecisionPlayedCard> builder)
    {
        builder.ToTable("PlayCardDecisionPlayedCards");

        builder.HasKey(e => new { e.PlayCardDecisionId, e.RelativePlayerPositionId });

        builder.HasOne(e => e.PlayCardDecision)
            .WithMany(d => d.PlayedCards)
            .HasForeignKey(e => e.PlayCardDecisionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.RelativePlayerPosition)
            .WithMany()
            .HasForeignKey(e => e.RelativePlayerPositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.RelativeCard)
            .WithMany()
            .HasForeignKey(e => e.RelativeCardId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

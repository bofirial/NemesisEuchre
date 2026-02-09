using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.DataAccess.Entities.Metadata;

namespace NemesisEuchre.DataAccess.Entities;

public class PlayCardDecisionAccountedForCard
{
    public int PlayCardDecisionId { get; set; }

    public int RelativeCardId { get; set; }

    public PlayCardDecisionEntity? PlayCardDecision { get; set; }

    public RelativeCardMetadata? RelativeCard { get; set; }
}

public class PlayCardDecisionAccountedForCardConfiguration : IEntityTypeConfiguration<PlayCardDecisionAccountedForCard>
{
    public void Configure(EntityTypeBuilder<PlayCardDecisionAccountedForCard> builder)
    {
        builder.ToTable("PlayCardDecisionCardsAccountedFor");

        builder.HasKey(e => new { e.PlayCardDecisionId, e.RelativeCardId });

        builder.HasOne(e => e.PlayCardDecision)
            .WithMany(d => d.CardsAccountedFor)
            .HasForeignKey(e => e.PlayCardDecisionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.RelativeCard)
            .WithMany()
            .HasForeignKey(e => e.RelativeCardId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

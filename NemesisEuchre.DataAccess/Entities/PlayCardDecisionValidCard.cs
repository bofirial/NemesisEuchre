using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.DataAccess.Entities.Metadata;

namespace NemesisEuchre.DataAccess.Entities;

public class PlayCardDecisionValidCard
{
    public int PlayCardDecisionId { get; set; }

    public int RelativeCardId { get; set; }

    public PlayCardDecisionEntity? PlayCardDecision { get; set; }

    public RelativeCardMetadata? RelativeCard { get; set; }
}

public class PlayCardDecisionValidCardConfiguration : IEntityTypeConfiguration<PlayCardDecisionValidCard>
{
    public void Configure(EntityTypeBuilder<PlayCardDecisionValidCard> builder)
    {
        builder.ToTable("PlayCardDecisionValidCards");

        builder.HasKey(e => new { e.PlayCardDecisionId, e.RelativeCardId });

        builder.HasOne(e => e.PlayCardDecision)
            .WithMany(d => d.ValidCards)
            .HasForeignKey(e => e.PlayCardDecisionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.RelativeCard)
            .WithMany()
            .HasForeignKey(e => e.RelativeCardId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

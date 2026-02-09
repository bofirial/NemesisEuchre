using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NemesisEuchre.DataAccess.Entities.Metadata;

public class RelativeCardMetadata
{
    public int RelativeCardId { get; set; }

    public int RelativeSuitId { get; set; }

    public int RankId { get; set; }

    public RelativeSuitMetadata? RelativeSuit { get; set; }

    public RankMetadata? Rank { get; set; }
}

public class RelativeCardMetadataConfiguration : IEntityTypeConfiguration<RelativeCardMetadata>
{
    public void Configure(EntityTypeBuilder<RelativeCardMetadata> builder)
    {
        builder.ToTable("RelativeCards");

        builder.HasKey(e => e.RelativeCardId);

        builder.Property(e => e.RelativeCardId)
            .ValueGeneratedNever();

        builder.HasOne(e => e.RelativeSuit)
            .WithMany()
            .HasForeignKey(e => e.RelativeSuitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Rank)
            .WithMany()
            .HasForeignKey(e => e.RankId)
            .OnDelete(DeleteBehavior.Restrict);

        var cards = new List<object>();
        int[] relativeSuitIds = [0, 1, 2, 3];
        int[] rankIds = [9, 10, 11, 12, 13, 14, 15, 16];

        foreach (var suitId in relativeSuitIds)
        {
            foreach (var rankId in rankIds)
            {
                cards.Add(new { RelativeCardId = (suitId * 100) + rankId, RelativeSuitId = suitId, RankId = rankId });
            }
        }

        builder.HasData(cards);
    }
}

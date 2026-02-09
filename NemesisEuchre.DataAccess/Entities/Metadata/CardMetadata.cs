using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NemesisEuchre.DataAccess.Entities.Metadata;

public class CardMetadata
{
    public int CardId { get; set; }

    public int SuitId { get; set; }

    public int RankId { get; set; }

    public SuitMetadata? Suit { get; set; }

    public RankMetadata? Rank { get; set; }
}

public class CardMetadataConfiguration : IEntityTypeConfiguration<CardMetadata>
{
    public void Configure(EntityTypeBuilder<CardMetadata> builder)
    {
        builder.ToTable("Cards");

        builder.HasKey(e => e.CardId);

        builder.Property(e => e.CardId)
            .ValueGeneratedNever();

        builder.HasOne(e => e.Suit)
            .WithMany()
            .HasForeignKey(e => e.SuitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Rank)
            .WithMany()
            .HasForeignKey(e => e.RankId)
            .OnDelete(DeleteBehavior.Restrict);

        var cards = new List<object>();
        int[] suitIds = [1, 2, 3, 4];
        int[] rankIds = [9, 10, 11, 12, 13, 14];

        foreach (var suitId in suitIds)
        {
            foreach (var rankId in rankIds)
            {
                cards.Add(new { CardId = (suitId * 100) + rankId, SuitId = suitId, RankId = rankId });
            }
        }

        builder.HasData(cards);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.DataAccess.Entities.Metadata;

namespace NemesisEuchre.DataAccess.Entities;

public class DealDeckCard
{
    public int DealId { get; set; }

    public int CardId { get; set; }

    public int SortOrder { get; set; }

    public DealEntity? Deal { get; set; }

    public CardMetadata? Card { get; set; }
}

public class DealDeckCardConfiguration : IEntityTypeConfiguration<DealDeckCard>
{
    public void Configure(EntityTypeBuilder<DealDeckCard> builder)
    {
        builder.ToTable("DealDeckCards");

        builder.HasKey(e => new { e.DealId, e.CardId });

        builder.Property(e => e.SortOrder)
            .IsRequired();

        builder.HasOne(e => e.Deal)
            .WithMany(d => d.DealDeckCards)
            .HasForeignKey(e => e.DealId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Card)
            .WithMany()
            .HasForeignKey(e => e.CardId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

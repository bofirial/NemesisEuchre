using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.DataAccess.Entities.Metadata;

namespace NemesisEuchre.DataAccess.Entities;

public class DealPlayerStartingHandCard
{
    public int DealPlayerId { get; set; }

    public int CardId { get; set; }

    public int SortOrder { get; set; }

    public DealPlayerEntity? DealPlayer { get; set; }

    public CardMetadata? Card { get; set; }
}

public class DealPlayerStartingHandCardConfiguration : IEntityTypeConfiguration<DealPlayerStartingHandCard>
{
    public void Configure(EntityTypeBuilder<DealPlayerStartingHandCard> builder)
    {
        builder.ToTable("DealPlayerStartingHandCards");

        builder.HasKey(e => new { e.DealPlayerId, e.CardId });

        builder.Property(e => e.SortOrder)
            .IsRequired();

        builder.HasOne(e => e.DealPlayer)
            .WithMany(dp => dp.StartingHandCards)
            .HasForeignKey(e => e.DealPlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Card)
            .WithMany()
            .HasForeignKey(e => e.CardId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

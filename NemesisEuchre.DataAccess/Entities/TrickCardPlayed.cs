using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.DataAccess.Entities.Metadata;

namespace NemesisEuchre.DataAccess.Entities;

public class TrickCardPlayed
{
    public int TrickId { get; set; }

    public int PlayerPositionId { get; set; }

    public int CardId { get; set; }

    public int PlayOrder { get; set; }

    public TrickEntity? Trick { get; set; }

    public PlayerPositionMetadata? PlayerPosition { get; set; }

    public CardMetadata? Card { get; set; }
}

public class TrickCardPlayedConfiguration : IEntityTypeConfiguration<TrickCardPlayed>
{
    public void Configure(EntityTypeBuilder<TrickCardPlayed> builder)
    {
        builder.ToTable("TrickCardsPlayed");

        builder.HasKey(e => new { e.TrickId, e.PlayerPositionId });

        builder.Property(e => e.PlayOrder)
            .IsRequired();

        builder.HasOne(e => e.Trick)
            .WithMany(t => t.TrickCardsPlayed)
            .HasForeignKey(e => e.TrickId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.PlayerPosition)
            .WithMany()
            .HasForeignKey(e => e.PlayerPositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Card)
            .WithMany()
            .HasForeignKey(e => e.CardId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.DataAccess.Entities;

namespace NemesisEuchre.DataAccess.Configurations;

public class DealEntityConfiguration : IEntityTypeConfiguration<DealEntity>
{
    public void Configure(EntityTypeBuilder<DealEntity> builder)
    {
        builder.ToTable("Deals");

        builder.HasKey(e => e.DealId);

        builder.Property(e => e.DealId)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.GameId)
            .IsRequired();

        builder.Property(e => e.DealNumber)
            .IsRequired();

        builder.Property(e => e.DealStatus)
            .IsRequired();

        builder.Property(e => e.DealerPosition);

        builder.Property(e => e.DeckJson)
            .IsRequired();

        builder.Property(e => e.UpCardJson)
            .HasMaxLength(200);

        builder.Property(e => e.Trump);

        builder.Property(e => e.CallingPlayer);

        builder.Property(e => e.CallingPlayerIsGoingAlone)
            .IsRequired();

        builder.Property(e => e.DealResult);

        builder.Property(e => e.WinningTeam);

        builder.Property(e => e.Team1Score)
            .IsRequired();

        builder.Property(e => e.Team2Score)
            .IsRequired();

        builder.Property(e => e.PlayersJson)
            .IsRequired();

        builder.HasOne(e => e.Game)
            .WithMany(g => g.Deals)
            .HasForeignKey(e => e.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Tricks)
            .WithOne(t => t.Deal)
            .HasForeignKey(t => t.DealId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.CallTrumpDecisions)
            .WithOne(d => d.Deal)
            .HasForeignKey(d => d.DealId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.DiscardCardDecisions)
            .WithOne(d => d.Deal)
            .HasForeignKey(d => d.DealId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.PlayCardDecisions)
            .WithOne(d => d.Deal)
            .HasForeignKey(d => d.DealId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.GameId)
            .HasDatabaseName("IX_Deals_GameId");
    }
}

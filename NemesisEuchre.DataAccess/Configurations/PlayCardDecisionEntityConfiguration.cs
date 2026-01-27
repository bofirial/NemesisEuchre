using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.DataAccess.Entities;

namespace NemesisEuchre.DataAccess.Configurations;

public class PlayCardDecisionEntityConfiguration : IEntityTypeConfiguration<PlayCardDecisionEntity>
{
    public void Configure(EntityTypeBuilder<PlayCardDecisionEntity> builder)
    {
        builder.ToTable("PlayCardDecisions");

        builder.HasKey(e => e.PlayCardDecisionId);

        builder.Property(e => e.PlayCardDecisionId)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.DealId)
            .IsRequired();

        builder.Property(e => e.TrickId)
            .IsRequired();

        builder.Property(e => e.TrickNumber)
            .IsRequired();

        builder.Property(e => e.HandJson)
            .IsRequired();

        builder.Property(e => e.DecidingPlayerPosition)
            .IsRequired();

        builder.Property(e => e.TeamScore)
            .IsRequired();

        builder.Property(e => e.OpponentScore)
            .IsRequired();

        builder.Property(e => e.TrumpSuit)
            .IsRequired();

        builder.Property(e => e.LeadPlayer)
            .IsRequired();

        builder.Property(e => e.LeadSuit);

        builder.Property(e => e.PlayedCardsJson)
            .IsRequired();

        builder.Property(e => e.WinningTrickPlayer);

        builder.Property(e => e.ValidCardsToPlayJson)
            .IsRequired();

        builder.Property(e => e.ChosenCardJson)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.ActorType);

        builder.Property(e => e.DidTeamWinTrick);

        builder.Property(e => e.DidTeamWinDeal);

        builder.Property(e => e.DidTeamWinGame);

        builder.HasOne(e => e.Deal)
            .WithMany(d => d.PlayCardDecisions)
            .HasForeignKey(e => e.DealId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(e => e.Trick)
            .WithMany(t => t.PlayCardDecisions)
            .HasForeignKey(e => e.TrickId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.DealId)
            .HasDatabaseName("IX_PlayCardDecisions_DealId");

        builder.HasIndex(e => e.TrickId)
            .HasDatabaseName("IX_PlayCardDecisions_TrickId");

        builder.HasIndex(e => new { e.ActorType, e.TrickNumber })
            .HasDatabaseName("IX_PlayCardDecisions_ActorType_TrickNumber");
    }
}

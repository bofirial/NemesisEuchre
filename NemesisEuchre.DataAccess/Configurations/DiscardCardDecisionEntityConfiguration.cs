using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.DataAccess.Entities;

namespace NemesisEuchre.DataAccess.Configurations;

public class DiscardCardDecisionEntityConfiguration : IEntityTypeConfiguration<DiscardCardDecisionEntity>
{
    public void Configure(EntityTypeBuilder<DiscardCardDecisionEntity> builder)
    {
        builder.ToTable("DiscardCardDecisions");

        builder.HasKey(e => e.DiscardCardDecisionId);

        builder.Property(e => e.DiscardCardDecisionId)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.DealId)
            .IsRequired();

        builder.Property(e => e.HandJson)
            .IsRequired();

        builder.Property(e => e.CallingPlayerPosition)
            .IsRequired();

        builder.Property(e => e.TeamScore)
            .IsRequired();

        builder.Property(e => e.OpponentScore)
            .IsRequired();

        builder.Property(e => e.ValidCardsToDiscardJson)
            .IsRequired();

        builder.Property(e => e.ChosenCardJson)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.ActorType);

        builder.Property(e => e.DidTeamWinDeal);

        builder.Property(e => e.DidTeamWinGame);

        builder.HasOne(e => e.Deal)
            .WithMany(d => d.DiscardCardDecisions)
            .HasForeignKey(e => e.DealId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.DealId)
            .HasDatabaseName("IX_DiscardCardDecisions_DealId");

        builder.HasIndex(e => e.ActorType)
            .HasDatabaseName("IX_DiscardCardDecisions_ActorType");
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.DataAccess.Entities;

namespace NemesisEuchre.DataAccess.Configurations;

public class CallTrumpDecisionEntityConfiguration : IEntityTypeConfiguration<CallTrumpDecisionEntity>
{
    public void Configure(EntityTypeBuilder<CallTrumpDecisionEntity> builder)
    {
        builder.ToTable("CallTrumpDecisions");

        builder.HasKey(e => e.CallTrumpDecisionId);

        builder.Property(e => e.CallTrumpDecisionId)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.DealId)
            .IsRequired();

        builder.Property(e => e.HandJson)
            .IsRequired();

        builder.Property(e => e.UpCardJson)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.DealerPosition)
            .IsRequired();

        builder.Property(e => e.DecidingPlayerPosition)
            .IsRequired();

        builder.Property(e => e.TeamScore)
            .IsRequired();

        builder.Property(e => e.OpponentScore)
            .IsRequired();

        builder.Property(e => e.ValidDecisionsJson)
            .IsRequired();

        builder.Property(e => e.ChosenDecisionJson)
            .IsRequired();

        builder.Property(e => e.DecisionOrder)
            .IsRequired();

        builder.Property(e => e.ActorType);

        builder.Property(e => e.DidTeamWinDeal);

        builder.Property(e => e.DidTeamWinGame);

        builder.HasOne(e => e.Deal)
            .WithMany(d => d.CallTrumpDecisions)
            .HasForeignKey(e => e.DealId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.DealId)
            .HasDatabaseName("IX_CallTrumpDecisions_DealId");

        builder.HasIndex(e => e.ActorType)
            .HasDatabaseName("IX_CallTrumpDecisions_ActorType");
    }
}

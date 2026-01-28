using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.DataAccess.Entities;

public class DiscardCardDecisionEntity
{
    public int DiscardCardDecisionId { get; set; }

    public int DealId { get; set; }

    public string CardsInHandJson { get; set; } = null!;

    public short TeamScore { get; set; }

    public short OpponentScore { get; set; }

    public RelativePlayerPosition CallingPlayer { get; set; }

    public bool CallingPlayerGoingAlone { get; set; }

    public string ValidCardsToDiscardJson { get; set; } = null!;

    public string ChosenCardJson { get; set; } = null!;

    public ActorType? ActorType { get; set; }

    public bool? DidTeamWinDeal { get; set; }

    public bool? DidTeamWinGame { get; set; }

    public DealEntity Deal { get; set; } = null!;
}

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

        builder.Property(e => e.CardsInHandJson)
            .IsRequired();

        builder.Property(e => e.CallingPlayer)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(25);

        builder.Property(e => e.TeamScore)
            .IsRequired();

        builder.Property(e => e.OpponentScore)
            .IsRequired();

        builder.Property(e => e.ValidCardsToDiscardJson)
            .IsRequired();

        builder.Property(e => e.ChosenCardJson)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.ActorType)
            .HasConversion<string>()
            .HasMaxLength(10);

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

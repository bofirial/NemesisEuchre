using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.DataAccess.Entities;

public class PlayCardDecisionEntity
{
    public int PlayCardDecisionId { get; set; }

    public int DealId { get; set; }

    public int TrickId { get; set; }

    public string CardsInHandJson { get; set; } = null!;

    public short TeamScore { get; set; }

    public short OpponentScore { get; set; }

    public RelativePlayerPosition LeadPlayer { get; set; }

    public RelativeSuit? LeadSuit { get; set; }

    public string PlayedCardsJson { get; set; } = null!;

    public RelativePlayerPosition? WinningTrickPlayer { get; set; }

    public string ValidCardsToPlayJson { get; set; } = null!;

    public RelativePlayerPosition CallingPlayer { get; set; }

    public bool CallingPlayerGoingAlone { get; set; }

    public string ChosenCardJson { get; set; } = null!;

    public ActorType? ActorType { get; set; }

    public bool? DidTeamWinTrick { get; set; }

    public bool? DidTeamWinDeal { get; set; }

    public short? RelativeDealPoints { get; set; }

    public bool? DidTeamWinGame { get; set; }

    public DealEntity Deal { get; set; } = null!;

    public TrickEntity Trick { get; set; } = null!;
}

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

        builder.Property(e => e.CardsInHandJson)
            .IsRequired();

        builder.Property(e => e.TeamScore)
            .IsRequired();

        builder.Property(e => e.OpponentScore)
            .IsRequired();

        builder.Property(e => e.LeadPlayer)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(25);

        builder.Property(e => e.LeadSuit)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(e => e.PlayedCardsJson)
            .IsRequired();

        builder.Property(e => e.WinningTrickPlayer)
            .HasConversion<string>()
            .HasMaxLength(25);

        builder.Property(e => e.ValidCardsToPlayJson)
            .IsRequired();

        builder.Property(e => e.CallingPlayer)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(25);

        builder.Property(e => e.CallingPlayerGoingAlone)
            .IsRequired();

        builder.Property(e => e.ChosenCardJson)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.ActorType)
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(e => e.DidTeamWinTrick);

        builder.Property(e => e.DidTeamWinDeal);

        builder.Property(e => e.RelativeDealPoints);

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

        builder.HasIndex(e => e.ActorType)
            .HasDatabaseName("IX_PlayCardDecisions_ActorType");
    }
}

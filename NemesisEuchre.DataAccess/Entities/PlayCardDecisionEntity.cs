using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.DataAccess.Entities;

public class PlayCardDecisionEntity : IDecisionEntity
{
    public int PlayCardDecisionId { get; set; }

    public int DealId { get; set; }

    public int TrickId { get; set; }

    public required string CardsInHandJson { get; set; }

    public short TeamScore { get; set; }

    public short OpponentScore { get; set; }

    public RelativePlayerPosition LeadPlayer { get; set; }

    public RelativeSuit? LeadSuit { get; set; }

    public required string PlayedCardsJson { get; set; }

    public RelativePlayerPosition? WinningTrickPlayer { get; set; }

    public short TrickNumber { get; set; }

    public required string ValidCardsToPlayJson { get; set; }

    public RelativePlayerPosition CallingPlayer { get; set; }

    public bool CallingPlayerGoingAlone { get; set; }

    public RelativePlayerPosition DealerPosition { get; set; }

    public string? DealerPickedUpCardJson { get; set; }

    public required string ChosenCardJson { get; set; }

    public required string KnownPlayerSuitVoidsJson { get; set; }

    public required string CardsAccountedForJson { get; set; }

    public ActorType? ActorType { get; set; }

    public bool? DidTeamWinTrick { get; set; }

    public bool? DidTeamWinDeal { get; set; }

    public short? RelativeDealPoints { get; set; }

    public bool? DidTeamWinGame { get; set; }

    public DealEntity? Deal { get; set; }

    public TrickEntity? Trick { get; set; }
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

        builder.Property(e => e.TrickNumber)
            .IsRequired();

        builder.Property(e => e.ValidCardsToPlayJson)
            .IsRequired();

        builder.Property(e => e.CallingPlayer)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(25);

        builder.Property(e => e.CallingPlayerGoingAlone)
            .IsRequired();

        builder.Property(e => e.DealerPosition)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(25);

        builder.Property(e => e.DealerPickedUpCardJson)
            .HasMaxLength(200);

        builder.Property(e => e.KnownPlayerSuitVoidsJson)
            .HasMaxLength(1500);

        builder.Property(e => e.CardsAccountedForJson);

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

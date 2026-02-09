using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.DataAccess.Entities.Metadata;

namespace NemesisEuchre.DataAccess.Entities;

public class PlayCardDecisionEntity : IDecisionEntity
{
    public int PlayCardDecisionId { get; set; }

    public int DealId { get; set; }

    public int TrickId { get; set; }

    public short TeamScore { get; set; }

    public short OpponentScore { get; set; }

    public int LeadRelativePlayerPositionId { get; set; }

    public int? LeadRelativeSuitId { get; set; }

    public int? WinningTrickRelativePlayerPositionId { get; set; }

    public short TrickNumber { get; set; }

    public int CallingRelativePlayerPositionId { get; set; }

    public bool CallingPlayerGoingAlone { get; set; }

    public int DealerRelativePlayerPositionId { get; set; }

    public int? DealerPickedUpRelativeCardId { get; set; }

    public int ChosenRelativeCardId { get; set; }

    public int? ActorTypeId { get; set; }

    public bool? DidTeamWinTrick { get; set; }

    public bool? DidTeamWinDeal { get; set; }

    public short? RelativeDealPoints { get; set; }

    public bool? DidTeamWinGame { get; set; }

    public DealEntity? Deal { get; set; }

    public TrickEntity? Trick { get; set; }

    public RelativePlayerPositionMetadata? LeadPlayer { get; set; }

    public RelativeSuitMetadata? LeadSuit { get; set; }

    public RelativePlayerPositionMetadata? WinningTrickPlayer { get; set; }

    public RelativePlayerPositionMetadata? CallingPlayerPosition { get; set; }

    public RelativePlayerPositionMetadata? DealerPosition { get; set; }

    public RelativeCardMetadata? DealerPickedUpRelativeCard { get; set; }

    public RelativeCardMetadata? ChosenRelativeCard { get; set; }

    public ActorTypeMetadata? ActorTypeMetadata { get; set; }

    public ICollection<PlayCardDecisionCardsInHand> CardsInHand { get; set; } = [];

    public ICollection<PlayCardDecisionPlayedCard> PlayedCards { get; set; } = [];

    public ICollection<PlayCardDecisionValidCard> ValidCards { get; set; } = [];

    public ICollection<PlayCardDecisionKnownVoid> KnownVoids { get; set; } = [];

    public ICollection<PlayCardDecisionAccountedForCard> CardsAccountedFor { get; set; } = [];

    public ICollection<PlayCardDecisionPredictedPoints> PredictedPoints { get; set; } = [];
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

        builder.Property(e => e.TeamScore)
            .IsRequired();

        builder.Property(e => e.OpponentScore)
            .IsRequired();

        builder.Property(e => e.TrickNumber)
            .IsRequired();

        builder.Property(e => e.CallingPlayerGoingAlone)
            .IsRequired();

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

        builder.HasOne(e => e.LeadPlayer)
            .WithMany()
            .HasForeignKey(e => e.LeadRelativePlayerPositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.LeadSuit)
            .WithMany()
            .HasForeignKey(e => e.LeadRelativeSuitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.WinningTrickPlayer)
            .WithMany()
            .HasForeignKey(e => e.WinningTrickRelativePlayerPositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CallingPlayerPosition)
            .WithMany()
            .HasForeignKey(e => e.CallingRelativePlayerPositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.DealerPosition)
            .WithMany()
            .HasForeignKey(e => e.DealerRelativePlayerPositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.DealerPickedUpRelativeCard)
            .WithMany()
            .HasForeignKey(e => e.DealerPickedUpRelativeCardId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ChosenRelativeCard)
            .WithMany()
            .HasForeignKey(e => e.ChosenRelativeCardId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ActorTypeMetadata)
            .WithMany()
            .HasForeignKey(e => e.ActorTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.DealId)
            .HasDatabaseName("IX_PlayCardDecisions_DealId");

        builder.HasIndex(e => e.TrickId)
            .HasDatabaseName("IX_PlayCardDecisions_TrickId");

        builder.HasIndex(e => e.ActorTypeId)
            .HasDatabaseName("IX_PlayCardDecisions_ActorTypeId");
    }
}

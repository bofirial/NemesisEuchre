using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.DataAccess.Entities.Metadata;

namespace NemesisEuchre.DataAccess.Entities;

public class DiscardCardDecisionEntity : IDecisionEntity
{
    public int DiscardCardDecisionId { get; set; }

    public int DealId { get; set; }

    public int CallingRelativePlayerPositionId { get; set; }

    public bool CallingPlayerGoingAlone { get; set; }

    public short TeamScore { get; set; }

    public short OpponentScore { get; set; }

    public int ChosenRelativeCardId { get; set; }

    public int? ActorTypeId { get; set; }

    public bool? DidTeamWinDeal { get; set; }

    public short? RelativeDealPoints { get; set; }

    public bool? DidTeamWinGame { get; set; }

    public DealEntity? Deal { get; set; }

    public RelativePlayerPositionMetadata? CallingPlayer { get; set; }

    public RelativeCardMetadata? ChosenRelativeCard { get; set; }

    public ActorTypeMetadata? ActorTypeMetadata { get; set; }

    public ICollection<DiscardCardDecisionCardsInHand> CardsInHand { get; set; } = [];

    public ICollection<DiscardCardDecisionPredictedPoints> PredictedPoints { get; set; } = [];
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

        builder.Property(e => e.CallingPlayerGoingAlone)
            .IsRequired();

        builder.Property(e => e.TeamScore)
            .IsRequired();

        builder.Property(e => e.OpponentScore)
            .IsRequired();

        builder.Property(e => e.DidTeamWinDeal);

        builder.Property(e => e.RelativeDealPoints);

        builder.Property(e => e.DidTeamWinGame);

        builder.HasOne(e => e.Deal)
            .WithMany(d => d.DiscardCardDecisions)
            .HasForeignKey(e => e.DealId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.CallingPlayer)
            .WithMany()
            .HasForeignKey(e => e.CallingRelativePlayerPositionId)
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
            .HasDatabaseName("IX_DiscardCardDecisions_DealId");

        builder.HasIndex(e => e.ActorTypeId)
            .HasDatabaseName("IX_DiscardCardDecisions_ActorTypeId");
    }
}

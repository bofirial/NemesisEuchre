using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.DataAccess.Entities.Metadata;

namespace NemesisEuchre.DataAccess.Entities;

public class CallTrumpDecisionEntity : IDecisionEntity
{
    public int CallTrumpDecisionId { get; set; }

    public int DealId { get; set; }

    public int DealerRelativePositionId { get; set; }

    public int UpCardId { get; set; }

    public short TeamScore { get; set; }

    public short OpponentScore { get; set; }

    public int ChosenDecisionValueId { get; set; }

    public byte DecisionOrder { get; set; }

    public int? ActorTypeId { get; set; }

    public bool? DidTeamWinDeal { get; set; }

    public short? RelativeDealPoints { get; set; }

    public bool? DidTeamWinGame { get; set; }

    public DealEntity? Deal { get; set; }

    public RelativePlayerPositionMetadata? DealerPosition { get; set; }

    public CardMetadata? UpCard { get; set; }

    public CallTrumpDecisionValueMetadata? ChosenDecisionValue { get; set; }

    public ActorTypeMetadata? ActorTypeMetadata { get; set; }

    public ICollection<CallTrumpDecisionCardsInHand> CardsInHand { get; set; } = [];

    public ICollection<CallTrumpDecisionValidDecision> ValidDecisions { get; set; } = [];

    public ICollection<CallTrumpDecisionPredictedPoints> PredictedPoints { get; set; } = [];
}

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

        builder.Property(e => e.TeamScore)
            .IsRequired();

        builder.Property(e => e.OpponentScore)
            .IsRequired();

        builder.Property(e => e.DecisionOrder)
            .IsRequired();

        builder.Property(e => e.DidTeamWinDeal);

        builder.Property(e => e.RelativeDealPoints);

        builder.Property(e => e.DidTeamWinGame);

        builder.HasOne(e => e.Deal)
            .WithMany(d => d.CallTrumpDecisions)
            .HasForeignKey(e => e.DealId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.DealerPosition)
            .WithMany()
            .HasForeignKey(e => e.DealerRelativePositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.UpCard)
            .WithMany()
            .HasForeignKey(e => e.UpCardId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ChosenDecisionValue)
            .WithMany()
            .HasForeignKey(e => e.ChosenDecisionValueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ActorTypeMetadata)
            .WithMany()
            .HasForeignKey(e => e.ActorTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.DealId)
            .HasDatabaseName("IX_CallTrumpDecisions_DealId");

        builder.HasIndex(e => e.ActorTypeId)
            .HasDatabaseName("IX_CallTrumpDecisions_ActorTypeId");
    }
}

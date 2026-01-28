using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.GameEngine.Constants;

namespace NemesisEuchre.DataAccess.Entities;

public class CallTrumpDecisionEntity
{
    public int CallTrumpDecisionId { get; set; }

    public int DealId { get; set; }

    public string CardsInHandJson { get; set; } = null!;

    public short TeamScore { get; set; }

    public short OpponentScore { get; set; }

    public PlayerPosition DealerPosition { get; set; }

    public string UpCardJson { get; set; } = null!;

    public string ValidDecisionsJson { get; set; } = null!;

    public PlayerPosition DecidingPlayerPosition { get; set; }

    public string ChosenDecisionJson { get; set; } = null!;

    public string? ChosenDecisionType { get; set; }

    public string? ChosenTrumpSuit { get; set; }

    public byte DecisionOrder { get; set; }

    public string? ActorType { get; set; }

    public float? DecisionConfidence { get; set; }

    public bool? DidTeamWinDeal { get; set; }

    public bool? DidTeamWinGame { get; set; }

    public DealEntity Deal { get; set; } = null!;
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

        builder.Property(e => e.CardsInHandJson)
            .IsRequired();

        builder.Property(e => e.TeamScore)
            .IsRequired();

        builder.Property(e => e.OpponentScore)
            .IsRequired();

        builder.Property(e => e.DealerPosition)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(e => e.UpCardJson)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.ValidDecisionsJson)
            .IsRequired();

        builder.Property(e => e.DecidingPlayerPosition)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(e => e.ChosenDecisionJson)
            .IsRequired();

        builder.Property(e => e.ChosenDecisionType)
            .HasMaxLength(20);

        builder.Property(e => e.ChosenTrumpSuit)
            .HasMaxLength(10);

        builder.Property(e => e.DecisionOrder)
            .IsRequired();

        builder.Property(e => e.ActorType)
            .HasMaxLength(25);

        builder.Property(e => e.DecisionConfidence);

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

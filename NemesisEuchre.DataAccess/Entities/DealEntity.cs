using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.DataAccess.Entities.Metadata;

namespace NemesisEuchre.DataAccess.Entities;

public class DealEntity
{
    public int DealId { get; set; }

    public int GameId { get; set; }

    public int DealNumber { get; set; }

    public int DealStatusId { get; set; }

    public int? DealerPositionId { get; set; }

    public int? UpCardId { get; set; }

    public int? DiscardedCardId { get; set; }

    public int? TrumpSuitId { get; set; }

    public int? CallingPlayerPositionId { get; set; }

    public bool CallingPlayerIsGoingAlone { get; set; }

    public int? ChosenCallTrumpDecisionId { get; set; }

    public int? DealResultId { get; set; }

    public int? WinningTeamId { get; set; }

    public short Team1Score { get; set; }

    public short Team2Score { get; set; }

    public GameEntity? Game { get; set; }

    public DealStatusMetadata? DealStatus { get; set; }

    public PlayerPositionMetadata? DealerPosition { get; set; }

    public CardMetadata? UpCard { get; set; }

    public CardMetadata? DiscardedCard { get; set; }

    public SuitMetadata? TrumpSuit { get; set; }

    public PlayerPositionMetadata? CallingPlayer { get; set; }

    public CallTrumpDecisionValueMetadata? ChosenDecision { get; set; }

    public DealResultMetadata? DealResult { get; set; }

    public TeamMetadata? WinningTeam { get; set; }

    public ICollection<DealDeckCard> DealDeckCards { get; set; } = [];

    public ICollection<DealPlayerEntity> DealPlayers { get; set; } = [];

    public ICollection<DealKnownPlayerSuitVoid> DealKnownPlayerSuitVoids { get; set; } = [];

    public ICollection<TrickEntity> Tricks { get; set; } = [];

    public ICollection<CallTrumpDecisionEntity> CallTrumpDecisions { get; set; } = [];

    public ICollection<DiscardCardDecisionEntity> DiscardCardDecisions { get; set; } = [];

    public ICollection<PlayCardDecisionEntity> PlayCardDecisions { get; set; } = [];
}

public class DealEntityConfiguration : IEntityTypeConfiguration<DealEntity>
{
    public void Configure(EntityTypeBuilder<DealEntity> builder)
    {
        builder.ToTable("Deals");

        builder.HasKey(e => e.DealId);

        builder.Property(e => e.DealId)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.GameId)
            .IsRequired();

        builder.Property(e => e.DealNumber)
            .IsRequired();

        builder.Property(e => e.DealStatusId)
            .IsRequired();

        builder.Property(e => e.CallingPlayerIsGoingAlone)
            .IsRequired();

        builder.Property(e => e.Team1Score)
            .IsRequired();

        builder.Property(e => e.Team2Score)
            .IsRequired();

        builder.HasOne(e => e.Game)
            .WithMany(g => g.Deals)
            .HasForeignKey(e => e.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.DealStatus)
            .WithMany()
            .HasForeignKey(e => e.DealStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.DealerPosition)
            .WithMany()
            .HasForeignKey(e => e.DealerPositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.UpCard)
            .WithMany()
            .HasForeignKey(e => e.UpCardId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.DiscardedCard)
            .WithMany()
            .HasForeignKey(e => e.DiscardedCardId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.TrumpSuit)
            .WithMany()
            .HasForeignKey(e => e.TrumpSuitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CallingPlayer)
            .WithMany()
            .HasForeignKey(e => e.CallingPlayerPositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ChosenDecision)
            .WithMany()
            .HasForeignKey(e => e.ChosenCallTrumpDecisionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.DealResult)
            .WithMany()
            .HasForeignKey(e => e.DealResultId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.WinningTeam)
            .WithMany()
            .HasForeignKey(e => e.WinningTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Tricks)
            .WithOne(t => t.Deal)
            .HasForeignKey(t => t.DealId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.CallTrumpDecisions)
            .WithOne(d => d.Deal)
            .HasForeignKey(d => d.DealId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.DiscardCardDecisions)
            .WithOne(d => d.Deal)
            .HasForeignKey(d => d.DealId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.PlayCardDecisions)
            .WithOne(d => d.Deal)
            .HasForeignKey(d => d.DealId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.GameId)
            .HasDatabaseName("IX_Deals_GameId");
    }
}

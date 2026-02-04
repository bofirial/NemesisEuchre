using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.DataAccess.Entities;

public class DealEntity
{
    public int DealId { get; set; }

    public int GameId { get; set; }

    public int DealNumber { get; set; }

    public DealStatus DealStatus { get; set; }

    public PlayerPosition? DealerPosition { get; set; }

    public required string DeckJson { get; set; }

    public string? UpCardJson { get; set; }

    public string? DiscardedCardJson { get; set; }

    public Suit? Trump { get; set; }

    public PlayerPosition? CallingPlayer { get; set; }

    public bool CallingPlayerIsGoingAlone { get; set; }

    public CallTrumpDecision? ChosenDecision { get; set; }

    public DealResult? DealResult { get; set; }

    public Team? WinningTeam { get; set; }

    public short Team1Score { get; set; }

    public short Team2Score { get; set; }

    public string? KnownPlayerSuitVoidsJson { get; set; }

    public required string PlayersJson { get; set; }

    public GameEntity? Game { get; set; }

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

        builder.Property(e => e.DealStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.DealerPosition)
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(e => e.DeckJson)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(e => e.UpCardJson)
            .HasMaxLength(50);

        builder.Property(e => e.DiscardedCardJson)
            .HasMaxLength(50);

        builder.Property(e => e.Trump)
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(e => e.CallingPlayer)
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(e => e.CallingPlayerIsGoingAlone)
            .IsRequired();

        builder.Property(e => e.ChosenDecision)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(e => e.DealResult)
            .HasConversion<string>()
            .HasMaxLength(25);

        builder.Property(e => e.WinningTeam)
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(e => e.Team1Score)
            .IsRequired();

        builder.Property(e => e.Team2Score)
            .IsRequired();

        builder.Property(e => e.KnownPlayerSuitVoidsJson)
            .HasMaxLength(600);

        builder.Property(e => e.PlayersJson)
            .IsRequired()
            .HasMaxLength(1500);

        builder.HasOne(e => e.Game)
            .WithMany(g => g.Deals)
            .HasForeignKey(e => e.GameId)
            .OnDelete(DeleteBehavior.Cascade);

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

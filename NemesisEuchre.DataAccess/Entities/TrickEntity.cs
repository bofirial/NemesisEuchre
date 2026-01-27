using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.GameEngine.Constants;

namespace NemesisEuchre.DataAccess.Entities;

public class TrickEntity
{
    public int TrickId { get; set; }

    public int DealId { get; set; }

    public int TrickNumber { get; set; }

    public PlayerPosition LeadPosition { get; set; }

    public Suit? LeadSuit { get; set; }

    public string CardsPlayedJson { get; set; } = null!;

    public PlayerPosition? WinningPosition { get; set; }

    public Team? WinningTeam { get; set; }

    public DealEntity Deal { get; set; } = null!;

    public List<PlayCardDecisionEntity> PlayCardDecisions { get; set; } = [];
}

public class TrickEntityConfiguration : IEntityTypeConfiguration<TrickEntity>
{
    public void Configure(EntityTypeBuilder<TrickEntity> builder)
    {
        builder.ToTable("Tricks");

        builder.HasKey(e => e.TrickId);

        builder.Property(e => e.TrickId)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.DealId)
            .IsRequired();

        builder.Property(e => e.TrickNumber)
            .IsRequired();

        builder.Property(e => e.LeadPosition)
            .IsRequired();

        builder.Property(e => e.CardsPlayedJson)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.LeadSuit);

        builder.Property(e => e.WinningPosition);

        builder.Property(e => e.WinningTeam);

        builder.HasOne(e => e.Deal)
            .WithMany(d => d.Tricks)
            .HasForeignKey(e => e.DealId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.DealId)
            .HasDatabaseName("IX_Tricks_DealId");
    }
}

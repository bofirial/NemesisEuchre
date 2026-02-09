using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.DataAccess.Entities.Metadata;

namespace NemesisEuchre.DataAccess.Entities;

public class TrickEntity
{
    public int TrickId { get; set; }

    public int DealId { get; set; }

    public int TrickNumber { get; set; }

    public int LeadPlayerPositionId { get; set; }

    public int? LeadSuitId { get; set; }

    public int? WinningPlayerPositionId { get; set; }

    public int? WinningTeamId { get; set; }

    public DealEntity? Deal { get; set; }

    public PlayerPositionMetadata? LeadPosition { get; set; }

    public SuitMetadata? LeadSuit { get; set; }

    public PlayerPositionMetadata? WinningPosition { get; set; }

    public TeamMetadata? WinningTeam { get; set; }

    public ICollection<TrickCardPlayed> TrickCardsPlayed { get; set; } = [];

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

        builder.Property(e => e.LeadPlayerPositionId)
            .IsRequired();

        builder.HasOne(e => e.Deal)
            .WithMany(d => d.Tricks)
            .HasForeignKey(e => e.DealId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.LeadPosition)
            .WithMany()
            .HasForeignKey(e => e.LeadPlayerPositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.LeadSuit)
            .WithMany()
            .HasForeignKey(e => e.LeadSuitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.WinningPosition)
            .WithMany()
            .HasForeignKey(e => e.WinningPlayerPositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.WinningTeam)
            .WithMany()
            .HasForeignKey(e => e.WinningTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.DealId)
            .HasDatabaseName("IX_Tricks_DealId");
    }
}

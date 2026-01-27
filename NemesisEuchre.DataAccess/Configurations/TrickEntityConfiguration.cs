using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.DataAccess.Entities;

namespace NemesisEuchre.DataAccess.Configurations;

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

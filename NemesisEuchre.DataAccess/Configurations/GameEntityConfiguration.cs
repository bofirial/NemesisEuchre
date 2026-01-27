using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.DataAccess.Entities;

namespace NemesisEuchre.DataAccess.Configurations;

public class GameEntityConfiguration : IEntityTypeConfiguration<GameEntity>
{
    public void Configure(EntityTypeBuilder<GameEntity> builder)
    {
        builder.ToTable("Games");

        builder.HasKey(e => e.GameId);

        builder.Property(e => e.GameId)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.GameStatus)
            .IsRequired();

        builder.Property(e => e.PlayersJson)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(e => e.Team1Score)
            .IsRequired();

        builder.Property(e => e.Team2Score)
            .IsRequired();

        builder.Property(e => e.WinningTeam);

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasMany(e => e.Deals)
            .WithOne(d => d.Game)
            .HasForeignKey(d => d.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("IX_Games_CreatedAt");

        builder.HasIndex(e => e.WinningTeam)
            .HasDatabaseName("IX_Games_WinningTeam");
    }
}

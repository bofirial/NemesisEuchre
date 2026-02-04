using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.DataAccess.Entities;

public class GameEntity
{
    public int GameId { get; set; }

    public GameStatus GameStatus { get; set; }

    public required string PlayersJson { get; set; }

    public short Team1Score { get; set; }

    public short Team2Score { get; set; }

    public Team? WinningTeam { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<DealEntity> Deals { get; set; } = [];
}

public class GameEntityConfiguration : IEntityTypeConfiguration<GameEntity>
{
    public void Configure(EntityTypeBuilder<GameEntity> builder)
    {
        builder.ToTable("Games");

        builder.HasKey(e => e.GameId);

        builder.Property(e => e.GameId)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.GameStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.PlayersJson)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(e => e.Team1Score)
            .IsRequired();

        builder.Property(e => e.Team2Score)
            .IsRequired();

        builder.Property(e => e.WinningTeam)
            .HasConversion<string>()
            .HasMaxLength(10);

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

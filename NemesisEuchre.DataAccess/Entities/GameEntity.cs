using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NemesisEuchre.DataAccess.Entities.Metadata;

namespace NemesisEuchre.DataAccess.Entities;

public class GameEntity
{
    public int GameId { get; set; }

    public int GameStatusId { get; set; }

    public short Team1Score { get; set; }

    public short Team2Score { get; set; }

    public int? WinningTeamId { get; set; }

    public DateTime CreatedAt { get; set; }

    public GameStatusMetadata? GameStatus { get; set; }

    public TeamMetadata? WinningTeam { get; set; }

    public ICollection<GamePlayer> GamePlayers { get; set; } = [];

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

        builder.Property(e => e.GameStatusId)
            .IsRequired();

        builder.Property(e => e.Team1Score)
            .IsRequired();

        builder.Property(e => e.Team2Score)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(e => e.GameStatus)
            .WithMany()
            .HasForeignKey(e => e.GameStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.WinningTeam)
            .WithMany()
            .HasForeignKey(e => e.WinningTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Deals)
            .WithOne(d => d.Game)
            .HasForeignKey(d => d.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("IX_Games_CreatedAt");

        builder.HasIndex(e => e.WinningTeamId)
            .HasDatabaseName("IX_Games_WinningTeamId");
    }
}

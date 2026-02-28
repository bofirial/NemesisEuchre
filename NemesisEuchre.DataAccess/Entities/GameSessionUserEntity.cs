using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NemesisEuchre.DataAccess.Entities;

public class GameSessionUserEntity : EntityBase
{
    public int GameSessionId { get; set; }

    public int UserId { get; set; }

    public DateTime JoinedDate { get; set; }

    public bool IsSessionLeader { get; set; }

    public GameSessionEntity? GameSession { get; set; }

    public UserEntity? User { get; set; }
}

public class GameSessionUserEntityConfiguration : IEntityTypeConfiguration<GameSessionUserEntity>
{
    public void Configure(EntityTypeBuilder<GameSessionUserEntity> builder)
    {
        builder.ToTable("GameSessionUsers");

        builder.HasKey(e => new { e.GameSessionId, e.UserId });

        builder.Property(e => e.JoinedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.IsSessionLeader).IsRequired().HasDefaultValue(false);

        builder.HasOne(e => e.GameSession)
            .WithMany(s => s.GameSessionUsers)
            .HasForeignKey(e => e.GameSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.User)
            .WithMany(u => u.GameSessionUsers)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

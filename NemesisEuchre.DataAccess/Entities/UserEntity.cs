using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NemesisEuchre.DataAccess.Entities;

public class UserEntity
{
    public int UserId { get; set; }

    public required string GitHubId { get; set; }

    public required string GitHubLogin { get; set; }

    public string? Email { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime LastSeenAt { get; set; }

    public ICollection<GameSessionUserEntity> GameSessionUsers { get; set; } = [];
}

public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(e => e.UserId);

        builder.Property(e => e.UserId)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.GitHubId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.GitHubLogin)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Email)
            .HasMaxLength(256);

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.LastSeenAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(e => e.GitHubId)
            .IsUnique()
            .HasDatabaseName("IX_Users_GitHubId");
    }
}

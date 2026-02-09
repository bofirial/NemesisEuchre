using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NemesisEuchre.DataAccess.Entities.Metadata;

public class PlayerPositionMetadata
{
    public int PlayerPositionId { get; set; }

    public required string Name { get; set; }
}

public class PlayerPositionMetadataConfiguration : IEntityTypeConfiguration<PlayerPositionMetadata>
{
    public void Configure(EntityTypeBuilder<PlayerPositionMetadata> builder)
    {
        builder.ToTable("PlayerPositions");

        builder.HasKey(e => e.PlayerPositionId);

        builder.Property(e => e.PlayerPositionId)
            .ValueGeneratedNever();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasData(
            new PlayerPositionMetadata { PlayerPositionId = 0, Name = "North" },
            new PlayerPositionMetadata { PlayerPositionId = 1, Name = "East" },
            new PlayerPositionMetadata { PlayerPositionId = 2, Name = "South" },
            new PlayerPositionMetadata { PlayerPositionId = 3, Name = "West" });
    }
}

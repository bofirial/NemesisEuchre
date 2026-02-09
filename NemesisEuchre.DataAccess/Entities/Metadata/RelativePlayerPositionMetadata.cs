using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NemesisEuchre.DataAccess.Entities.Metadata;

public class RelativePlayerPositionMetadata
{
    public int RelativePlayerPositionId { get; set; }

    public required string Name { get; set; }
}

public class RelativePlayerPositionMetadataConfiguration : IEntityTypeConfiguration<RelativePlayerPositionMetadata>
{
    public void Configure(EntityTypeBuilder<RelativePlayerPositionMetadata> builder)
    {
        builder.ToTable("RelativePlayerPositions");

        builder.HasKey(e => e.RelativePlayerPositionId);

        builder.Property(e => e.RelativePlayerPositionId)
            .ValueGeneratedNever();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(25);

        builder.HasData(
            new RelativePlayerPositionMetadata { RelativePlayerPositionId = 0, Name = "Self" },
            new RelativePlayerPositionMetadata { RelativePlayerPositionId = 1, Name = "LeftHandOpponent" },
            new RelativePlayerPositionMetadata { RelativePlayerPositionId = 2, Name = "Partner" },
            new RelativePlayerPositionMetadata { RelativePlayerPositionId = 3, Name = "RightHandOpponent" });
    }
}

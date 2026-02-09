using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NemesisEuchre.DataAccess.Entities.Metadata;

public class SuitMetadata
{
    public int SuitId { get; set; }

    public required string Name { get; set; }
}

public class SuitMetadataConfiguration : IEntityTypeConfiguration<SuitMetadata>
{
    public void Configure(EntityTypeBuilder<SuitMetadata> builder)
    {
        builder.ToTable("Suits");

        builder.HasKey(e => e.SuitId);

        builder.Property(e => e.SuitId)
            .ValueGeneratedNever();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasData(
            new SuitMetadata { SuitId = 1, Name = "Spades" },
            new SuitMetadata { SuitId = 2, Name = "Hearts" },
            new SuitMetadata { SuitId = 3, Name = "Clubs" },
            new SuitMetadata { SuitId = 4, Name = "Diamonds" });
    }
}

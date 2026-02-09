using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NemesisEuchre.DataAccess.Entities.Metadata;

public class RankMetadata
{
    public int RankId { get; set; }

    public required string Name { get; set; }
}

public class RankMetadataConfiguration : IEntityTypeConfiguration<RankMetadata>
{
    public void Configure(EntityTypeBuilder<RankMetadata> builder)
    {
        builder.ToTable("Ranks");

        builder.HasKey(e => e.RankId);

        builder.Property(e => e.RankId)
            .ValueGeneratedNever();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasData(
            new RankMetadata { RankId = 9, Name = "Nine" },
            new RankMetadata { RankId = 10, Name = "Ten" },
            new RankMetadata { RankId = 11, Name = "Jack" },
            new RankMetadata { RankId = 12, Name = "Queen" },
            new RankMetadata { RankId = 13, Name = "King" },
            new RankMetadata { RankId = 14, Name = "Ace" },
            new RankMetadata { RankId = 15, Name = "LeftBower" },
            new RankMetadata { RankId = 16, Name = "RightBower" });
    }
}

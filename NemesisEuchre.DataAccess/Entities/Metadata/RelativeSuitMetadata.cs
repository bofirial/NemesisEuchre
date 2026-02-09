using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NemesisEuchre.DataAccess.Entities.Metadata;

public class RelativeSuitMetadata
{
    public int RelativeSuitId { get; set; }

    public required string Name { get; set; }
}

public class RelativeSuitMetadataConfiguration : IEntityTypeConfiguration<RelativeSuitMetadata>
{
    public void Configure(EntityTypeBuilder<RelativeSuitMetadata> builder)
    {
        builder.ToTable("RelativeSuits");

        builder.HasKey(e => e.RelativeSuitId);

        builder.Property(e => e.RelativeSuitId)
            .ValueGeneratedNever();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(30);

        builder.HasData(
            new RelativeSuitMetadata { RelativeSuitId = 0, Name = "Trump" },
            new RelativeSuitMetadata { RelativeSuitId = 1, Name = "NonTrumpSameColor" },
            new RelativeSuitMetadata { RelativeSuitId = 2, Name = "NonTrumpOppositeColor1" },
            new RelativeSuitMetadata { RelativeSuitId = 3, Name = "NonTrumpOppositeColor2" });
    }
}

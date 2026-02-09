using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NemesisEuchre.DataAccess.Entities.Metadata;

public class DealResultMetadata
{
    public int DealResultId { get; set; }

    public required string Name { get; set; }
}

public class DealResultMetadataConfiguration : IEntityTypeConfiguration<DealResultMetadata>
{
    public void Configure(EntityTypeBuilder<DealResultMetadata> builder)
    {
        builder.ToTable("DealResults");

        builder.HasKey(e => e.DealResultId);

        builder.Property(e => e.DealResultId)
            .ValueGeneratedNever();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(25);

        builder.HasData(
            new DealResultMetadata { DealResultId = 1, Name = "WonStandardBid" },
            new DealResultMetadata { DealResultId = 2, Name = "WonGotAllTricks" },
            new DealResultMetadata { DealResultId = 3, Name = "OpponentsEuchred" },
            new DealResultMetadata { DealResultId = 4, Name = "WonAndWentAlone" },
            new DealResultMetadata { DealResultId = 5, Name = "ThrowIn" });
    }
}

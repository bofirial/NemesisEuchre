using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NemesisEuchre.DataAccess.Entities.Metadata;

public class DealStatusMetadata
{
    public int DealStatusId { get; set; }

    public required string Name { get; set; }
}

public class DealStatusMetadataConfiguration : IEntityTypeConfiguration<DealStatusMetadata>
{
    public void Configure(EntityTypeBuilder<DealStatusMetadata> builder)
    {
        builder.ToTable("DealStatuses");

        builder.HasKey(e => e.DealStatusId);

        builder.Property(e => e.DealStatusId)
            .ValueGeneratedNever();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasData(
            new DealStatusMetadata { DealStatusId = 0, Name = "NotStarted" },
            new DealStatusMetadata { DealStatusId = 1, Name = "SelectingTrump" },
            new DealStatusMetadata { DealStatusId = 2, Name = "Playing" },
            new DealStatusMetadata { DealStatusId = 3, Name = "Scoring" },
            new DealStatusMetadata { DealStatusId = 4, Name = "Complete" });
    }
}

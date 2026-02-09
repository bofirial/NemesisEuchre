using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NemesisEuchre.DataAccess.Entities.Metadata;

public class GameStatusMetadata
{
    public int GameStatusId { get; set; }

    public required string Name { get; set; }
}

public class GameStatusMetadataConfiguration : IEntityTypeConfiguration<GameStatusMetadata>
{
    public void Configure(EntityTypeBuilder<GameStatusMetadata> builder)
    {
        builder.ToTable("GameStatuses");

        builder.HasKey(e => e.GameStatusId);

        builder.Property(e => e.GameStatusId)
            .ValueGeneratedNever();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasData(
            new GameStatusMetadata { GameStatusId = 0, Name = "NotStarted" },
            new GameStatusMetadata { GameStatusId = 1, Name = "Playing" },
            new GameStatusMetadata { GameStatusId = 2, Name = "Complete" });
    }
}

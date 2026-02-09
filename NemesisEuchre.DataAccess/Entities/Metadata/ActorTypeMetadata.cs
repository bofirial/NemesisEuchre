using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NemesisEuchre.DataAccess.Entities.Metadata;

public class ActorTypeMetadata
{
    public int ActorTypeId { get; set; }

    public required string Name { get; set; }
}

public class ActorTypeMetadataConfiguration : IEntityTypeConfiguration<ActorTypeMetadata>
{
    public void Configure(EntityTypeBuilder<ActorTypeMetadata> builder)
    {
        builder.ToTable("ActorTypes");

        builder.HasKey(e => e.ActorTypeId);

        builder.Property(e => e.ActorTypeId)
            .ValueGeneratedNever();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasData(
            new ActorTypeMetadata { ActorTypeId = 0, Name = "User" },
            new ActorTypeMetadata { ActorTypeId = 1, Name = "Chaos" },
            new ActorTypeMetadata { ActorTypeId = 2, Name = "Chad" },
            new ActorTypeMetadata { ActorTypeId = 3, Name = "Beta" },
            new ActorTypeMetadata { ActorTypeId = 10, Name = "Gen1" },
            new ActorTypeMetadata { ActorTypeId = 11, Name = "Gen1Trainer" });
    }
}

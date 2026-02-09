using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NemesisEuchre.DataAccess.Entities.Metadata;

public class TeamMetadata
{
    public int TeamId { get; set; }

    public required string Name { get; set; }
}

public class TeamMetadataConfiguration : IEntityTypeConfiguration<TeamMetadata>
{
    public void Configure(EntityTypeBuilder<TeamMetadata> builder)
    {
        builder.ToTable("Teams");

        builder.HasKey(e => e.TeamId);

        builder.Property(e => e.TeamId)
            .ValueGeneratedNever();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasData(
            new TeamMetadata { TeamId = 0, Name = "Team1" },
            new TeamMetadata { TeamId = 1, Name = "Team2" });
    }
}

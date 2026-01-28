using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NemesisEuchre.DataAccess.Entities;

public class TrainingBatchEntity
{
    public int TrainingBatchId { get; set; }

    public int GenerationNumber { get; set; }

    public string? ModelVersion { get; set; }

    public string? ActorType { get; set; }

    public int? GameIdStart { get; set; }

    public int? GameIdEnd { get; set; }

    public int TotalDecisions { get; set; }

    public DateTime CreatedAt { get; set; }

    public GameEntity? GameStart { get; set; }

    public GameEntity? GameEnd { get; set; }
}

public class TrainingBatchEntityConfiguration : IEntityTypeConfiguration<TrainingBatchEntity>
{
    public void Configure(EntityTypeBuilder<TrainingBatchEntity> builder)
    {
        builder.ToTable("TrainingBatches");

        builder.HasKey(e => e.TrainingBatchId);

        builder.Property(e => e.TrainingBatchId)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.GenerationNumber)
            .IsRequired();

        builder.Property(e => e.ModelVersion)
            .HasMaxLength(50);

        builder.Property(e => e.ActorType)
            .HasMaxLength(25);

        builder.Property(e => e.TotalDecisions)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(e => e.GameStart)
            .WithMany()
            .HasForeignKey(e => e.GameIdStart)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(e => e.GameEnd)
            .WithMany()
            .HasForeignKey(e => e.GameIdEnd)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(e => e.GenerationNumber)
            .HasDatabaseName("IX_TrainingBatches_GenerationNumber");

        builder.HasIndex(e => e.ActorType)
            .HasDatabaseName("IX_TrainingBatches_ActorType");

        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("IX_TrainingBatches_CreatedAt");
    }
}

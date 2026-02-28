using Microsoft.EntityFrameworkCore;

using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.DataAccess.Entities.Metadata;

namespace NemesisEuchre.DataAccess;

public class NemesisEuchreDbContext(DbContextOptions<NemesisEuchreDbContext> options) : DbContext(options)
{
    public DbSet<SuitMetadata>? Suits { get; set; }

    public DbSet<RankMetadata>? Ranks { get; set; }

    public DbSet<CardMetadata>? Cards { get; set; }

    public DbSet<RelativeSuitMetadata>? RelativeSuits { get; set; }

    public DbSet<RelativeCardMetadata>? RelativeCards { get; set; }

    public DbSet<PlayerPositionMetadata>? PlayerPositions { get; set; }

    public DbSet<RelativePlayerPositionMetadata>? RelativePlayerPositions { get; set; }

    public DbSet<TeamMetadata>? Teams { get; set; }

    public DbSet<GameStatusMetadata>? GameStatuses { get; set; }

    public DbSet<DealStatusMetadata>? DealStatuses { get; set; }

    public DbSet<DealResultMetadata>? DealResults { get; set; }

    public DbSet<CallTrumpDecisionValueMetadata>? CallTrumpDecisionValues { get; set; }

    public DbSet<ActorTypeMetadata>? ActorTypes { get; set; }

    public DbSet<UserEntity>? Users { get; set; }

    public DbSet<GameSessionEntity>? GameSessions { get; set; }

    public DbSet<GameSessionUserEntity>? GameSessionUsers { get; set; }

    public DbSet<GameSessionConnectionEntity>? GameSessionConnections { get; set; }

    public DbSet<GameEntity>? Games { get; set; }

    public DbSet<GamePlayer>? GamePlayers { get; set; }

    public DbSet<DealEntity>? Deals { get; set; }

    public DbSet<DealDeckCard>? DealDeckCards { get; set; }

    public DbSet<DealPlayerEntity>? DealPlayers { get; set; }

    public DbSet<DealPlayerStartingHandCard>? DealPlayerStartingHandCards { get; set; }

    public DbSet<DealKnownPlayerSuitVoid>? DealKnownPlayerSuitVoids { get; set; }

    public DbSet<TrickEntity>? Tricks { get; set; }

    public DbSet<TrickCardPlayed>? TrickCardsPlayed { get; set; }

    public DbSet<CallTrumpDecisionEntity>? CallTrumpDecisions { get; set; }

    public DbSet<DiscardCardDecisionEntity>? DiscardCardDecisions { get; set; }

    public DbSet<PlayCardDecisionEntity>? PlayCardDecisions { get; set; }

    public DbSet<CallTrumpDecisionCardsInHand>? CallTrumpCardsInHand { get; set; }

    public DbSet<CallTrumpDecisionValidDecision>? CallTrumpValidDecisions { get; set; }

    public DbSet<CallTrumpDecisionPredictedPoints>? CallTrumpPredictedPoints { get; set; }

    public DbSet<DiscardCardDecisionCardsInHand>? DiscardCardsInHand { get; set; }

    public DbSet<DiscardCardDecisionPredictedPoints>? DiscardPredictedPoints { get; set; }

    public DbSet<PlayCardDecisionCardsInHand>? PlayCardCardsInHand { get; set; }

    public DbSet<PlayCardDecisionPlayedCard>? PlayCardPlayedCards { get; set; }

    public DbSet<PlayCardDecisionValidCard>? PlayCardValidCards { get; set; }

    public DbSet<PlayCardDecisionKnownVoid>? PlayCardKnownVoids { get; set; }

    public DbSet<PlayCardDecisionAccountedForCard>? PlayCardCardsAccountedFor { get; set; }

    public DbSet<PlayCardDecisionPredictedPoints>? PlayCardPredictedPoints { get; set; }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<EntityBase>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreateDate = now;
                entry.Entity.ModifyDate = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifyDate = now;
            }
        }

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NemesisEuchreDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(EntityBase).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(EntityBase.CreateDate))
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(EntityBase.ModifyDate))
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");
            }
        }
    }
}

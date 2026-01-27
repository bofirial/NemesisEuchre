using Microsoft.EntityFrameworkCore;

using NemesisEuchre.DataAccess.Entities;

namespace NemesisEuchre.DataAccess;

public class NemesisEuchreDbContext(DbContextOptions<NemesisEuchreDbContext> options) : DbContext(options)
{
    public DbSet<GameEntity>? Games { get; set; }

    public DbSet<DealEntity>? Deals { get; set; }

    public DbSet<TrickEntity>? Tricks { get; set; }

    public DbSet<CallTrumpDecisionEntity>? CallTrumpDecisions { get; set; }

    public DbSet<DiscardCardDecisionEntity>? DiscardCardDecisions { get; set; }

    public DbSet<PlayCardDecisionEntity>? PlayCardDecisions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NemesisEuchreDbContext).Assembly);
    }
}

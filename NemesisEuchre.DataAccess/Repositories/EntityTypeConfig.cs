using Microsoft.EntityFrameworkCore;

using NemesisEuchre.DataAccess.Entities;

namespace NemesisEuchre.DataAccess.Repositories;

public partial class TrainingDataRepository
{
    private interface IEntityTypeConfig;

    private sealed class EntityTypeConfig<TEntity>(
        Func<NemesisEuchreDbContext, DbSet<TEntity>> dbSetAccessor,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> includesApplicator) : IEntityTypeConfig
        where TEntity : class, IDecisionEntity
    {
        public DbSet<TEntity> GetDbSet(NemesisEuchreDbContext ctx)
        {
            return dbSetAccessor(ctx);
        }

        public IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> query)
        {
            return includesApplicator(query);
        }
    }
}

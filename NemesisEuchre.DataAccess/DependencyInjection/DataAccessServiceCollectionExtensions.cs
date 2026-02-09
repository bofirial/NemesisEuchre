using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NemesisEuchre.DataAccess.Mappers;
using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.DataAccess.Services;

namespace NemesisEuchre.DataAccess.DependencyInjection;

public static class DataAccessServiceCollectionExtensions
{
    public static IServiceCollection AddNemesisEuchreDataAccess(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("NemesisEuchreDb")
            ?? throw new InvalidOperationException("Connection string 'NemesisEuchreDb' not found.");

        services.AddDbContext<NemesisEuchreDbContext>(options => options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            sqlOptions.CommandTimeout(120);
        }));

        services.AddScoped<ITrickToEntityMapper, TrickToEntityMapper>();
        services.AddScoped<IDealToEntityMapper, DealToEntityMapper>();
        services.AddScoped<IGameToEntityMapper, GameToEntityMapper>();
        services.AddScoped<IEntityToTrickMapper, EntityToTrickMapper>();
        services.AddScoped<IEntityToDealMapper, EntityToDealMapper>();
        services.AddScoped<IEntityToGameMapper, EntityToGameMapper>();
        services.AddScoped<IBulkInsertService, BulkInsertService>();
        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<ITrainingDataRepository, TrainingDataRepository>();

        return services;
    }
}

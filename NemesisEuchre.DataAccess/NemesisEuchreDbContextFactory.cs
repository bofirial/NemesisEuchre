using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace NemesisEuchre.DataAccess;

public class NemesisEuchreDbContextFactory : IDesignTimeDbContextFactory<NemesisEuchreDbContext>
{
    public NemesisEuchreDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "NemesisEuchre.Console"))
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = configuration.GetConnectionString("NemesisEuchreDb");

        var optionsBuilder = new DbContextOptionsBuilder<NemesisEuchreDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new NemesisEuchreDbContext(optionsBuilder.Options);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NemesisEuchre.DataAccess;

public class NemesisEuchreDbContextFactory : IDesignTimeDbContextFactory<NemesisEuchreDbContext>
{
    public NemesisEuchreDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NemesisEuchreDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=NemesisEuchre;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new NemesisEuchreDbContext(optionsBuilder.Options);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AdaptiveRag.Data;

public class AdaptiveRagDbContextFactory : IDesignTimeDbContextFactory<AdaptiveRagDbContext>
{
    public AdaptiveRagDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AdaptiveRagDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=adaptive_rag_dev;Username=postgres;Password=postgres");
        return new AdaptiveRagDbContext(optionsBuilder.Options);
    }
}

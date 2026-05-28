using DAL.Data;
using DAL.Data.Initializer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace DAL;

public class ApplicationDbContextInitializer(AppDbContext context, IHostEnvironment environment)
{
    public async Task InitializeAsync()
    {
        await context.Database.MigrateAsync();
        
        if (environment.IsDevelopment() ||  environment.IsProduction())
        {
            await BogusDataSeeder.SeedTestDataAsync(context);
        }
    }
}
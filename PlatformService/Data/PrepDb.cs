using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlatformService.Models;
using System;
using System.Linq;

namespace PlatformService.Data;

public static class PrepDb
{
    public static void PrepPopulation(IApplicationBuilder app, bool isProd)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();

        SeedData(serviceScope.ServiceProvider.GetService<AppDbContext>(), 
            isProd,
            serviceScope.ServiceProvider.GetService<ILogger<Startup>>());
    }

    private static void SeedData(AppDbContext context, bool isProd, ILogger logger)
    {
        if (isProd)
        {
            logger.LogInformation("Attempting to apply migrations");
            try
            {
                context.Database.Migrate();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Could not run migrations");
            }

        }

        if (!context.Platforms.Any())
        {
            logger.LogInformation("Seeding Data...");

            var newPlatforms = new[]
            {
                new Platform() { Name = "Dot Net", Publisher = "Microsoft", Cost = "Free" },
                new Platform() { Name = "SQL Server Express", Publisher = "Microsoft", Cost = "Free" },
                new Platform() { Name = "Kubernetes", Publisher = "Cloud Native Computing Foundation", Cost = "Free" }
            };

            context.Platforms.AddRange(newPlatforms);

            context.SaveChanges();
            logger.LogInformation("Added {Count} Platforms", newPlatforms.Length);
        }
        else
        {
            logger.LogInformation("We already have data");
        }
    }
}

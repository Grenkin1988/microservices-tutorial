using CommandsService.Models;
using CommandsService.SyncDataServices.Grpc;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommandsService.Data;

public static class PrepDb
{
    public static void PrepPopulation(IApplicationBuilder app, bool isProd)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();

        var grpcClient = serviceScope.ServiceProvider.GetRequiredService<IPlatformDataClient>();
        var platforms = grpcClient.ReturnAllPlatforms();

        SeedData(
            serviceScope.ServiceProvider.GetService<AppDbContext>(),
            serviceScope.ServiceProvider.GetService<ICommandRepo>(),
            serviceScope.ServiceProvider.GetService<ILogger<Startup>>(),
            platforms,
            isProd);
    }

    private static void SeedData(AppDbContext context,
        ICommandRepo repo,
        ILogger logger,
        IEnumerable<Platform> platfroms,
        bool isProd)
    {
        if (isProd)
        {
            logger.LogInformation("Attempting to apply migrations...");
            try
            {
                context.Database.Migrate();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not run migrations");
            }

        }

        var newPlatforms = platfroms
            .Where(pl => !repo.ExternalPlatformExist(pl.ExternalId))
            .ToList();
        if (newPlatforms.Count != 0)
        {
            logger.LogInformation("Seeding new platforms...");

            newPlatforms.ForEach(pl => repo.CreatePlatform(pl));
            repo.SaveChanges();
            logger.LogInformation("Added {Count} Platforms", newPlatforms.Count);
        }
    }
}

using CommandsService.Models;
using CommandsService.SyncDataServices.Grpc;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
            platforms,
            isProd);
    }

    private static void SeedData(AppDbContext context,
        ICommandRepo repo,
        IEnumerable<Platform> platfroms,
        bool isProd)
    {
        if (isProd)
        {
            Console.WriteLine("--> Attempting to apply migrations...");
            try
            {
                context.Database.Migrate();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not run migrations: {ex.Message}");
            }

        }

        var newPlatfroms = platfroms
            .Where(pl => !repo.ExternalPlatformExist(pl.ExternalId))
            .ToList();
        if (newPlatfroms.Count != 0)
        {
            Console.WriteLine("--> Seeding new platforms...");

            newPlatfroms.ForEach(pl => repo.CreatePlatform(pl));
            repo.SaveChanges();
        }
    }
}

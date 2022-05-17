using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.SyncDataService.Grpc;
using System;
using System.IO;

namespace PlatformService;

public class Startup
{
    private readonly IWebHostEnvironment _env;

    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        Configuration = configuration;
        _env = env;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        if (_env.IsProduction())
        {
            var conStrBuilder = new SqlConnectionStringBuilder(
                Configuration["Platforms:ConnectionString"]);
            conStrBuilder.UserID = Configuration["Platforms:DbUserId"];
            conStrBuilder.Password = Configuration["Platforms:DbPassword"];
            services.AddDbContext<AppDbContext>(opt =>
                opt.UseSqlServer(conStrBuilder.ConnectionString));
        }
        else
        {
            services.AddDbContext<AppDbContext>(opt =>
                opt.UseInMemoryDatabase("InMemo"));
        }

        services.AddScoped<IPlatformRepo, PlatformRepo>();

        services.AddSingleton<IMessageBusClient, MessageBusClient>();
        services.AddGrpc();
        services.AddControllers();
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "PlatformService", Version = "v1" });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PlatformService v1"));
        }

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGrpcService<GrpcPlatformService>();

            endpoints.MapGet("/protos/platforms.proto", async context =>
            {
                await context.Response.WriteAsync(File.ReadAllText("Protos/platforms.proto"));
            });
        });

        logger.LogInformation("CommandService Endpoint {CommandServiceEndpoint}", Configuration["CommandService"]);

        PrepDb.PrepPopulation(app, env.IsProduction());
    }
}

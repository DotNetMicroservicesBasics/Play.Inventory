
using GreenPipes;
using Play.Common.Configuration;
using Play.Common.Data;
using Play.Common.HealthChecks;
using Play.Common.Identity;
using Play.Common.MassTansit;
using Play.Inventory.Api.Exceptions;
using Play.Inventory.Entities;

namespace Play.Inventory.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        if(builder.Environment.IsProduction()){
            builder.Configuration.ConfigureAzureKeyVault();
        }

        var allowedOriginsSettingsKey = "AllowedOrigins";

        // Add services to the container.

        builder.Services.AddMongoDb()
                        .AddMongoRepository<InventoryItem>("InventoryItems")
                        .AddMongoRepository<CatalogItem>("CatalogItems");

        builder.Services.AddJwtBearerAuthentication();

        builder.Services.AddMassTransitWithMesageBroker(builder.Configuration, retryConfigurator =>
        {
            retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
            retryConfigurator.Ignore(typeof(UnknownItemException));
        });

        builder.Services.AddControllers(options =>
        {
            ///To solve conflict happen on using nameOf(ControllerAction)
            options.SuppressAsyncSuffixInActionNames = false;
        });
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddHealthChecks()
                        .AddMongo();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseCors(corsBuilder =>
            {
                corsBuilder.WithOrigins(builder.Configuration[allowedOriginsSettingsKey])
                            .AllowAnyHeader()
                            .AllowAnyMethod();
            });
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllers();

        app.MapPlayEconomyHealthChecks();

        app.Run();
    }
}

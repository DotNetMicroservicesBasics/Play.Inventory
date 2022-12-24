using Play.Common.Data;
using Play.Common.MassTansit;
using Play.Inventory.Api.Clients;
using Play.Inventory.Data.Entities;
using Play.Common.Identity;
using Polly;
using Polly.Timeout;
using MassTransit;
using Play.Inventory.Api.Exceptions;
using GreenPipes;

namespace Play.Catalog.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var allowedOriginSettingsKey = "AllowedOrigins";

        // Add services to the container

        builder.Services.AddMongoDb()
                        .AddMongoRepository<InventoryItem>("InventoryItems")
                        .AddMongoRepository<CatalogItem>("CatalogItems");

        builder.Services.AddMassTransitWithRabbitMq(retryConfigurator =>
        {
            retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
            retryConfigurator.Ignore(typeof(UnknownItemException));
        });

        builder.Services.AddJwtBearerAuthentication();

        //AddCatalogClient(builder);

        builder.Services.AddControllers(options =>
        {
            ///To solve conflict happen on using nameOf(ControllerAction)
            options.SuppressAsyncSuffixInActionNames = false;
        });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseCors(corsBuilder =>
            {
                corsBuilder.WithOrigins(builder.Configuration[allowedOriginSettingsKey])
                            .AllowAnyHeader()
                            .AllowAnyMethod();
            });
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }

    private static void AddCatalogClient(WebApplicationBuilder builder)
    {
        Random jitterer = new Random();

        builder.Services.AddHttpClient<CatalogClient>(client =>
        {
            client.BaseAddress = new Uri("https://localhost:7040");
        })
        .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().WaitAndRetryAsync(
            5,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)),
            onRetry: (outcome, timespan, retryAttempt) =>
            {
                Console.WriteLine($"Delaying for {timespan.TotalSeconds} seconds, then making retry {retryAttempt}");
            }
        ))
        .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().CircuitBreakerAsync(
            3,
            TimeSpan.FromSeconds(15),
            onBreak: (outcome, timespan) =>
            {
                Console.WriteLine($"Opening the circuit for {timespan.TotalSeconds} seconds...");
            },
            onReset: () =>
            {
                Console.WriteLine($"Closing the circuit...");
            }
        ))
        .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));
    }
}
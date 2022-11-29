using Play.Common.Data;
using Play.Inventory.Data.Entities;

namespace Play.Catalog.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);


        // Add services to the container

        builder.Services.AddMongoDb()
                        .AddMongoRepository<InventoryItem>("InventoryItems");

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
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }


}
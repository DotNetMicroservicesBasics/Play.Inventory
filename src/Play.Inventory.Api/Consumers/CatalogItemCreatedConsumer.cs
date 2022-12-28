using MassTransit;
using Play.Catalog.Contracts.Dtos;
using Play.Common.Contracts.Interfaces;
using Play.Inventory.Entities;

namespace Play.Inventory.Api.Consumers
{
    public class CatalogItemCreatedConsumer : IConsumer<CatalogItemCreated>
    {

        private readonly IRepository<CatalogItem> _catalogItemRepository;

        public CatalogItemCreatedConsumer(IRepository<CatalogItem> catalogItemRepository)
        {
            _catalogItemRepository = catalogItemRepository;
        }

        public async Task Consume(ConsumeContext<CatalogItemCreated> context)
        {
            var messsage = context.Message;

            var item = await _catalogItemRepository.GetAsync(messsage.ItemId);

            if (item != null)
            {
                return;
            }

            item = new CatalogItem()
            {
                Id = messsage.ItemId,
                Name = messsage.Name,
                Description = messsage.Description
            };

            await _catalogItemRepository.CreateAsync(item);
        }
    }
}
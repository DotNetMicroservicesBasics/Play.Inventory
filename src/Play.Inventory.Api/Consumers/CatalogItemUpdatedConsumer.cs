using MassTransit;
using Play.Catalog.Contracts.Dtos;
using Play.Common.Contracts.Interfaces;
using Play.Inventory.Entities;

namespace Play.Inventory.Api.Consumers
{
    public class CatalogItemUpdatedConsumer : IConsumer<CatalogItemUpdated>
    {

        private readonly IRepository<CatalogItem> _catalogItemRepository;

        public CatalogItemUpdatedConsumer(IRepository<CatalogItem> catalogItemRepository)
        {
            _catalogItemRepository = catalogItemRepository;
        }

        public async Task Consume(ConsumeContext<CatalogItemUpdated> context)
        {
            var messsage = context.Message;

            var item = await _catalogItemRepository.GetAsync(messsage.ItemId);

            if (item == null)
            {
                item = new CatalogItem()
                {
                    Id = messsage.ItemId,
                    Name = messsage.Name,
                    Description = messsage.Description
                };

                await _catalogItemRepository.CreateAsync(item);
            }
            else
            {
                item.Name = messsage.Name;
                item.Description = messsage.Description;

                await _catalogItemRepository.UpdateAsync(item);
            }


        }
    }
}
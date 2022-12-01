using MassTransit;
using Play.Catalog.Contracts.Dtos;
using Play.Common.Contracts.Interfaces;
using Play.Inventory.Data.Entities;

namespace Play.Inventory.Api.Consumers
{
    public class CatalogItemCreatedUpdated : IConsumer<CatalogItemUpdated>
    {

        private readonly IRepository<CatalogItem> _catalogItemRepository;

        public CatalogItemCreatedUpdated(IRepository<CatalogItem> catalogItemRepository)
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
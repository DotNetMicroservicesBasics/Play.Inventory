using MassTransit;
using Play.Catalog.Contracts.Dtos;
using Play.Common.Contracts.Interfaces;
using Play.Inventory.Entities;

namespace Play.Inventory.Api.Consumers
{
    public class CatalogItemDeletedConsumer : IConsumer<CatalogItemDeleted>
    {

        private readonly IRepository<CatalogItem> _catalogItemRepository;

        public CatalogItemDeletedConsumer(IRepository<CatalogItem> catalogItemRepository)
        {
            _catalogItemRepository = catalogItemRepository;
        }

        public async Task Consume(ConsumeContext<CatalogItemDeleted> context)
        {
            var messsage = context.Message;

            var item = await _catalogItemRepository.GetAsync(messsage.ItemId);

            if (item == null)
            {
                return;
            }

            await _catalogItemRepository.DeleteAsync(item.Id);
        }
    }
}
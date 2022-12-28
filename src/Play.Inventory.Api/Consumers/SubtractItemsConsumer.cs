using MassTransit;
using Play.Common.Contracts.Interfaces;
using Play.Inventory.Api.Exceptions;
using Play.Inventory.Contracts;
using Play.Inventory.Entities;

namespace Play.Inventory.Api.Consumers
{
    public class SubtractItemsConsumer : IConsumer<SubtractItems>
    {
        private readonly IRepository<InventoryItem> _inventoryItemsRepository;
        private readonly IRepository<CatalogItem> _catalogItemsRepository;

        public SubtractItemsConsumer(IRepository<CatalogItem> catalogItemsRepository, IRepository<InventoryItem> inventoryItemsRepository)
        {
            _catalogItemsRepository = catalogItemsRepository;
            _inventoryItemsRepository = inventoryItemsRepository;
        }

        public async Task Consume(ConsumeContext<SubtractItems> context)
        {
            var messsage = context.Message;
            var item = await _catalogItemsRepository.GetAsync(messsage.CatalogItemId);
            if (item == null)
            {
                throw new UnknownItemException(messsage.CatalogItemId);
            }
            var inventoryItem = await _inventoryItemsRepository.GetAsync(item => item.UserId == messsage.UserId && item.CatalogItemId == messsage.CatalogItemId);
            if (inventoryItem == null)
            {
                return;
            }
            else
            {
                if (inventoryItem.MessageIds.Contains(context.MessageId.Value))
                {
                    await context.Publish(new InventoryItemsSubtracted(messsage.CorrelationId));
                    return;
                }
                inventoryItem.MessageIds.Add(context.MessageId.Value);
                inventoryItem.Quantity -= messsage.Quantity;
                if (inventoryItem.Quantity == 0)
                {
                    await _inventoryItemsRepository.DeleteAsync(inventoryItem.Id);
                }
                else
                {
                    await _inventoryItemsRepository.UpdateAsync(inventoryItem);
                }
                await context.Publish(new InventoryItemUpdated(messsage.UserId, messsage.CatalogItemId, inventoryItem.Quantity));

            }

            await context.Publish(new InventoryItemsSubtracted(messsage.CorrelationId));
        }
    }
}
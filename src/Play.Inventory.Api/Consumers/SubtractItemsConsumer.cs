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
            var message = context.Message;
            _logger.LogInformation("Receive Subtract Items Event of {Quantity} of item {ItemId} from user {UserId} with CorrelationId {CorrelationId}",
                        message.Quantity,
                        message.CatalogItemId, 
                        message.UserId, 
                        message.CorrelationId);
            var item = await _catalogItemsRepository.GetAsync(message.CatalogItemId);
            if (item == null)
            {
                throw new UnknownItemException(message.CatalogItemId);
            }
            var inventoryItem = await _inventoryItemsRepository.GetAsync(item => item.UserId == message.UserId && item.CatalogItemId == message.CatalogItemId);
            if (inventoryItem == null)
            {
                return;
            }
            else
            {
                if (inventoryItem.MessageIds.Contains(context.MessageId.Value))
                {
                    await context.Publish(new InventoryItemsSubtracted(message.CorrelationId));
                    return;
                }
                inventoryItem.MessageIds.Add(context.MessageId.Value);
                inventoryItem.Quantity -= message.Quantity;
                if (inventoryItem.Quantity == 0)
                {
                    await _inventoryItemsRepository.DeleteAsync(inventoryItem.Id);
                }
                else
                {
                    await _inventoryItemsRepository.UpdateAsync(inventoryItem);
                }
                await context.Publish(new InventoryItemUpdated(message.UserId, message.CatalogItemId, inventoryItem.Quantity));

            }

            await context.Publish(new InventoryItemsSubtracted(message.CorrelationId));
        }
    }
}
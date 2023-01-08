using MassTransit;
using Play.Common.Contracts.Interfaces;
using Play.Inventory.Api.Exceptions;
using Play.Inventory.Contracts;
using Play.Inventory.Entities;

namespace Play.Inventory.Api.Consumers
{
    public class GrantItemsConsumer : IConsumer<GrantItems>
    {
        private readonly IRepository<InventoryItem> _inventoryItemsRepository;
        private readonly IRepository<CatalogItem> _catalogItemsRepository;
        private readonly ILogger<GrantItemsConsumer> _logger;

        public GrantItemsConsumer(IRepository<CatalogItem> catalogItemsRepository, IRepository<InventoryItem> inventoryItemsRepository, ILogger<GrantItemsConsumer> logger)
        {
            _catalogItemsRepository = catalogItemsRepository;
            _inventoryItemsRepository = inventoryItemsRepository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<GrantItems> context)
        {
            var message = context.Message;
            _logger.LogInformation("Receive Grant Items Event of {Quantity} of item {ItemId} from user {UserId} with CorrelationId {CorrelationId}",
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
                inventoryItem = new InventoryItem()
                {
                    UserId = message.UserId,
                    CatalogItemId = message.CatalogItemId,
                    Quantity = message.Quantity,
                    AcquiredDate = DateTimeOffset.UtcNow
                };
                inventoryItem.MessageIds.Add(context.MessageId.Value);
                await _inventoryItemsRepository.CreateAsync(inventoryItem);
            }
            else if (!inventoryItem.MessageIds.Contains(context.MessageId.Value))
            {
                inventoryItem.MessageIds.Add(context.MessageId.Value);
                inventoryItem.Quantity += message.Quantity;
                await _inventoryItemsRepository.UpdateAsync(inventoryItem);
            }

            var itemGrantedTask = context.Publish(new InventoryItemsGranted(message.CorrelationId));
            var itemUpdatedTask = context.Publish(new InventoryItemUpdated(message.UserId, message.CatalogItemId, inventoryItem.Quantity));

            Task.WaitAll(itemGrantedTask, itemUpdatedTask);
        }
    }
}
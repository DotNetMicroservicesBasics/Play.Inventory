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
        public GrantItemsConsumer(IRepository<CatalogItem> catalogItemsRepository, IRepository<InventoryItem> inventoryItemsRepository)
        {
            _catalogItemsRepository = catalogItemsRepository;
            _inventoryItemsRepository = inventoryItemsRepository;
        }

        public async Task Consume(ConsumeContext<GrantItems> context)
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
                inventoryItem = new InventoryItem()
                {
                    UserId = messsage.UserId,
                    CatalogItemId = messsage.CatalogItemId,
                    Quantity = messsage.Quantity,
                    AcquiredDate = DateTimeOffset.UtcNow
                };
                inventoryItem.MessageIds.Add(context.MessageId.Value);
                await _inventoryItemsRepository.CreateAsync(inventoryItem);
            }
            else if (!inventoryItem.MessageIds.Contains(context.MessageId.Value))
            {
                inventoryItem.MessageIds.Add(context.MessageId.Value);
                inventoryItem.Quantity += messsage.Quantity;
                await _inventoryItemsRepository.UpdateAsync(inventoryItem);
            }

            var itemGrantedTask = context.Publish(new InventoryItemsGranted(messsage.CorrelationId));
            var itemUpdatedTask = context.Publish(new InventoryItemUpdated(messsage.UserId, messsage.CatalogItemId, inventoryItem.Quantity));

            Task.WaitAll(itemGrantedTask, itemUpdatedTask);
        }
    }
}
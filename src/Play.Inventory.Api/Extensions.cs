using Play.Inventory.Contracts.Dtos;
using Play.Inventory.Data.Entities;

namespace Play.Inventory.Api
{
    public static class Extensions
    {
        public static InventoryItemDto AsDto(this InventoryItem inventoryItem)
        {
            return new InventoryItemDto(inventoryItem.CatalogItemId, inventoryItem.Quantity, inventoryItem.AcquiredDate);
        }
    }
}
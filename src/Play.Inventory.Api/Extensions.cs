using Play.Inventory.Dtos;
using Play.Inventory.Entities;

namespace Play.Inventory.Api
{
    public static class Extensions
    {
        public static InventoryItemDto AsDto(this InventoryItem inventoryItem, string name, string description)
        {
            return new InventoryItemDto(inventoryItem.CatalogItemId, name, description, inventoryItem.Quantity, inventoryItem.AcquiredDate);
        }
    }
}
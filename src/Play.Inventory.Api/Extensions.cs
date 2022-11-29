using Play.Inventory.Contracts.Dtos;
using Play.Inventory.Data.Entities;

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
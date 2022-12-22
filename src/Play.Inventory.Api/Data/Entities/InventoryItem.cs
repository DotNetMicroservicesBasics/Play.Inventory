using Play.Common.Contracts.Interfaces;

namespace Play.Inventory.Data.Entities
{
    public class InventoryItem : IEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid CatalogItemId { get; set; }
        public int Quantity { get; set; }
        public HashSet<Guid> MessageIds { get; set; } = new HashSet<Guid>();
        public DateTimeOffset AcquiredDate { get; set; }
    }
}
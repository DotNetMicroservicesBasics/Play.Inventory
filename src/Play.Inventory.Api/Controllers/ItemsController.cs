using Microsoft.AspNetCore.Mvc;
using Play.Common.Contracts.Interfaces;
using Play.Inventory.Api.Clients;
using Play.Inventory.Contracts.Dtos;
using Play.Inventory.Data.Entities;

namespace Play.Inventory.Api.Controllers
{
    [ApiController]
    [Route("Items")]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<InventoryItem> _itemsRepository;
        private readonly CatalogClient _catalogClient;

        public ItemsController(IRepository<InventoryItem> itemsRepository, CatalogClient catalogClient)
        {
            _itemsRepository = itemsRepository;
            _catalogClient = catalogClient;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest();
            }

            var catalogItems = await _catalogClient.GetCatalogItemsAsync();
            var inventoryEntities = await _itemsRepository.GetAllAsync(item => item.UserId == userId);
            var inventoryItems = inventoryEntities.Select(item =>
            {
                var catalogItem = catalogItems.Single(i => i.Id == item.CatalogItemId);
                return item.AsDto(catalogItem.Name, catalogItem.Description);
            });

            //var items = (await _itemsRepository.GetAllAsync(item => item.UserId == userId)).Select(item => item.AsDto());
            return Ok(inventoryItems);
        }

        [HttpPost]
        public async Task<ActionResult> PostAsync(GrantItemsDto grantItemsDto)
        {
            var inventoryItem = await _itemsRepository.GetAsync(item => item.UserId == grantItemsDto.UserId && item.CatalogItemId == grantItemsDto.CatalogItemId);
            if (inventoryItem == null)
            {
                inventoryItem = new InventoryItem()
                {
                    UserId = grantItemsDto.UserId,
                    CatalogItemId = grantItemsDto.CatalogItemId,
                    Quantity = grantItemsDto.Quantity,
                    AcquiredDate = DateTimeOffset.UtcNow
                };
                await _itemsRepository.CreateAsync(inventoryItem);
            }
            else
            {
                inventoryItem.Quantity += grantItemsDto.Quantity;
                await _itemsRepository.UpdateAsync(inventoryItem);
            }
            return Ok();
        }
    }
}
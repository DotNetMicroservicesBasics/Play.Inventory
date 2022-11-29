using Microsoft.AspNetCore.Mvc;
using Play.Common.Contracts.Interfaces;
using Play.Inventory.Contracts.Dtos;
using Play.Inventory.Data.Entities;

namespace Play.Inventory.Api.Controllers
{
    [ApiController]
    [Route("Items")]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<InventoryItem> _itemsRepository;

        public ItemsController(IRepository<InventoryItem> itemsRepository)
        {
            _itemsRepository = itemsRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest();
            }

            var items = (await _itemsRepository.GetAllAsync(item => item.UserId == userId)).Select(item => item.AsDto());
            return Ok(items);
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
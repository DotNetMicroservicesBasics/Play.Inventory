using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Play.Common.Contracts.Interfaces;
using Play.Inventory.Api.Clients;
using Play.Inventory.Api.Consts;
using Play.Inventory.Contracts;
using Play.Inventory.Contracts.Dtos;
using Play.Inventory.Data.Entities;

namespace Play.Inventory.Api.Controllers
{
    [ApiController]
    [Route("Items")]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<InventoryItem> _inventoryItemsRepository;
        private readonly IRepository<CatalogItem> _catalogItemsRepository;

        private readonly IPublishEndpoint _publishEndpoint;

        public ItemsController(IRepository<InventoryItem> inventoryItemsRepository, IRepository<CatalogItem> catalogItemsRepository, IPublishEndpoint publishEndpoint)
        {
            _inventoryItemsRepository = inventoryItemsRepository;
            _catalogItemsRepository = catalogItemsRepository;
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest();
            }

            var currentUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (Guid.Parse(currentUserId) != userId)
            {
                if (!User.IsInRole(Roles.Admin))
                {
                    return Forbid();
                }
            }

            var inventoryEntities = await _inventoryItemsRepository.GetAllAsync(inverntoryItem => inverntoryItem.UserId == userId);
            var itemsIds = inventoryEntities.Select(inverntoryItem => inverntoryItem.CatalogItemId);
            var catalogItems = await _catalogItemsRepository.GetAllAsync(catalogItem => itemsIds.Contains(catalogItem.Id));

            var inventoryItems = inventoryEntities.Select(item =>
            {
                var catalogItem = catalogItems.Single(catalogItem => catalogItem.Id == item.CatalogItemId);
                return item.AsDto(catalogItem.Name, catalogItem.Description);
            });

            //var items = (await _itemsRepository.GetAllAsync(item => item.UserId == userId)).Select(item => item.AsDto());
            return Ok(inventoryItems);
        }

        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> PostAsync(GrantItemsDto grantItemsDto)
        {
            var inventoryItem = await _inventoryItemsRepository.GetAsync(item => item.UserId == grantItemsDto.UserId && item.CatalogItemId == grantItemsDto.CatalogItemId);
            if (inventoryItem == null)
            {
                inventoryItem = new InventoryItem()
                {
                    UserId = grantItemsDto.UserId,
                    CatalogItemId = grantItemsDto.CatalogItemId,
                    Quantity = grantItemsDto.Quantity,
                    AcquiredDate = DateTimeOffset.UtcNow
                };
                await _inventoryItemsRepository.CreateAsync(inventoryItem);
            }
            else
            {
                inventoryItem.Quantity += grantItemsDto.Quantity;
                await _inventoryItemsRepository.UpdateAsync(inventoryItem);
            }

            await _publishEndpoint.Publish(new InventoryItemUpdated(inventoryItem.UserId, inventoryItem.CatalogItemId, inventoryItem.Quantity));

            return Ok();
        }
    }
}
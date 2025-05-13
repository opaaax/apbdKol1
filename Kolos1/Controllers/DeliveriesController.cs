using System.ComponentModel.DataAnnotations;
using Kolos1.Exceptions;
using Kolos1.Models.DTOs;
using Kolos1.Services;
using Microsoft.AspNetCore.Mvc;

namespace Kolos1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DeliveriesController : ControllerBase
{
    public readonly IDeliveryService _deliveryService;

    public DeliveriesController(IDeliveryService deliveryService)
    {
        _deliveryService = deliveryService;
    }

    [HttpGet("{deliveryId:int}")]
    public async Task<IActionResult> GetDeliveriesByIdAsync(int deliveryId)
    {
        try
        {
            var delivery = await _deliveryService.GetDeliveryByIdAsync(deliveryId);
            return Ok(delivery);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> PostDeliveryAsync(PostRequestDeliveryDTO delivery)
    {
        try
        {
            await _deliveryService.PostDeliveryAsync(delivery);
            return Created();
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (AlreadyPresentException e)
        {
            return Conflict(e.Message);
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Message);
        }
    }
    
}
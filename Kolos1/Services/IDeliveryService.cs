using Kolos1.Models.DTOs;

namespace Kolos1.Services;

public interface IDeliveryService
{
    Task<GetRequestDeliveryDTO> GetDeliveryByIdAsync(int deliveryId);
    Task PostDeliveryAsync(PostRequestDeliveryDTO postRequestDelivery);
}
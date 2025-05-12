using WebApplication1.Models.DTOs;

namespace WebApplication1.Services;

public interface IClientService
{
    Task<ClientDTO> GetClientAsync(int id);
    Task AddNewRentalAsync(int customerId, CreateRentalRequestDTO rentalRequest);
}
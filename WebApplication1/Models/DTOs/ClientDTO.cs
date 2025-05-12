using System.Collections;

namespace WebApplication1.Models.DTOs;

public class ClientDTO
{
    public string firstName { get; set; }
    public string lastName { get; set; }
    public List<RentalDTO> rentals { get; set; }
}
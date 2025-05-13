namespace Kolos1.Models.DTOs;

public class GetRequestDeliveryDTO
{
    public DateTime Date { get; set; }
    public CustomerDTO Customer { get; set; }
    public DriverDTO Driver { get; set; }
    public List<GetProductDTO> Products { get; set; }
}
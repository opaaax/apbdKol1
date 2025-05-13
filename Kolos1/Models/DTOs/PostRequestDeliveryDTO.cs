using System.ComponentModel.DataAnnotations;

namespace Kolos1.Models.DTOs;

public class PostRequestDeliveryDTO
{
    [Required]
    public int DeliveryId { get; set; }
    [Required] 
    public int CustomerId { get; set; }
    [Required, StringLength(17)]
    public string LicenceNumber { get; set; }
    [Required]
    public List<PostProductDTO> Products { get; set; }
}
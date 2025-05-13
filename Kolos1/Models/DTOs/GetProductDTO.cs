using System.ComponentModel.DataAnnotations;

namespace Kolos1.Models.DTOs;

public class GetProductDTO
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Amount { get; set; }
}
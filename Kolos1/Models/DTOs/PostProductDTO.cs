using System.ComponentModel.DataAnnotations;

namespace Kolos1.Models.DTOs;

public class PostProductDTO
{
    [Required, StringLength(100)]
    public string Name { get; set; }
    [Required]
    public int Amount { get; set; }
}
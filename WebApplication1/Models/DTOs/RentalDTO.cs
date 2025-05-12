namespace WebApplication1.Models.DTOs;

public class RentalDTO
{
    public int RentalId { get; set; }
    public DateTime RentalDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string Status { get; set; }
    public List<MovieDTO> Movies { get; set; }
}
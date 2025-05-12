namespace WebApplication1.Models.DTOs;

public class CreateRentalRequestDTO
{
    public int Id { get; set; }
    public DateTime RentalDate { get; set; }
    public List<RentedMovieInputDto> Movies { get; set; } = new List<RentedMovieInputDto>();
}

public class RentedMovieInputDto
{
    public string Title { get; set; } = string.Empty;
    public decimal RentalPrice { get; set; }
}
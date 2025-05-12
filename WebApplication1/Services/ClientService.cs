using System.Data.Common;
using Microsoft.Data.SqlClient;
using WebApplication1.Exceptions;
using WebApplication1.Models.DTOs;

namespace WebApplication1.Services;

public class ClientService : IClientService
{
    private readonly string? _connectionString;

    public ClientService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default");
    }
    
    
    public async Task<ClientDTO> GetClientAsync(int id)
    {
        ClientDTO clientDTO = null;
        MovieDTO movieDTO = null;
        RentalDTO rentalDTO = null;

        string command = "SELECT" +
                         " first_name, last_name, rental.rental_id, rental.rental_date, rental.return_date, status.name, rental_item.price_at_rental, movie.title " +
                         "FROM Customer " +
                         "JOIN Rental rental ON customer.customer_id = rental.Customer_id " +
                         "JOIN Status status ON rental.status_id = status.status_id " +
                         "JOIN Rental_Item rental_item ON rental_item.rental_id = rental.rental_id " +
                         "JOIN Movie movie ON movie.movie_id = rental_item.movie_id WHERE Customer.customer_id = @id";
        
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand cmd = new SqlCommand();
        
        cmd.CommandText = command;
        cmd.Connection = connection;
        await connection.OpenAsync();
        
        cmd.Parameters.AddWithValue("@id", id);
        var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            if (clientDTO is null)
            {
                clientDTO = new ClientDTO
                {
                    firstName = reader.GetString(0),
                    lastName = reader.GetString(1),
                    rentals = new List<RentalDTO>()
                };
            }
            
            int rentalId = reader.GetInt32(2);
            
            var rental = clientDTO.rentals.FirstOrDefault(e => e.RentalId.Equals(rentalId));
            if (rental is null)
            {
                rental = new RentalDTO()
                {
                    RentalId = rentalId,
                    RentalDate = reader.GetDateTime(3),
                    ReturnDate = await reader.IsDBNullAsync(4) ? null : reader.GetDateTime(4),
                    Status = reader.GetString(5),
                    Movies = new List<MovieDTO>()
                };
                clientDTO.rentals.Add(rental);
            } 
            rental.Movies.Add(new MovieDTO()
            {
                Title = reader.GetString(7),
                PriceAtRental = reader.GetDecimal(6),
            });
        }

        if (clientDTO is null)
        {
            throw new NotFoundException("No client found");
        }
        
        return clientDTO;
    }

    public async Task AddNewRentalAsync(int customerId, CreateRentalRequestDTO rentalRequest)
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        await connection.OpenAsync();

        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            command.Parameters.Clear();
            command.CommandText = "SELECT 1 FROM Customer WHERE customer_id = @IdCustomer;";
            command.Parameters.AddWithValue("@IdCustomer", customerId);

            var customerIdRes = await command.ExecuteScalarAsync();
            if (customerIdRes is null)
                throw new NotFoundException($"Customer with ID - {customerId} - not found.");

            command.Parameters.Clear();
            command.CommandText =
                @"INSERT INTO Rental
            VALUES(@IdRental, @RentalDate, @ReturnDate, @CustomerId, @StatusId);";

            command.Parameters.AddWithValue("@IdRental", rentalRequest.Id);
            command.Parameters.AddWithValue("@RentalDate", rentalRequest.RentalDate);
            command.Parameters.AddWithValue("@ReturnDate", DBNull.Value);
            command.Parameters.AddWithValue("@CustomerId", customerId);
            command.Parameters.AddWithValue("@StatusId", 1);

            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                throw new ConflictException("A rental with the same ID already exists.");
            }


            foreach (var movie in rentalRequest.Movies)
            {
                command.Parameters.Clear();
                command.CommandText = "SELECT movie_id FROM Movie WHERE Title = @MovieTitle;";
                command.Parameters.AddWithValue("@MovieTitle", movie.Title);

                var movieId = await command.ExecuteScalarAsync();
                if (movieId is null)
                    throw new NotFoundException($"Movie - {movie.Title} - not found.");

                command.Parameters.Clear();
                command.CommandText =
                    @"INSERT INTO Rental_Item
                        VALUES(@IdRental, @MovieId, @RentalPrice);";

                command.Parameters.AddWithValue("@IdRental", rentalRequest.Id);
                command.Parameters.AddWithValue("@MovieId", movieId);
                command.Parameters.AddWithValue("@RentalPrice", movie.RentalPrice);

                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
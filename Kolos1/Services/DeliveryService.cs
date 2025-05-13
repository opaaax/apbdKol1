using System.Data.Common;
using Kolos1.Exceptions;
using Kolos1.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace Kolos1.Services;

public class DeliveryService : IDeliveryService
{
    private readonly string? _connectionString;
    public DeliveryService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default");
    }
    
    public async Task<GetRequestDeliveryDTO> GetDeliveryByIdAsync(int deliveryId)
    {
        GetRequestDeliveryDTO getRequestDeliveryDto = null;
        CustomerDTO customerDto = null;
        DriverDTO driverDto = null;

        string command = "SELECT" +
                         " delivery.date," +
                         " customer.first_name, customer.last_name, customer.date_of_birth," +
                         " driver.first_name, driver.last_name, driver.licence_number," +
                         " productDelivery.amount, product.name, product.price " +
                         "FROM DELIVERY delivery " +
                         "JOIN CUSTOMER customer ON delivery.customer_id = customer.customer_id " + 
                         "JOIN DRIVER driver ON delivery.driver_id = driver.driver_id " +
                         "JOIN PRODUCT_DELIVERY productDelivery ON delivery.delivery_id = productDelivery.delivery_id " + 
                         "JOIN PRODUCT product ON product.product_id = productDelivery.product_id " + 
                         "WHERE delivery.delivery_id = @deliveryId";
        
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand cmd = new SqlCommand();
        
        cmd.CommandText = command;
        cmd.Connection = connection;
        await connection.OpenAsync();
        
        cmd.Parameters.AddWithValue("@deliveryId", deliveryId);
        var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            if (getRequestDeliveryDto is null)
            {
                getRequestDeliveryDto = new GetRequestDeliveryDTO();
                getRequestDeliveryDto.Date = reader.GetDateTime(0);
                getRequestDeliveryDto.Products = new List<GetProductDTO>();
            }

            if (customerDto is null)
            {
                customerDto = new CustomerDTO();
                customerDto.FirstName = reader.GetString(1);
                customerDto.LastName = reader.GetString(2);
                customerDto.DateOfBirth = reader.GetDateTime(3);
                getRequestDeliveryDto.Customer = customerDto;
            }

            if (driverDto is null)
            {
                driverDto = new DriverDTO();
                driverDto.FirstName = reader.GetString(4);
                driverDto.LastName = reader.GetString(5);
                driverDto.LicenceNumber = reader.GetString(6);
            }
            string productName = reader.GetString(8);
            var product = getRequestDeliveryDto.Products.FirstOrDefault(p => p.Name == productName);
            if (product is null)
            {
                product = new GetProductDTO();
                product.Name = productName;
                product.Price = reader.GetDecimal(9);
                product.Amount = reader.GetInt32(7);
            }
            getRequestDeliveryDto.Products.Add(product);
        }

        if (getRequestDeliveryDto is null)
        {
            throw new NotFoundException("Delivery with given id: " + deliveryId + " does not exist");
        }
        
        return getRequestDeliveryDto;
    }

    public async Task PostDeliveryAsync(PostRequestDeliveryDTO postRequestDelivery)
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
            command.CommandText = "SELECT 1 FROM DELIVERY WHERE delivery_id = @deliveryId";
            command.Parameters.AddWithValue("@deliveryId", postRequestDelivery.DeliveryId);
            
            var deliverySelectOutput = await command.ExecuteScalarAsync();
            if (deliverySelectOutput is not null)
            {
                throw new AlreadyPresentException("Delivery with given id: " + postRequestDelivery.DeliveryId + " already exists");
            }
            
            command.Parameters.Clear();
            command.CommandText = "SELECT 1 FROM CUSTOMER WHERE customer_id = @customerId";
            command.Parameters.AddWithValue("@customerId", postRequestDelivery.CustomerId);
            
            var customerSelectOutput = await command.ExecuteScalarAsync();
            if (customerSelectOutput is null)
            {
                throw new NotFoundException("Customer with given id: " + postRequestDelivery.CustomerId + " does not exist");
            }
            
            command.Parameters.Clear();
            command.CommandText = "SELECT driver_id FROM DRIVER WHERE licence_number = @licenceNumber";
            command.Parameters.AddWithValue("@licenceNumber", postRequestDelivery.LicenceNumber);
            
            var licenseNumberSelectOutput = await command.ExecuteScalarAsync();
            if (licenseNumberSelectOutput is null)
            {
                throw new NotFoundException("Driver with given licence number: " + postRequestDelivery.LicenceNumber + " does not exist");
            }
            
            command.Parameters.Clear();
            command.CommandText = "INSERT INTO Delivery(delivery_id, customer_id, driver_id, date) VALUES (@deliveryId, @customerId, @driverId, @date)";
            command.Parameters.AddWithValue("@deliveryId", postRequestDelivery.DeliveryId);
            command.Parameters.AddWithValue("@customerId", postRequestDelivery.CustomerId);
            command.Parameters.AddWithValue("@driverId",(int) licenseNumberSelectOutput);
            command.Parameters.AddWithValue("@date", DateTime.Now);
            await command.ExecuteNonQueryAsync();
            
            foreach (var product in postRequestDelivery.Products)
            {
                command.Parameters.Clear();
                command.CommandText = "SELECT product_id FROM PRODUCT WHERE name = @name";
                command.Parameters.AddWithValue("@name", product.Name);
                
                var productSelectOutput = await command.ExecuteScalarAsync();
                if (productSelectOutput is null)
                {
                    throw new NotFoundException("Product with given name: " + product.Name + " does not exist");
                }
                
                command.Parameters.Clear();
                command.CommandText = "INSERT INTO PRODUCT_DELIVERY(product_id, delivery_id, amount) VALUES (@productId, @deliveryId, @amount)";
                command.Parameters.AddWithValue("@productId", (int) productSelectOutput);
                command.Parameters.AddWithValue("@deliveryId", postRequestDelivery.DeliveryId);
                command.Parameters.AddWithValue("@amount", product.Amount);
                await command.ExecuteNonQueryAsync();
            }
            
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
using Azure.Data.Tables;
using ST10257937cldv.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace ST10257937cldv.Services
{
    public class TableService
    {
        private readonly TableClient _tableClient;
        private readonly IConfiguration _configuration;

        // Constructor
        public TableService(IConfiguration configuration)
        {
            var connectionString = configuration["AzureStorage:ConnectionString"];
            var serviceClient = new TableServiceClient(connectionString);
            _tableClient = serviceClient.GetTableClient("CustomerProfiles");
            _tableClient.CreateIfNotExists();
            _configuration = configuration;
        }

        // Method to add customer profile to Table Storage
        public async Task AddEntityAsync(CustomerProfile profile)
        {
            await _tableClient.AddEntityAsync(profile);
        }

        // Method to add customer profile to SQL Database
        public async Task AddProfileToSqlAsync(CustomerProfile profile)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var query = @"INSERT INTO CustomerProfiles (RowKey, FirstName, LastName, Email, PhoneNumber)
                          VALUES (@RowKey, @FirstName, @LastName, @Email, @PhoneNumber)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RowKey", profile.RowKey);
                command.Parameters.AddWithValue("@FirstName", profile.FirstName);
                command.Parameters.AddWithValue("@LastName", profile.LastName);
                command.Parameters.AddWithValue("@Email", profile.Email);
                command.Parameters.AddWithValue("@PhoneNumber", profile.PhoneNumber);

                connection.Open();
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}

using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace ST10257937cldv.Services
{
    public class BlobService
    {
        private readonly IConfiguration _configuration;

        public BlobService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Upload blob to Azure Blob Storage
        public async Task UploadBlobAsync(string containerName, string blobName, Stream content)
        {
            var blobServiceClient = new BlobServiceClient(_configuration["AzureStorage:ConnectionString"]);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(content, overwrite: true);
        }

        // Insert image data as a blob into SQL
        public async Task InsertBlobAsync(byte[] imageData)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var query = @"INSERT INTO BlobTable (BlobImage) VALUES (@BlobImage)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@BlobImage", imageData);

                connection.Open();
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}

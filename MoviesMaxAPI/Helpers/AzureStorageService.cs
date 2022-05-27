using Azure.Storage.Blobs;

namespace MoviesMaxAPI.Helpers
{
    //a container in Azure Storage is like a folder that allows us group certain files together
    public class AzureStorageService : IFileStorageService
    {
        private string connectionString;
        public AzureStorageService(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("AzureStorageConnection");
        }
        public async Task DeleteFile(string filePath, string containerName)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return;

            var client = new BlobContainerClient(connectionString, containerName);
            await client.CreateIfNotExistsAsync();
            var fileName = Path.GetFileName(filePath);
            var blob = client.GetBlobClient(fileName);
            await blob.DeleteIfExistsAsync();
        }

        public async Task<string> EditFile(string containerName, IFormFile file, string filePath)
        {
            //utility method to delete a file and then create a new one by calling both other methods respectively
            await DeleteFile(filePath, containerName);
            return await SaveFile(containerName, file);
        }

        public async Task<string> SaveFile(string containerName, IFormFile file)
        {
            //create folder if it doesn't exist yet
            var client = new BlobContainerClient(connectionString, containerName);
            await client.CreateIfNotExistsAsync();
            client.SetAccessPolicy(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

            //Generate random name for file and uplaod to Azure Storage
            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var blob = client.GetBlobClient(fileName);
            await blob.UploadAsync(file.OpenReadStream());

            return blob.Uri.ToString();
        }
    }
}

namespace MoviesMaxAPI.Helpers
{
    public interface IFileStorageService
    {
        //a container in Azure Storage is like a folder that allows us group certain files together
        Task DeleteFile(string filePath, string containerName);
        Task<string> SaveFile(string containerName, IFormFile file);
        Task<string> EditFile(string containerName, IFormFile file, string filePath);
    }
}

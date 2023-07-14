using ClientsManagmentAppExample.Models;

namespace ClientsManagmentAppExample.Interfaces
{
    public interface IFileService
    {
        Task UploadFileAsync(IFormFile formFile, FileModel file);
        Task<List<FileModel>> GetProjectFiles(FileModel file);
        //Task DownloadFileAsync (FileModel file);
    }
}

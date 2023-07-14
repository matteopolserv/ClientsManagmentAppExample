using ClientsManagmentAppExample.Interfaces;
using ClientsManagmentAppExample.Models;
using Microsoft.AspNetCore.Http;
using MimeKit;

namespace ClientsManagmentAppExample.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FileService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        //public async Task<File> DownloadFileAsync(FileModel file)
        //{
            
        //}

        public async Task<List<FileModel>> GetProjectFiles(FileModel file)
        {
            List<FileModel> files = new();
            await Task.Run(() =>
            {
                string dirPath = Path.Combine(@"uploadedfiles/" + file.ProjectId + "/" + file.Uploader);
                if (Directory.Exists(dirPath))
                {
                    foreach (string fileName in Directory.GetFiles(dirPath, "*"))
                    {
                        FileModel uploadedFile = new();
                        int c = fileName.LastIndexOf(file.Uploader);
                        c = c + file.Uploader.Length + 1;

                        uploadedFile.FilePath = fileName;
                        uploadedFile.FileName = fileName.Substring(c);
                        uploadedFile.CreationTime = new FileInfo(fileName).CreationTime;

                        files.Add(uploadedFile);
                    }
                }
            });
            return files.OrderByDescending(file => file.CreationTime).ToList();
        }

        public async Task UploadFileAsync(IFormFile formFile, FileModel file)
        {
            if (!Directory.Exists(@"uploadedfiles"))
            {
                Directory.CreateDirectory(@"uploadedfiles");
            }
            if (!Directory.Exists(@"uploadedfiles/" + file.ProjectId))
            {
                Directory.CreateDirectory(@"uploadedfiles/" + file.ProjectId);
                Directory.CreateDirectory(@"uploadedfiles/" + file.ProjectId + "/client");
                Directory.CreateDirectory(@"uploadedfiles/" + file.ProjectId + "/user");
            }
            string filePath = Path.Combine(@"uploadedfiles/" + file.ProjectId + "/" + file.Uploader, formFile.FileName);
            using var fileStream = new FileStream(filePath, FileMode.Create);
            await formFile.CopyToAsync(fileStream);
        }
    }
}

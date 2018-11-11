using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using File = Google.Apis.Drive.v3.Data.File;
using Google.Apis.Download;

namespace DownloadAndUploadPhoto
{
    class Program
    {
        private static string[] Scopes = {DriveService.Scope.Drive};
        private static string ApplicationName = "GoogleDriveAPIStart";
        private static string FolderId = "1dlr5yXRz5KAFonlTzbsnBk739NHXhoQa";  //link to your google drive folder here. Example: drive.google.com/drive/folder/abcdexyz, just put abcdexyz here.
        private static string _fileName = "testFile";
        private static string _filePath = @"D:\Share\HRobotics.rar";

        private static string _contentType = "application/zip";
        private static string _downloadFilePath = @"D:\asdsad.rar";

        static void Main(string[] args)
        {
            Console.WriteLine("Create creds");
            UserCredential credential = GetUserCredential();

            Console.WriteLine("Get service");
            DriveService service = GetDriveService(credential);

            Console.WriteLine("Uploading file(s)");
            var fileId = UploadPhotoToDrive(service, _fileName, _filePath, _contentType);

            Console.WriteLine("Downloading file(s)");
            DownloadPhotoFromDrive(service, fileId, _downloadFilePath);

            Console.WriteLine("End");
            Console.ReadLine();


    }

        private static UserCredential GetUserCredential()
        {
            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, "driveApiCredential", "drive-credentials.json");

                return GoogleWebAuthorizationBroker.AuthorizeAsync
                (
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "User",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)
                ).Result;
            }
        }

        private static DriveService GetDriveService(UserCredential credential)
        {
            return new DriveService
            (
                new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName
                }
            );
        }
        private static string UploadPhotoToDrive(DriveService service, string fileName, string filePath, string contentType)
        {
            var fileMetaData = new File();
            fileMetaData.Name = fileName;
            fileMetaData.Parents = new List<string> { FolderId };
            
            FilesResource.CreateMediaUpload request;
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                request = service.Files.Create(fileMetaData, stream, contentType);
                request.Upload();
            }
            var File = request.ResponseBody;
            return File.Id;
        }
        private static void DownloadPhotoFromDrive(DriveService service, string fileId, string filePath)
        {
            var request = service.Files.Get(fileId);
            using (var memoryStream = new MemoryStream())
            {
                request.MediaDownloader.ProgressChanged += (IDownloadProgress progress) => 
                {
                    switch (progress.Status)
                    {
                        case DownloadStatus.Downloading:
                            Console.WriteLine(progress.BytesDownloaded);
                            break;
                        case DownloadStatus.Completed:
                            Console.WriteLine("Download complete");
                            break;
                        case DownloadStatus.Failed:
                            Console.WriteLine("Download failed");
                            break;
                    }
                };
                request.Download(memoryStream);

                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    fileStream.Write(memoryStream.GetBuffer(), 0, memoryStream.GetBuffer().Length);
                }
            }
        }
    }
}

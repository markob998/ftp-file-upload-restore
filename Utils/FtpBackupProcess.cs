using System;
using System.IO;
using System.Net;
using System.Windows;
using FluentFTP;
using System.Text;

using FtpBackup.Config;

namespace FtpBackup.Utils
{
    class FtpBackupProcess
    {
        public event Action FtpConnected;
        public event Action<string> FtpUploadFolderStarted;
        public event Action<string> FtpUploadedFileStarted;
        public event Action<string> FtpUploadedFileFinished;
        public event Action<string> FtpUploadFolderFinished;
        public event Action<string> FtpRestoreFolderStarted;
        public event Action<string> FtpRestoreFileStarted;
        public event Action<string> FtpRestoreFileFinished;
        public event Action<string> FtpRestoreFolderFinished;
        public FtpBackupProcess()
        { }
        public async Task BackupFiles(List<string> fileList, string remoteDirName = "/")
        {
            try
            {
                string ftpHost;
                ftpHost = FtpServerConfig.FtpHost;

                if (remoteDirName == "/") remoteDirName = "";
                else if (!remoteDirName.StartsWith("/")) remoteDirName = "/" + remoteDirName;

                string remoteDir = $"{ftpHost}/backups{remoteDirName}";

                MessageBox.Show(remoteDir);
                await EnsureDirectoryExists(remoteDir);

                OnFtpConnected();

                foreach (var filePath in fileList)
                {
                    var fileName = Path.GetFileName(filePath);
                    var remotePath = $"{remoteDir}/{fileName}";

                    OnFtpUploadedFileStarted($"from {filePath} to {remotePath}");

                    bool fileExists = await CheckFileExists(remotePath);
                    if (fileExists)
                    {
                        var localLastModified = File.GetLastWriteTimeUtc(filePath);
                        var remoteLastModified = await GetFileModifiedTime(remotePath);
                        if (localLastModified > remoteLastModified)
                        {
                            await UploadFile(filePath, remotePath);
                        }
                    }
                    else
                    {
                        await UploadFile(filePath, remotePath);
                    }
                    OnFtpUploadFolderFinished($"from {filePath} to {remotePath}");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        private async Task<bool> EnsureDirectoryExists(string remoteDir)
        {
            string ftpUser, ftpPass;
            ftpUser = FtpServerConfig.FtpUser;
            ftpPass = FtpServerConfig.FtpPassword;

            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(remoteDir);
                request.Credentials = new NetworkCredential(ftpUser, ftpPass);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                {
                    return true;
                }
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response.StatusCode != FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    throw; // Rethrow if the error is not "directory already exists"
                }
                return false;
            }
        }

        public async Task<bool> CheckFileExists(string remotePath)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(remotePath);
                request.Credentials = new NetworkCredential(FtpServerConfig.FtpUser, FtpServerConfig.FtpPassword);
                request.Method = WebRequestMethods.Ftp.GetDateTimestamp;
                using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                {
                    return true;
                }
            }
            catch (WebException)
            {
                return false;
            }
        }

        public async Task<DateTime> GetFileModifiedTime(string remotePath)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(remotePath);
            request.Credentials = new NetworkCredential(FtpServerConfig.FtpUser, FtpServerConfig.FtpPassword);
            request.Method = WebRequestMethods.Ftp.GetDateTimestamp;
            using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
            {
                return response.LastModified;
            }
        }

        public async Task UploadFile(string filePath, string remotePath)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(remotePath);
            request.Credentials = new NetworkCredential(FtpServerConfig.FtpUser, FtpServerConfig.FtpPassword);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[4096];
                int bytesRead = 0;
                using (Stream requestStream = await request.GetRequestStreamAsync())
                {
                    while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await requestStream.WriteAsync(buffer, 0, bytesRead);
                    }
                }
            }
        }

        public async Task<List<string>> BrowseRemoteFolder(string remoteDirName = "/")
        {
            List<string> remoteDirs = new List<string>();
            string dir = "";
            try
            {
                if (remoteDirName == "/") remoteDirName = "";
                else if (!remoteDirName.StartsWith("/")) remoteDirName = "/" + remoteDirName;

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"{FtpServerConfig.FtpHost}/backups{remoteDirName}");
                request.Credentials = new NetworkCredential(FtpServerConfig.FtpUser, FtpServerConfig.FtpPassword);
                request.Method = WebRequestMethods.Ftp.ListDirectory;

                using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.ASCII))
                {
                    string line = null;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        remoteDirs.Add(line);
                        // if (line.EndsWith("..") || line.EndsWith(".")) continue;
                    }
                    return remoteDirs;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return null;
            }
        }

        public async Task RestoreFolder(string folderPath, string remoteDirName = "/")
        {
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                if (remoteDirName == "/") remoteDirName = "";
                else if (!remoteDirName.StartsWith("/")) remoteDirName = "/" + remoteDirName;

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"{FtpServerConfig.FtpHost}/backups{remoteDirName}");
                request.Credentials = new NetworkCredential(FtpServerConfig.FtpUser, FtpServerConfig.FtpPassword);
                request.Method = WebRequestMethods.Ftp.ListDirectory;

                using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.ASCII))
                {
                    string line = null;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        if (line.EndsWith("..") || line.EndsWith(".")) continue;
                        var remoteFilePath = $"{FtpServerConfig.FtpHost}/{line}";
                        var localFilePath = Path.Combine(folderPath, line);
                        OnFtpRestoreFileStarted(remoteFilePath);
                        await DownloadFile(remoteFilePath, localFilePath);
                        OnFtpRestoreFileFinished(localFilePath);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public async Task DownloadFile(string remotePath, string localPath)
        {
            string directory = Path.GetDirectoryName(localPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(remotePath);
            request.Credentials = new NetworkCredential(FtpServerConfig.FtpUser, FtpServerConfig.FtpPassword);
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
            using (Stream responseStream = response.GetResponseStream())
            using (FileStream fileStream = new FileStream(localPath, FileMode.Create))
            {
                byte[] buffer = new byte[4096];
                int bytesRead = 0;
                while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                }
            }
        }
        private void OnFtpConnected()
        {
            FtpConnected?.Invoke();
        }
        private void OnFtpUploadFolderStarted(string path)
        {
            FtpUploadFolderStarted?.Invoke(path);
        }
        private void OnFtpUploadedFileStarted(string path)
        {
            FtpUploadedFileStarted?.Invoke(path);
        }
        private void OnFtpUploadedFileFinished(string path)
        {
            FtpUploadedFileFinished?.Invoke(path);
        }
        private void OnFtpUploadFolderFinished(string path)
        {
            FtpUploadFolderFinished?.Invoke(path);
        }
        private void OnFtpRestoreFolderStarted(string path)
        {
            FtpRestoreFolderStarted?.Invoke(path);
        }
        private void OnFtpRestoreFileStarted(string path)
        {
            FtpRestoreFileStarted?.Invoke(path);
        }
        private void OnFtpRestoreFileFinished(string path)
        {
            FtpRestoreFileFinished?.Invoke(path);
        }
        private void OnFtpRestoreFolderFinished(string path)
        {
            FtpRestoreFolderFinished?.Invoke(path);
        }
    }
}
using System;
using System.IO;
using System.Net;
using System.Windows;
using FluentFTP;
using System.Text;

using FtpBackup.Config;
using FluentFTP.Helpers;

namespace FtpBackup.Utils;

public class FtpBackupProcess
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
    public event Action FtpFileListEmpty;
    FtpFileTransfor ftpFileTransfor;
    public FtpBackupProcess()
    {
        ftpFileTransfor = new FtpFileTransfor(
            FtpServerConfig.FtpHost,
            FtpServerConfig.FtpUser,
            FtpServerConfig.FtpPassword
        );
    }
    public async Task<List<string>> BrowseRemoteFolder(string remoteDir = "/backups")
    {
        List<string> remotePaths = await ftpFileTransfor.BrowseRemoteFolder(remoteDir);
        return remotePaths;
    }
    public async Task<bool> BackupFiles(List<string> filePathList, string remoteDir = "/backups")
    {
        if (filePathList.Count() == 0)
        {
            OnFtpFileListEmpty();
            return false;
        }
        try
        {
            string ftpHost = FtpServerConfig.FtpHost;
            remoteDir = remoteDir.MakeRegularDirectory();

            OnFtpConnected();
            foreach (var filePath in filePathList)
            {
                var fileName = Path.GetFileName(filePath);
                var remotePathName = $"{remoteDir}/{filePath.Substring(3)}";
                var remoteDirName = $"{remoteDir}/{Path.GetDirectoryName(filePath).Substring(3)}";

                await ftpFileTransfor.CreateDirectory(remoteDirName);
                OnFtpUploadedFileStarted($"from {filePath} to {remotePathName}");
                bool fileExists = await ftpFileTransfor.CheckFileExists(remotePathName);
                if (fileExists)
                {
                    var localLastModified = File.GetLastWriteTimeUtc(filePath);
                    var remoteLastModified = await ftpFileTransfor.GetFileModifiedTime(remotePathName);
                    if (localLastModified <= remoteLastModified) continue;
                }
                await ftpFileTransfor.UploadFile(filePath, remotePathName);
                OnFtpUploadFolderFinished($"from {filePath} to {remotePathName}");
            }
            return true;
        }
        catch (Exception e)
        {
            // MessageBox.Show(e.ToString());
            return false;
        }
    }
    public async Task RestoreFolder(string folderPath, string remoteDir = "/backups")
    {
        try
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            remoteDir = remoteDir.MakeRegularDirectory();

            List<string> remotePathList = await ftpFileTransfor.BrowseRemoteFolder(remoteDir);
            foreach (string remotePath in remotePathList)
            {
                var localFilePath = $"{folderPath}{remotePath}";
                localFilePath = localFilePath.Replace("\\", "/");
                OnFtpRestoreFileStarted(remotePath);
                await ftpFileTransfor.DownloadFile(remotePath, localFilePath);
                OnFtpRestoreFileFinished(localFilePath);
            }
        }
        catch (Exception e)
        {
            // MessageBox.Show(e.ToString());
        }
    }
    public async void ClearFtpServer(string remoteDir = "/backups") {
        await ftpFileTransfor.DeleteDirectory(remoteDir);
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
    private void OnFtpFileListEmpty()
    {
        FtpFileListEmpty?.Invoke();
    }
}
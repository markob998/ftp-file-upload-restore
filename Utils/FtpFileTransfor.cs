using System;
using System.Dynamic;
using System.IO;
using System.IO.Pipes;
using System.Net;
using FluentFTP.Proxy.SyncProxy;
using System.Text;
using FtpBackup.Config;
using System.Windows;

namespace FtpBackup.Utils;

public class FtpFileTransfor
{
    public string FtpHost { get; set; }
    public string FtpUser { get; set; }
    public string FtpPassword { get; set; }
    public FtpFileTransfor(string host, string user, string password)
    {
        FtpHost = host;
        FtpUser = user;
        FtpPassword = password;
    }
    public FtpWebRequest CreateFtpRequest(string remotePath)
    {
        if (!remotePath.StartsWith("ftp://"))
        {
            remotePath = remotePath.MakeRegularDirectory();
            remotePath = $"{FtpHost}{remotePath}";
        }
        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(remotePath);
        request.Credentials = new NetworkCredential(FtpUser, FtpPassword);
        return request;
    }
    public async Task<DateTime> GetFileModifiedTime(string remotePath)
    {
        FtpWebRequest request = CreateFtpRequest(remotePath);
        request.Method = WebRequestMethods.Ftp.GetDateTimestamp;
        using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
        {
            return response.LastModified;
        }
    }
    public async Task<bool> CheckFileExists(string remotePath)
    {
        try
        {
            FtpWebRequest request = CreateFtpRequest(remotePath);
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
    public async Task<bool> CreateDirectory(string remoteDir)
    {
        try
        {
            remoteDir = remoteDir.Replace("\\", "/");
            string[] subDirs = remoteDir.Trim('/').Split('/');
            string currentDir = string.Empty;
            foreach (var subDir in subDirs)
            {
                currentDir = string.IsNullOrEmpty(currentDir) ? subDir : $"{currentDir}/{subDir}";
                if (!await DirectoryExists(currentDir))
                {
                    FtpWebRequest req = CreateFtpRequest(currentDir);
                    req.Method = WebRequestMethods.Ftp.MakeDirectory;
                    try
                    {
                        using (FtpWebResponse response = (FtpWebResponse)await req.GetResponseAsync())
                        {
                            if (response.StatusCode != FtpStatusCode.PathnameCreated && response.StatusCode != FtpStatusCode.CommandOK)
                            {
                                throw new Exception($"Failed to create directory {currentDir}: {response.StatusDescription}");
                            }
                        }
                    }
                    catch (WebException ex)
                    {
                        FtpWebResponse response = (FtpWebResponse)ex.Response;
                        if (response.StatusCode != FtpStatusCode.ActionNotTakenFileUnavailable)
                        {
                            throw;
                        }
                    }
                }
            }
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
    private async Task<bool> DirectoryExists(string remoteDir)
    {
        try
        {
            FtpWebRequest req = CreateFtpRequest(remoteDir);
            req.Method = WebRequestMethods.Ftp.ListDirectory;

            using (FtpWebResponse response = (FtpWebResponse)await req.GetResponseAsync())
            {
                return true;
            }
        }
        catch (WebException ex)
        {
            FtpWebResponse response = (FtpWebResponse)ex.Response;
            if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
            {
                return false;
            }
            throw;
        }
    }
    public async Task<List<string>> BrowseRemoteFolder(string remoteDir = "/backups")
    {
        List<string> remoteDirs = new List<string>();
        try
        {
            remoteDir = remoteDir.MakeRegularDirectory();

            FtpWebRequest request = CreateFtpRequest(remoteDir);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

            using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.ASCII))
            {
                string line = null;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    string[] tokens = line.Split(new[] { ' ' }, 9, StringSplitOptions.RemoveEmptyEntries);
                    string permissions = tokens[0];
                    string name = tokens[8];
                    string fullRemotePath = $"{remoteDir}/{name}";

                    if (permissions.StartsWith("d"))
                    {
                        if (name != "." && name != "..")
                        {
                            remoteDirs.AddRange(await BrowseRemoteFolder(fullRemotePath));
                        }
                    }
                    else
                        remoteDirs.Add(fullRemotePath);
                }
                return remoteDirs;
            }
        }
        catch (Exception e)
        {
            return null;
        }
    }
    public async Task UploadFile(string filePath, string remotePath)
    {
        FtpWebRequest req = CreateFtpRequest(remotePath);
        req.Method = WebRequestMethods.Ftp.UploadFile;
        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            using (Stream requestStream = await req.GetRequestStreamAsync())
            {
                while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await requestStream.WriteAsync(buffer, 0, bytesRead);
                }
            }
        }
    }
    public async Task DownloadFile(string remotePath, string filePath)
    {
        string directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        FtpWebRequest request = CreateFtpRequest(remotePath);
        request.Method = WebRequestMethods.Ftp.DownloadFile;

        using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
        using (Stream responseStream = response.GetResponseStream())
        using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
        {
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead);
            }
        }
    }
}
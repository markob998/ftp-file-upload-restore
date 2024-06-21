using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Microsoft.Win32;

namespace FtpBackup.Config;
public static class CheckFilePath
{
    public static bool IsOfficeFilePath(this string source)
    {
        string pattern = @"^([a-zA-Z]:[\\/]|[\\/]{2})[^\/:*?""<>|]+[\\/](?:[^\/:*?""<>|]+[\\/])*[^\/:*?""<>|]+\.(pdf|doc|docx|xls|xlsx|ppt|pptx|txt|dwg|dxf)$";
        Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
        return regex.IsMatch(source);
    }
    public static List<string> ConvertToStringList(this string source)
    {
        return source.Split("\n").ToList();
    }
    public static string ConvertToString(this List<string> paths)
    {
        string str = "";
        foreach (var path in paths) str = str + path + "\n";
        return str;
    }
    public static string MakeRegularDirectory(this string path)
    {
        if (path == "/") path = "";
        else if (!path.StartsWith("/")) path = "/" + path;
        return path;
    }
}
public class UploadFolderList
{
    private const string registryKey = @"SOFTWARE\FtpBackup\FolderPaths";
    private List<string> folderPaths;
    public List<string> FolderPathList
    {
        get { return folderPaths; }
        set { folderPaths = value; }
    }
    public List<string> AllSubFiles
    {
        get
        {
            List<string> subfiles = new List<string>();
            foreach (var folderPath in folderPaths)
            {
                Console.WriteLine(folderPath);
                foreach (var filePath in Directory.GetFiles(folderPath))
                    subfiles.Add(filePath);
            }
            return subfiles;
        }
    }
    public List<string> AllDocFiles
    {
        get
        {
            return AllSubFiles.Where(subfile => subfile.IsOfficeFilePath()).ToList();
        }
    }
    public UploadFolderList(List<string> folderPaths = null)
    {
        if (folderPaths != null)
            this.folderPaths = folderPaths;
        else this.folderPaths = new List<string>();
    }
    public void AddFolder(string path)
    {
        if (!folderPaths.Contains(path))
            folderPaths.Add(path);
    }
    public void RemoveFolder(string path)
    {
        if (folderPaths.Contains(path))
            folderPaths.Remove(path);
    }
    public void Clear(string path)
    {
        folderPaths.Clear();
    }
    public void SaveToRegistry()
    {
        RegistryKey key = Registry.CurrentUser.CreateSubKey(registryKey);
        if (key != null)
        {
            foreach (var valueName in key.GetValueNames())
            {
                key.DeleteValue(valueName);
            }
            key.SetValue("Count", folderPaths.Count);
            for (int i = 0; i < folderPaths.Count; i++)
            {
                key.SetValue($"Path{i}", folderPaths[i]);
            }
            key.Close();
        }
    }

    public void LoadFromRegistry()
    {
        RegistryKey key = Registry.CurrentUser.OpenSubKey(registryKey);
        if (key != null)
        {
            int count = (int)key.GetValue("Count", 0);
            folderPaths = new List<string>();
            for (int i = 0; i < count; i++)
            {
                string path = (string)key.GetValue($"Path{i}", string.Empty);
                if (!string.IsNullOrEmpty(path))
                {
                    folderPaths.Add(path);
                }
            }
            key.Close();
        }
    }
}
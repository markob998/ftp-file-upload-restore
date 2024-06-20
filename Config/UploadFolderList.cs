using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace FtpBackup.Config
{
    public static class CheckFilePath
    {
        public static bool IsOfficeFilePath(this string source)
        {
            string pattern = @"^([a-zA-Z]:[\\/]|[\\/]{2})[^\/:*?""<>|]+[\\/](?:[^\/:*?""<>|]+[\\/])*[^\/:*?""<>|]+\.(pdf|doc|docx|xls|xlsx|ppt|pptx|txt)$";
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
    }
    public class UploadFolderList
    {
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
    }
}
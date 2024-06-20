using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace FtpBackup.Utils
{
    public class FolderBrowser
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class BrowseInfo
        {
            public IntPtr hwndOwner;
            public IntPtr pidlRoot;
            public IntPtr pszDisplayName;
            public string lpszTitle;
            public uint ulFlags;
            public IntPtr lpfn;
            public IntPtr lParam;
            public int iImage;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetActiveWindow();

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SHBrowseForFolder([In] BrowseInfo lpbi);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern bool SHGetPathFromIDList(IntPtr pidl, [Out] StringBuilder pszPath);

        public static string SelectFolder(string title)
        {
            var bi = new BrowseInfo
            {
                hwndOwner = GetActiveWindow(),
                lpszTitle = title,
                ulFlags = 0x00000040 // BIF_NEWDIALOGSTYLE
            };

            IntPtr pidl = SHBrowseForFolder(bi);

            if (pidl != IntPtr.Zero)
            {
                var path = new StringBuilder(260);
                if (SHGetPathFromIDList(pidl, path))
                {
                    return path.ToString();
                }
            }

            return null;
        }
    }
}


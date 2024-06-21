using Microsoft.Win32;

namespace FtpBackup.Config;
class FtpServerConfig
{
    private static string registryKey = @"SOFTWARE\FtpBackup\FtpServerConfig";
    private static string ftpHost;
    private static string ftpUser;
    private static string ftpPass;
    public static string FtpHost
    {
        get { return ftpHost; }
        set
        {
            if (value.StartsWith("ftp://")) ftpHost = value;
            else ftpHost = $"ftp://{value}";
        }
    }
    public static string FtpUser
    {
        get { return ftpUser; }
        set { ftpUser = value; }
    }
    public static string FtpPassword
    {
        get { return ftpPass; }
        set { ftpPass = value; }
    }
    public static void SaveToRegistry()
    {
        
        RegistryKey key = Registry.CurrentUser.CreateSubKey(registryKey);
        if (key != null)
        {
            foreach (var valueName in key.GetValueNames())
            {
                key.DeleteValue(valueName);
            }
            key.SetValue($"host", FtpHost);
            key.SetValue($"user", FtpUser);
            key.SetValue($"pass", FtpPassword);
            key.Close();
        }
    }

    public static bool LoadFromRegistry()
    {
        RegistryKey key = Registry.CurrentUser.OpenSubKey(registryKey);
        if (key != null)
        {
            FtpHost = (string)key.GetValue($"host", string.Empty);
            FtpUser = (string)key.GetValue($"user", string.Empty);
            FtpPassword = (string)key.GetValue($"passs", string.Empty);
            key.Close();
            return true;
        }
        return false;
    }
}


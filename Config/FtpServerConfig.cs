namespace FtpBackup.Config
{
    class FtpServerConfig
    {
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
    }
}
using System.Configuration;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using FtpBackup.Config;
using FtpBackup.Utils;

namespace FtpBackup.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private string ftpHost = "ftp.primariagradisteavalcea.ro";
    private string ftpUser = "dan@primariagradisteavalcea.ro";
    private string ftpPass = "jT4*000000";
    public FtpBackupProcess ftpBackupProcess;
    public UploadFolderList folderList;
    public static MainWindow Instance
    {
        get; private set;
    }
    public MainWindow()
    {
        InitializeComponent();

        FtpServerConfig.FtpHost = ftpHost;
        FtpServerConfig.FtpUser = ftpUser;
        FtpServerConfig.FtpPassword = ftpPass;

        ftpBackupProcess = new FtpBackupProcess();
        ftpBackupProcess.FtpConnected += OnFtpConnected;
        ftpBackupProcess.FtpUploadFolderStarted += OnFtpUploadFolderStarted;
        ftpBackupProcess.FtpUploadedFileStarted += OnFtpUploadedFileStarted;
        ftpBackupProcess.FtpUploadedFileFinished += OnFtpUploadedFileFinished;
        ftpBackupProcess.FtpUploadFolderFinished += OnFtpUploadFolderFinished;
        ftpBackupProcess.FtpRestoreFolderStarted += OnFtpRestoreFolderStarted;
        ftpBackupProcess.FtpRestoreFileStarted += OnFtpRestoreFileStarted;
        ftpBackupProcess.FtpRestoreFileFinished += OnFtpRestoreFileFinished;
        ftpBackupProcess.FtpRestoreFolderFinished += OnFtpRestoreFolderFinished;
        ftpBackupProcess.FtpFileListEmpty += OnFtpFileListEmpty;

        folderList = new UploadFolderList();
        folderList.LoadFromRegistry();

        Instance = this;
        Log(title: "Application Started");
    }
    private void OnFtpFileListEmpty()
    {
        Log("There is no Upload files. Please check you have inserted the folders or they contains office file", "File List Empty");
    }
    private void OnFtpRestoreFolderStarted(string path)
    {
        Log(path, "Ftp Restore Folder Started!");
    }
    private void OnFtpRestoreFileStarted(string path)
    {
        Log(path, "Ftp Restore File Started!");
    }
    private void OnFtpRestoreFileFinished(string path)
    {
        Log(path, "Ftp Restore File Finished!");
    }
    private void OnFtpRestoreFolderFinished(string path)
    {
        Log(path, "Ftp Restore Folder Finished!");
    }
    private void OnFtpConnected()
    {
        Log(title: "Ftp Server Connected!\n");
    }
    private void OnFtpUploadFolderStarted(string path)
    {
        Log(path, "Ftp Upload Folder Started!");
    }
    private void OnFtpUploadedFileStarted(string path)
    {
        Log(path, "Ftp Upload File Started!");
    }
    private void OnFtpUploadedFileFinished(string path)
    {
        Log(path, "Ftp Upload File Completed!");
    }
    private void OnFtpUploadFolderFinished(string path)
    {
        Log(path, "Ftp Upload Folder Finished!");
    }
    public void SelectFolder_Click(object sender, RoutedEventArgs e)
    {
        string folderPath = FolderBrowser.SelectFolder("Select a folder");
        if (!string.IsNullOrEmpty(folderPath))
        {
            AddFolder(folderPath);
        }
    }
    public void Log(string str = "", string title = "")
    {
        if (title == "") title = "Notification";
        title = $"\n********************  {title}  ********************\n";
        Dispatcher.Invoke(() =>
        {
            tbxLog.Text += (title + str + "\n");
        });
    }
    private void TxtLog_TextChanged(object sender, TextChangedEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            tbxLog.ScrollToEnd();
        });
    }
    private async void BackupNow_Click(object sender, RoutedEventArgs e)
    {
        Log(title: "Backup Started!");
        if (await ftpBackupProcess.BackupFiles(folderList.AllDocFiles))
            Log(title: "Backup Completed!");
    }
    private async void RemoteBrowse_Click(object sender, RoutedEventArgs e)
    {
        Log(title: "Browse Remote Server");
        List<string> remotePaths = await ftpBackupProcess.BrowseRemoteFolder("/backups");
        string str = remotePaths.ConvertToString();
        Log(string.IsNullOrEmpty(str) ? "No Files ..." : str);
        Log(title: "Finished Browse Remote Server");
    }
    private async void Restore_Click(object sender, RoutedEventArgs e)
    {
        string folderPath = FolderBrowser.SelectFolder("Select a download folder.");
        Log(title: "Restore Started!");
        if (!string.IsNullOrEmpty(folderPath))
        {
            await ftpBackupProcess.RestoreFolder(folderPath);
        }
        Log(title: "Restore Completed!");
    }
    private async void ScheduleBackup_Click(object sender, RoutedEventArgs e)
    {
        Log(title: "Daily Schedule Set!");
        await BackupScheduler.ScheduleDailyBackup(folderList.AllDocFiles);
    }
    private void Configure_Click(object sender, RoutedEventArgs e)
    {
        ConfigureDialog dialog = new ConfigureDialog();
        dialog.ShowDialog();
    }
    public void AddFolder(string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            folderList.AddFolder(path);
            Log(path, "Local Folder Added");
            Log(folderList.AllDocFiles.ConvertToString(), title: "Sub Office Files");
        }
    }
    public void RemoveFolder(string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            folderList.RemoveFolder(path);
            Log(path, "Local Folder Removed");
            Log(folderList.AllDocFiles.ConvertToString(), title: "Sub Office Files");
        }
    }
    protected override void OnClosed(EventArgs e)
    {
        folderList.SaveToRegistry();
        base.OnClosed(e);
    }
}
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
    private FtpBackupProcess ftpBackupProcess;
    public UploadFolderList folderList;
    public static MainWindow Instance
    {
        get; private set;
    }
    public MainWindow()
    {
        InitializeComponent();

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

        FtpServerConfig.FtpHost = ftpHost;
        FtpServerConfig.FtpUser = ftpUser;
        FtpServerConfig.FtpPassword = ftpPass;

        folderList = new UploadFolderList();

        Instance = this;
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
        Log(title: "Ftp Connection completed!\n");
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
    private void Log(string str = "", string title = "")
    {
        title = $"\n********************  {title}  ********************\n";
        tbxLog.Text = tbxLog.Text + title + str + "\n";
    }
    private void TxtLog_TextChanged(object sender, TextChangedEventArgs e)
    {
        tbxLog.ScrollToEnd();
    }
    private async void BackupNow_Click(object sender, RoutedEventArgs e)
    {
        Log(title: "Backup Started!");
        await ftpBackupProcess.BackupFiles(folderList.AllDocFiles);
        Log(title: "Backup Competed!");
    }
    private async void RemoteBrowse_Click(object sender, RoutedEventArgs e)
    {
        tbxLog.Text = tbxLog.Text + "starting browse ...\n";
        List<string> remotePaths = await ftpBackupProcess.BrowseRemoteFolder("/");
        string str = "";
        if (remotePaths != null)
        {
            foreach (string remotePath in remotePaths)
            {
                str = str + remotePath + "\n";
            }
        }
        else str = "no files ...\n";
        tbxLog.Text = tbxLog.Text + str;
        tbxLog.Text = tbxLog.Text + "finishing browse ...\n";
    }
    private async void Restore_Click(object sender, RoutedEventArgs e)
    {
        string folderPath = FolderBrowser.SelectFolder("Select a download folder.");
        if (!string.IsNullOrEmpty(folderPath))
        {
            await ftpBackupProcess.RestoreFolder(folderPath);
        }
        MessageBox.Show("Restore completed!");
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
            Log(folderList.AllDocFiles.ConvertToStringPath(), title: "Sub Office Files");
        }
    }
    public void RemoveFolder(string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            folderList.RemoveFolder(path);
            Log(path, "Local Folder Removed");
            Log(folderList.AllDocFiles.ConvertToStringPath(), title: "Sub Office Files");
        }
    }
}
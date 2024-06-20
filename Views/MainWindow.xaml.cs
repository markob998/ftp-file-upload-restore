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

using FtpBackup.Consts;
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
    }
    private void OnFtpRestoreFolderStarted(string path)
    {
        tbxLog.Text = tbxLog.Text + $"{path} : Ftp Restore Folder Started!\n";
    }
    private void OnFtpRestoreFileStarted(string path)
    {
        tbxLog.Text = tbxLog.Text + $"{path} : Ftp Restore File Started!\n";
    }
    private void OnFtpRestoreFileFinished(string path)
    {
        tbxLog.Text = tbxLog.Text + $"{path} : Ftp Restore File Finished!\n";
    }
    private void OnFtpRestoreFolderFinished(string path)
    {
        tbxLog.Text = tbxLog.Text + $"{path} : Ftp Restore Folder Finished!\n";
    }
    private void OnFtpConnected()
    {
        tbxLog.Text = tbxLog.Text + "Ftp Connection completed!\n";
    }
    private void OnFtpUploadFolderStarted(string path)
    {
        tbxLog.Text = tbxLog.Text + $"{path} : Ftp Upload Folder Started!\n";
    }
    private void OnFtpUploadedFileStarted(string path)
    {
        tbxLog.Text = tbxLog.Text + $"{path} : Ftp Upload File Started!\n";
    }
    private void OnFtpUploadedFileFinished(string path)
    {
        tbxLog.Text = tbxLog.Text + $"{path} : Ftp Upload File Completed!\n";
    }
    private void OnFtpUploadFolderFinished(string path)
    {
        tbxLog.Text = tbxLog.Text + $"{path} : Ftp Upload Folder Finished!\n";
    }
    private void SelectFolder_Click(object sender, RoutedEventArgs e)
    {
        string folderPath = FolderBrowser.SelectFolder("Select a folder");
        if (!string.IsNullOrEmpty(folderPath))
        {
            tbxFolderPath.Text = folderPath;
        }
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
    private async void BackupNow_Click(object sender, RoutedEventArgs e)
    {
        await ftpBackupProcess.BackupFolder(tbxFolderPath.Text);
        MessageBox.Show("Backup completed!");
    }
    private async void ScheduleBackup_Click(object sender, RoutedEventArgs e)
    {
        await BackupScheduler.ScheduleDailyBackup(tbxFolderPath.Text);
        MessageBox.Show("Daily backup scheduled!");
    }
    private async void Restore_Click(object sender, RoutedEventArgs e)
    {
        await ftpBackupProcess.RestoreFolder(tbxFolderPath.Text);
        MessageBox.Show("Restore completed!");
    }
    private void Configure_Click(object sender, RoutedEventArgs e)
    {
        ConfigureDialog dialog = new ConfigureDialog();
        dialog.ShowDialog();
    }
}
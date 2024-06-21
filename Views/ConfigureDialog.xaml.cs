using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
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
public partial class ConfigureDialog : Window
{
    private bool configureChanged = false;
    private ObservableCollection<string> paths = new ObservableCollection<string>();
    public ConfigureDialog()
    {
        InitializeComponent();
        tbxFtpHost.Text = FtpServerConfig.FtpHost;
        tbxFtpUser.Text = FtpServerConfig.FtpUser;
        tbxFtpPass.Text = FtpServerConfig.FtpPassword;

        foreach (var path in MainWindow.Instance.folderList.FolderPathList)
            paths.Add(path);
        lbxFolderPath.ItemsSource = paths;
    }
    private void AddFolder_Clicked(object sender, RoutedEventArgs e)
    {
        MainWindow.Instance.SelectFolder_Click(sender, e);
        paths.Add(MainWindow.Instance.folderList.FolderPathList.Last());
    }
    private void RemoveFolder_Clicked(object sender, RoutedEventArgs e)
    {
        var selectedItem = lbxFolderPath.SelectedItem;

        if (selectedItem != null)
        {
            MainWindow.Instance.RemoveFolder(selectedItem.ToString());
            paths.Remove(selectedItem.ToString());
        }
        else
        {
            MessageBox.Show("No item selected.");
        }

    }
    private void Save_Clicked(object sender, RoutedEventArgs e)
    {
        FtpServerConfig.FtpHost = tbxFtpHost.Text;
        FtpServerConfig.FtpUser = tbxFtpUser.Text;
        FtpServerConfig.FtpPassword = tbxFtpPass.Text;
        this.Close();
    }
    private void Close_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
    private void ConfigChanged(object sender, TextChangedEventArgs e)
    {
        btnSave.IsEnabled = true;
    }
    
    private async void ClearFtpServer_Clicked(object sender, RoutedEventArgs e)
    {
        await MainWindow.Instance.ftpBackupProcess.ClearFtpServer();
    }
}
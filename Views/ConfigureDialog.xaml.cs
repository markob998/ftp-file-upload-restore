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
public partial class ConfigureDialog : Window
{
    public ConfigureDialog()
    {
        InitializeComponent();
        tbxFtpHost.Text = FtpServerConfig.FtpHost;
        tbxFtpUser.Text = FtpServerConfig.FtpUser;
        tbxFtpPass.Text = FtpServerConfig.FtpPassword;
    }
    private void Save_Clicked(object sender, RoutedEventArgs e)
    {
        FtpServerConfig.FtpHost = tbxFtpHost.Text;
        FtpServerConfig.FtpUser = tbxFtpUser.Text;
        FtpServerConfig.FtpPassword = tbxFtpPass.Text;
    }
    private void Close_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
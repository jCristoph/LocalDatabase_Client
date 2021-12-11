
using LocalDatabase_Client.Client;
using LocalDatabase_Client.Ui.CustomClasses.PlaceholderTextBox;
using System.Text.RegularExpressions;
using System.Windows;

namespace LocalDatabase_Client.Panels.Settings
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            SetServerHint();
            SetPort();
        }

        private void SetServerHint()
        {
            string serverIp = SettingsManager.Instance.GetServerIp();
            PlaceholderTextBox.Placeholder(serverIpText, serverIp);
        }

        private void SetPort()
        {
            string port = SettingsManager.Instance.GetPort().ToString();
            PlaceholderTextBox.Placeholder(portText, port);
        }

        private void changeServerIpButton_Click(object sender, RoutedEventArgs e)
        {
            string serverIp = serverIpText.Text;
            if (string.IsNullOrEmpty(serverIp))
            {
                ShowMessagePanel("Value can not be empty");
                return;
            }

            if (!IsValidateIP(serverIp))
            {
                ShowMessagePanel("Ip address is not in correct format");
                return;
            }

            SettingsManager.Instance.SetServerIp(serverIp);
            ShowMessagePanel();
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private bool IsValidateIP(string Address)
        {
            string Pattern = @"^([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])(\.([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])){3}$";
            Regex check = new Regex(Pattern);

            return check.IsMatch(Address, 0);
        }

        private void ShowMessagePanel(string message = "Changes applied")
        {
            SetServerHint();
            MessagePanel.MessagePanel messagePanel = new MessagePanel.MessagePanel(message, false);
            messagePanel.ShowDialog();
        }

        private void changePortButton_Click(object sender, RoutedEventArgs e)
        {
            string port = portText.Text;
            if (string.IsNullOrEmpty(port))
            {
                ShowMessagePanel("Value can not be empty");
                return;
            }

            if (int.TryParse(port, out int PortInt))
            {
                SettingsManager.Instance.SetPort(PortInt);
                ShowMessagePanel();
            }
            else
            {
                ShowMessagePanel("Value is not a number");
            }
        }
    }
}

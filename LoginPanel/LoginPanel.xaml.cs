using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LocalDatabase_Client.LoginPanel
{
    /// <summary>
    /// Logika interakcji dla klasy LoginPanel.xaml
    /// </summary>
    public partial class LoginPanel : Window
    {
        private TcpClient client;
        private ClientConnection cc;

        public LoginPanel()
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeComponent();
            Task t = new Task(() => Connection());
            t.Start();
        }

        private void Connection()
        {
            cc = new ClientConnection("127.0.0.1");
            client = cc.Start();
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            if (client == null)
            {
                MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Błąd połączenia z serwerem", false);
                mp.ShowDialog();
            }
            else
            {
                if (client.Connected)
                {
                    cc.sendMessage(ClientCom.LoginMessage(textBoxLogin.Text, passwordBoxPassword.Password), client);
                    if (cc.readMessage(client) == 1)
                    {
                        bool isPasswordChanged = false;
                        MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Zalogowano", false);
                        mp.ShowDialog();
                        if (cc.token.Equals(passwordBoxPassword.Password))
                            isPasswordChanged = true;
                        MainWindow mw = new MainWindow(client, cc, isPasswordChanged);
                        textBoxLogin.Text = "";
                        passwordBoxPassword.Password = "";
                        mw.Owner = this;
                        this.Hide();
                        mw.Show();
                    }
                    else
                    {
                        MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Złe hasło lub nazwa użytkownika", false);
                        mp.ShowDialog();
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
